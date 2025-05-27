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
        float desert = WorldGen.GetForestWeight(x, z);
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
        int ground_height = ground_heights[block_pos.z * chunk_size + block_pos.x];
        int biomes_idx = block_pos.z * chunk_size + block_pos.x;

        if (block_height < ground_height) { blocks[idx] = (biomes[biomes_idx] == 0) ? (byte)3 : (byte)4; }
        else if (block_height == ground_height) { blocks[idx] = (biomes[biomes_idx] == 0) ? (byte)2 : (byte)4; }

    }

}