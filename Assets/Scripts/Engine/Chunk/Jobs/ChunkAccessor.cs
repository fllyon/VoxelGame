using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ChunkAccessor {

    public readonly bool IsCreated => chunk_data.IsCreated;
    public NativeParallelHashMap<int3, ChunkData> chunk_data;

    public ChunkAccessor(NativeArray<int3> chunk_coords, ref NativeParallelHashMap<int3, CompressedChunkData> chunks) {

        chunk_data = new NativeParallelHashMap<int3, ChunkData>(chunk_coords.Length * 7, Allocator.Persistent);
        foreach (int3 chunk_coord in chunk_coords) {
            for (int idx = 0; idx < Utility.self_dirs.Length; ++idx) {
                int3 coord = chunk_coord + Utility.self_dirs[idx];
                if (chunk_data.ContainsKey(coord)) { continue; }
                if (!Utility.ChunkInWorld(coord)) { chunk_data[coord] = new ChunkData(coord); }
                if (!chunks.ContainsKey(coord)) { continue; }
                chunk_data[coord] = new ChunkData(chunks[coord]);
            }
        }

    }

    public void Dispose() {
        foreach (var pair in chunk_data) { pair.Value.Dispose(); }
        chunk_data.Dispose();
    }
}