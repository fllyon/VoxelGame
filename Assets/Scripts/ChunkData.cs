using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct ChunkData {

    public static int CHUNK_SIZE = 32;

    public int3 position;
    public NativeArray<byte> blocks;

    public ChunkData(int3 _position) {
        position = _position;
        blocks = new NativeArray<byte>(CHUNK_SIZE.Cubed(), Allocator.Persistent);
        PopulateBlocks();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    public void Dispose() {
        if (blocks.IsCreated) { blocks.Dispose(); }
    }

    private void PopulateBlocks() {

        NativeArray<int> heights = new NativeArray<int>(CHUNK_SIZE.Squared(), Allocator.Persistent, NativeArrayOptions.ClearMemory);
        NativeArray<int> biomes = new NativeArray<int>(CHUNK_SIZE.Squared(), Allocator.Persistent, NativeArrayOptions.ClearMemory);

        ChunkWideNoiseJob noise_job = new ChunkWideNoiseJob {
            chunk_size = CHUNK_SIZE,
            chunk_pos = position,
            ground_heights = heights,
            biomes = biomes
        };

        JobHandle noise_handle = noise_job.Schedule();
        noise_handle.Complete();

        ChunkWideGenerateJob generate_job = new ChunkWideGenerateJob {
            chunk_size = CHUNK_SIZE,
            chunk_pos = position,
            ground_heights = heights,
            biomes = biomes,
            blocks = blocks
        };

        JobHandle generation_handle = generate_job.Schedule();
        generation_handle.Complete();

        heights.Dispose();
        biomes.Dispose();
    }

}