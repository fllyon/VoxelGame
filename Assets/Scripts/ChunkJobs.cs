using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkNoiseJob : IJobParallelFor {

    [ReadOnly] public int chunk_size;
    [ReadOnly] public int3 chunk_pos;
    public NativeArray<int> ground_heights;

    public void Execute(int idx) {

        int block_x = idx % chunk_size;
        int block_z = idx / chunk_size;
        ground_heights[idx] = WorldGen.GetSurfaceHeight(chunk_pos.z + block_z, chunk_pos.x + block_x);

    }

}

[BurstCompile]
public struct ChunkGenerateJob : IJobParallelFor {

    [ReadOnly] public int chunk_size;
    [ReadOnly] public int3 chunk_pos;
    [ReadOnly] public NativeArray<int> ground_heights;
    public NativeArray<byte> blocks;

    public void Execute(int idx) {

        int3 block_pos = idx.Unflatten();
        int block_height = chunk_pos.y + block_pos.y;
        int ground_height = ground_heights[block_pos.z * chunk_size + block_pos.x];

        if (block_height < ground_height) { blocks[idx] = 3; }
        else if (block_height == ground_height) { blocks[idx] = 2; }

    }

}