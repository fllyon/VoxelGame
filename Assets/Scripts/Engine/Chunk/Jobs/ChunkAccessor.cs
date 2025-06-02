using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ChunkAccessor {

    public NativeHashMap<int3, ChunkData> chunk_data;

    public ChunkAccessor(NativeArray<int3> chunk_coords, ref NativeParallelHashMap<int3, ChunkData> chunks) {

        chunk_data = new NativeHashMap<int3, ChunkData>(chunk_coords.Length * 7, Allocator.Persistent);
        foreach (int3 chunk_coord in chunk_coords) {
            for (int idx = 0; idx < Utility.self_dirs.Length; ++idx) {
                int3 coord = chunk_coord + Utility.self_dirs[idx];
                if (chunk_data.ContainsKey(coord)) { continue; }
                chunk_data[coord] = chunks[coord];
            }
        }

    }

    public void Dispose() {
        chunk_data.Dispose();
    }
}