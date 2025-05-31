using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkData {

    public bool edited;
    public int3 position;
    public NativeArray<int> blocks;

    public ChunkData(int3 _position) {
        edited = false;
        position = _position;
        blocks = new NativeArray<int>(32768, Allocator.Persistent);
    }

    public void Dispose() {
        blocks.Dispose();
    }

}