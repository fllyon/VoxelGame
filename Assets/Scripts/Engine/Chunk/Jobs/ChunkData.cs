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

    public void Dispose() {
        blocks.Dispose();
    }

}