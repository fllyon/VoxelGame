using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkNoiseJob : IJobParallelFor {

    [ReadOnly] public int chunk_size;
    [ReadOnly] public int3 chunk_pos;
    public NativeArray<int> ground_heights;
    public NativeArray<int> biomes;

    public void Execute(int idx) {

        int block_x = idx % chunk_size;
        int block_z = idx / chunk_size;
        int x = chunk_pos.x + block_x;
        int z = chunk_pos.z + block_z;

        float forest = WorldGen.GetForestWeight(x, z);
        float desert = WorldGen.GetDesertWeight(x, z);
        float diff = math.abs(forest - desert);

        if (diff < 0.05f) {

            diff *= 10;
            float upper = 0.5f + diff;
            float lower = 0.5f - diff;
            upper *= upper *= upper;
            lower *= lower *= lower;

            if (desert > forest) { 
                desert = upper / (upper + lower);
                forest = lower / (upper + lower);
            } else {
                forest = upper / (upper + lower);
                desert = lower / (upper + lower);
            }

            int forest_surface = WorldGen.GetSurfaceHeight(x, z);
            int desert_surface = WorldGen.GetDesertSurfaceHeight(x, z);

            ground_heights[idx] = (int)(forest_surface * forest + desert_surface * desert);
        } else if (desert > forest) { 
            ground_heights[idx] = WorldGen.GetDesertSurfaceHeight(x, z); 
        } else { 
            ground_heights[idx] = WorldGen.GetSurfaceHeight(x, z);
        }

        biomes[idx] = (desert > forest) ? 1 : 0;
    }

}

[BurstCompile]
public struct ChunkGenerateJob : IJobParallelFor {

    [ReadOnly] public int chunk_size;
    [ReadOnly] public int3 chunk_pos;
    [ReadOnly] public NativeArray<int> ground_heights;
    [ReadOnly] public NativeArray<int> biomes;
    public NativeArray<byte> blocks;

    public void Execute(int idx) {

        int3 block_pos = idx.Unflatten();
        int block_height = chunk_pos.y + block_pos.y;
        if (block_height == 0) { blocks[idx] = 1; return; }

        int ground_height = ground_heights[block_pos.z * chunk_size + block_pos.x];
        int biomes_idx = block_pos.z * chunk_size + block_pos.x;

        if (block_height < (ground_height >> 2)) { blocks[idx] = 6; }
        else if (block_height < (ground_height >> 1)) { blocks[idx] = 5; }
        else if (block_height < ground_height - 6) { blocks[idx] = 4; }
        else if (block_height < ground_height) { blocks[idx] = (biomes[biomes_idx] == 0) ? (byte)3 : (byte)7; }
        else if (block_height == ground_height) { blocks[idx] = (biomes[biomes_idx] == 0) ? (byte)2 : (byte)7; }

    }

}

[BurstCompile]
public struct ChunkDrawJob : IJob {

    [ReadOnly] public int chunk_size;
    [ReadOnly] public NativeArray<byte> blocks;
    [ReadOnly] public NativeArray<byte> nbr_blocks;
    [ReadOnly] public NativeArray<int> face_data;
    public NativeList<float3> vertices; // Initialize to size 32,768
    public NativeList<float3> normals;
    public NativeList<float2> uvs;
    public NativeList<int> triangles;

    public void Execute() {

        int vertex_count = 0;

        NativeArray<int3> dirs = new NativeArray<int3>(6, Allocator.Temp);
        dirs[0] = new int3(0, 1, 0);
        dirs[1] = new int3(0, 0, 1);
        dirs[2] = new int3(1, 0, 0);
        dirs[3] = new int3(0, 0, -1);
        dirs[4] = new int3(-1, 0, 0);
        dirs[5] = new int3(0, -1, 0);

        NativeArray<int3> verts = new NativeArray<int3>(24, Allocator.Temp);
        verts[0] = new int3(0, 1, 0); verts[1] = new int3(0, 1, 1); verts[2] = new int3(1, 1, 1); verts[3] = new int3(1, 1, 0);
        verts[4] = new int3(0, 0, 1); verts[5] = new int3(1, 0, 1); verts[6] = new int3(1, 1, 1); verts[7] = new int3(0, 1, 1);
        verts[8] = new int3(1, 0, 1); verts[9] = new int3(1, 0, 0); verts[10] = new int3(1, 1, 0); verts[11] = new int3(1, 1, 1);
        verts[12] = new int3(1, 0, 0); verts[13] = new int3(0, 0, 0); verts[14] = new int3(0, 1, 0); verts[15] = new int3(1, 1, 0);
        verts[16] = new int3(0, 0, 0); verts[17] = new int3(0, 0, 1); verts[18] = new int3(0, 1, 1); verts[19] = new int3(0, 1, 0);
        verts[20] = new int3(0, 0, 0); verts[21] = new int3(1, 0, 0); verts[22] = new int3(1, 0, 1); verts[23] = new int3(0, 0, 1);

        int blocks_per_chunk = chunk_size.Cubed();

        int idx = 0;
        for (int x = 0; x < chunk_size; ++x) {
            for (int z = 0; z < chunk_size; ++z) {
                for (int y = 0; y < chunk_size; ++y) {
                    
                    byte _block_type = blocks[idx++];
                    if (_block_type == 0) { continue; }

                    int3 _block_pos = new int3(x, y, z);
                    for (int face = 0; face < 6; ++face) {
                        
                        int3 _nbr_pos = _block_pos + dirs[face];
                        byte nbr_type = (0 <= _nbr_pos.x && _nbr_pos.x < 32 &&
                                         0 <= _nbr_pos.y && _nbr_pos.y < 32 &&
                                         0 <= _nbr_pos.z && _nbr_pos.z < 32)
                                            ? blocks[_nbr_pos.Flatten()]
                                            : nbr_blocks[face * blocks_per_chunk + _nbr_pos.GetLocalPos().Flatten()];

                        if (nbr_type != 0) { continue; }

                        vertices.Add(_block_pos + verts[face * 4]);
                        vertices.Add(_block_pos + verts[face * 4 + 1]);
                        vertices.Add(_block_pos + verts[face * 4 + 2]);
                        vertices.Add(_block_pos + verts[face * 4 + 3]);

                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);

                        int texture_idx = face_data[_block_type * 6 + face];
                        float2 base_uv = new float2(0.0625f * (texture_idx % 16), 1.0f - (texture_idx / 16) - 0.0625f);

                        uvs.Add(base_uv);
                        uvs.Add(base_uv + new float2(0.0625f, 0));
                        uvs.Add(base_uv + new float2(0.0625f, 0.0625f));
                        uvs.Add(base_uv + new float2(0, 0.0625f));

                        triangles.Add(vertex_count);
                        triangles.Add(vertex_count + 1);
                        triangles.Add(vertex_count + 2);
                        triangles.Add(vertex_count + 2);
                        triangles.Add(vertex_count + 3);
                        triangles.Add(vertex_count);

                        vertex_count += 4;
                    }

                }
            }
        }

    }

}