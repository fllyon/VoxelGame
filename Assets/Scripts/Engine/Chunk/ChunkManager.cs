using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ChunkManager {

    public ChunkScheduler chunk_scheduler;

    private Dictionary<int3, Chunk> chunk_components;
    private NativeParallelHashMap<int3, ChunkData> chunk_data;

    public ChunkManager() {
        chunk_data = new NativeParallelHashMap<int3, ChunkData>(WorldSettings.LOAD_CONTAINER_SIZE, Allocator.Persistent);
        chunk_components = new Dictionary<int3, Chunk>(WorldSettings.RENDER_CONTAINER_SIZE);
    }

    // ============================================================= //
    //                        Public Methods                         //
    // ============================================================= //
    
    public void UpdatePlayerChunk(int3 player_chunk) {

        int load_range = WorldSettings.LOAD_DISTANCE;
        int load_distance = WorldSettings.LOAD_DISTANCE.Squared();
        int render_distance = WorldSettings.RENDER_DISTANCE.Squared();

        int min_x = math.max(player_chunk.x - load_range, 0);
        int min_y = math.max(player_chunk.y - load_range, 0);
        int min_z = math.max(player_chunk.z - load_range, 0);
        int max_x = math.min(player_chunk.x + load_range, WorldSettings.WORLD_SIZE_IN_CHUNKS);
        int max_y = math.min(player_chunk.y + load_range, WorldSettings.WORLD_HEIGHT_IN_CHUNKS);
        int max_z = math.min(player_chunk.z + load_range, WorldSettings.WORLD_SIZE_IN_CHUNKS);

        NativeList<int3> chunks_to_unrender = new NativeList<int3>(WorldSettings.RENDER_CONTAINER_SIZE, Allocator.Persistent);

        foreach (var data in chunk_components) {
            int3 chunk_pos = data.Key;
            float distance = math.lengthsq(player_chunk - chunk_pos);
            if (distance >= render_distance) { chunks_to_unrender.Add(chunk_pos); }
        }

        foreach (int3 chunk_pos in chunks_to_unrender) {
            Object.Destroy(chunk_components[chunk_pos].gameObject);
            chunk_components.Remove(chunk_pos);
        }

        chunks_to_unrender.Dispose();
        NativeList<int3> chunks_to_remove = new NativeList<int3>(WorldSettings.LOAD_CONTAINER_SIZE, Allocator.Persistent);

        foreach (var data in chunk_data) {
            int3 chunk_pos = data.Key;
            float distance = math.lengthsq(player_chunk - chunk_pos);
            if (distance >= load_distance) { chunks_to_remove.Add(chunk_pos); }
        }

        foreach (int3 chunk_pos in chunks_to_remove) {
            chunk_data[chunk_pos].Dispose();
            chunk_data.Remove(chunk_pos);
        }

        chunks_to_remove.Dispose();

        NativeList<int3> chunks_to_generate = new NativeList<int3>(WorldSettings.LOAD_CONTAINER_SIZE, Allocator.Persistent);
        NativeList<int3> chunks_to_render = new NativeList<int3>(WorldSettings.RENDER_CONTAINER_SIZE, Allocator.Persistent);
        
        for (int x = min_x; x < max_x; ++x) {
            for (int y = min_y; y < max_y; ++y) {
                for (int z = min_z; z < max_z; ++z) {
                    
                    int3 chunk_coord = new int3(x, y, z);
                    float distance = math.lengthsq(player_chunk - chunk_coord);
                    if (distance < load_distance && !chunk_data.ContainsKey(chunk_coord)) { 
                        chunks_to_generate.Add(chunk_coord);
                    } else if (distance < render_distance && !chunk_components.ContainsKey(chunk_coord) &&
                               GetNeighbors(chunk_coord) == 6) {
                        chunks_to_render.Add(chunk_coord);
                    }
                    
                }
            }
        }

        chunk_scheduler.ReplaceGenerateQueue(chunks_to_generate);
        chunks_to_generate.Dispose();

        chunk_scheduler.ReplaceRenderQueue(chunks_to_render);
        chunks_to_render.Dispose();
    }

    [BurstCompile]
    public ChunkAccessor GetAccessor(NativeArray<int3> chunk_coords) {
        return new ChunkAccessor(chunk_coords, ref chunk_data);
    }

    [BurstCompile]
    public void AddGeneratedChunks(NativeParallelHashMap<int3, ChunkData> _chunk_data) {
        if (_chunk_data.IsEmpty) { return; }

        NativeList<int3> chunks_to_render = new NativeList<int3>(_chunk_data.Count() * 2, Allocator.Persistent);

        foreach (var pair in _chunk_data) { 
            chunk_data.Add(pair.Key, pair.Value);
            foreach (int3 dir in Utility.self_dirs) { 

                int3 nbr_coord = pair.Key + dir;
                if (GetNeighbors(nbr_coord) == 6 && ChunkInWorld(nbr_coord) &&
                    Player.ChunkDistanceFromPlayer(nbr_coord) < WorldSettings.RENDER_DISTANCE) { 
                        chunks_to_render.Add(nbr_coord);
                }    
            }
        }

        chunk_scheduler.QueueChunksForRendering(chunks_to_render);
        chunks_to_render.Dispose();
    }

    public void AddRenderedChunks(NativeArray<int3> _jobs, Mesh.MeshDataArray _chunk_meshes) {
        if (_chunk_meshes.Length == 0) { return; }

        Mesh[] meshes = new Mesh[_jobs.Length];
        for (int idx = 0; idx < _jobs.Length; ++idx) { 
            GameObject chunk_object = new GameObject($"Chunk {_jobs[idx]}", typeof(MeshFilter), typeof(MeshRenderer));
            chunk_object.transform.position = _jobs[idx].Vector3() * 32;
            chunk_object.transform.parent = World.world_object;
            chunk_object.GetComponent<MeshRenderer>().material = World.material;

            Chunk chunk_component = chunk_object.AddComponent<Chunk>();
            meshes[idx] = chunk_component.mesh_filter.mesh;
            chunk_components[_jobs[idx]] = chunk_component;
        }

        Mesh.ApplyAndDisposeWritableMeshData(_chunk_meshes, meshes);
        foreach (Mesh mesh in meshes) { mesh.RecalculateBounds(); }

    }

    [BurstCompile]
    public void Dispose() {
        foreach (var pair in chunk_data) { pair.Value.Dispose(); }
        chunk_data.Dispose();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    [BurstCompile]
    private int GetNeighbors(int3 _chunk_coord) {
        int neighbors = 0;

        foreach (int3 dir in Utility.dirs) {
            int3 nbr = _chunk_coord + dir;
            if (chunk_data.ContainsKey(nbr) || !ChunkInWorld(nbr)) {
                ++neighbors;
            }
        }

        return neighbors;
    }

    [BurstCompile]
    private bool ChunkInWorld(int3 coord) {
        return 0 <= coord.x && coord.x < WorldSettings.WORLD_SIZE_IN_CHUNKS &&
               0 <= coord.y && coord.y < WorldSettings.WORLD_HEIGHT_IN_CHUNKS &&
               0 <= coord.z && coord.z < WorldSettings.WORLD_SIZE_IN_CHUNKS;
    }
}