using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkData {

    public bool edited;
    public int3 position;
    public UnsafeList<int> blocks;

    public ChunkData(int3 _position) {
        edited = false;
        position = _position;
        blocks = new UnsafeList<int>(32768, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        blocks.Length = 32768;
    }

    public ChunkData(CompressedChunkData compressed) {
        edited = compressed.edited;
        position = compressed.position;
        blocks = new UnsafeList<int>(32768, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        blocks.Length = 32768;

        int idx = 0;
        foreach (Run run in compressed.block_data.blocks) {
            for (int r = 0; r < run.length; ++r) {
                blocks[idx++] = run.block_type;
            }
        }
    }

    public void Dispose() {
        blocks.Dispose();
    }

}

[BurstCompile]
public struct CompressedChunkData {

    public bool edited;
    public int3 position;
    public UnsafeRunList block_data;

    public CompressedChunkData(ChunkData chunk_data) {
        edited = chunk_data.edited;
        position = chunk_data.position;
        block_data = new UnsafeRunList(chunk_data.blocks);
    }

    public void Dispose() {
        block_data.Dispose();
    }

}

[BurstCompile]
public struct UnsafeRunList {
    public UnsafeList<Run> blocks;

    public UnsafeRunList(UnsafeList<int> list) {
        blocks = new UnsafeList<Run>(1, Allocator.Persistent);

        int run = 1;
        int curr = list[0];

        for (int idx = 1; idx < list.Length; ++idx) {

            if (list[idx] == curr) { 
                run++;
            } else {
                blocks.Add(new Run(curr, run));
                curr = list[idx];
                run = 1;
            }

            

        }

        blocks.Add(new Run(curr, run));

    }

    public void Dispose() {
        blocks.Dispose();
    }
}

[BurstCompile]
public struct Run {
    public int block_type;
    public int length;

    public Run(int _block_type, int _length) {
        block_type = _block_type;
        length = _length;
    }
}