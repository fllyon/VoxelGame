using Priority_Queue;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

public class ChunkManager {

    public static int MAX_CHUNKS = 4096;
    public static int WORLD_SIZE_IN_CHUNKS = 16;
    public static int WORLD_HEIGHT_IN_CHUNKS = 16;

    private Data.BlockData block_data;

    private NativeHashSet<int3> chunks;
    private NativeHashMap<int3, ChunkData> chunk_data;
    private Dictionary<int3, Chunk> chunk_components;

    private FastPriorityQueue<ChunkNode> chunks_to_generate;
    private FastPriorityQueue<ChunkNode> chunks_to_draw;

    public ChunkManager(Data.BlockData _block_data) {

        block_data = _block_data;
        chunks = new NativeHashSet<int3>(MAX_CHUNKS, Allocator.Persistent);
        chunk_data = new NativeHashMap<int3, ChunkData>(MAX_CHUNKS, Allocator.Persistent);
        chunk_components = new Dictionary<int3, Chunk>(MAX_CHUNKS);

        chunks_to_generate = new FastPriorityQueue<ChunkNode>(MAX_CHUNKS);
        chunks_to_draw = new FastPriorityQueue<ChunkNode>(MAX_CHUNKS);

        Initialize();
    }

    // ============================================================= //
    //                       Utility Datatypes                       //
    // ============================================================= //

    public class ChunkNode : FastPriorityQueueNode {
        public int3 coord;
        public ChunkNode(int3 coord_in) { coord = coord_in; }
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    void Initialize() {

        Transform world_transform = GameObject.Find("World").transform;
        Material world_material = Resources.Load<Material>("BlockTextures");

        // Create the chunk and generate it
        for (int x = 0; x < WORLD_SIZE_IN_CHUNKS; ++x) {
            for (int z = 0; z < WORLD_SIZE_IN_CHUNKS; ++z) {
                for (int y = 0; y < WORLD_HEIGHT_IN_CHUNKS; ++y) {

                    int3 _chunk_coord = new int3(x, y, z);

                    Chunk _chunk_component;
                    GameObject _chunk_object;

                    _chunk_object = new GameObject($"Chunk {_chunk_coord}", typeof(MeshFilter), typeof(MeshRenderer)) {
                        transform = {
                            position = (_chunk_coord * ChunkData.CHUNK_SIZE).Vector3(),
                            parent = world_transform
                        }
                    };
                    _chunk_component = _chunk_object.AddComponent<Chunk>();
                    _chunk_object.GetComponent<MeshRenderer>().material = world_material;

                    chunks.Add(_chunk_coord);
                    chunk_components.Add(_chunk_coord, _chunk_component);
                    chunks_to_generate.Enqueue(new ChunkNode(_chunk_coord), Player.ChunkDistanceFromPlayer(_chunk_coord));
                }
            }
        }
    }

    public void Update() {
        if (chunks_to_generate.Count != 0) { GenerateChunk(chunks_to_generate.Dequeue().coord); }
        if (chunks_to_draw.Count != 0) { DrawChunk(chunks_to_draw.Dequeue().coord); }
    }

    private void GenerateChunk(int3 _chunk_coord) {
        ChunkData _chunk_data = new ChunkData(_chunk_coord * ChunkData.CHUNK_SIZE);
        chunk_data.Add(_chunk_coord, _chunk_data);

        foreach (int3 dir in Utility.self_dirs) {
            int3 _nbr_coord = _chunk_coord + dir;
            if (!chunk_data.ContainsKey(_nbr_coord)) { continue; }
            if (GetNeighbors(_nbr_coord) == 6) { chunks_to_draw.Enqueue(new ChunkNode(_nbr_coord), Player.ChunkDistanceFromPlayer(_nbr_coord)); }
        }
    }

    private void DrawChunk(int3 _chunk_coord) {

        int blocks_per_chunk = ChunkData.CHUNK_SIZE.Cubed();
        NativeArray<byte> neighbor_blocks = new NativeArray<byte>(blocks_per_chunk * 6, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        if (_chunk_coord.y != WORLD_HEIGHT_IN_CHUNKS - 1) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[0]].blocks, 0, neighbor_blocks, 0, blocks_per_chunk); }
        if (_chunk_coord.z != WORLD_SIZE_IN_CHUNKS - 1) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[1]].blocks, 0, neighbor_blocks, blocks_per_chunk * 1, blocks_per_chunk); }
        if (_chunk_coord.x != WORLD_SIZE_IN_CHUNKS - 1) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[2]].blocks, 0, neighbor_blocks, blocks_per_chunk * 2, blocks_per_chunk); }
        if (_chunk_coord.z != 0) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[3]].blocks, 0, neighbor_blocks, blocks_per_chunk * 3, blocks_per_chunk); }
        if (_chunk_coord.x != 0) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[4]].blocks, 0, neighbor_blocks, blocks_per_chunk * 4, blocks_per_chunk); }
        if (_chunk_coord.y != 0) { NativeArray<byte>.Copy(chunk_data[_chunk_coord + Utility.dirs[5]].blocks, 0, neighbor_blocks, blocks_per_chunk * 5, blocks_per_chunk); }

        NativeList<float3> vertices = new NativeList<float3>(ChunkData.CHUNK_SIZE.Cubed() * 4, Allocator.Persistent);
        NativeList<float3> normals = new NativeList<float3>(ChunkData.CHUNK_SIZE.Cubed() * 4, Allocator.Persistent);
        NativeList<float2> uvs = new NativeList<float2>(ChunkData.CHUNK_SIZE.Cubed() * 4, Allocator.Persistent);
        NativeList<int> triangles = new NativeList<int>(ChunkData.CHUNK_SIZE.Cubed() * 6, Allocator.Persistent);

        ChunkDrawJob draw_job = new ChunkDrawJob {
            chunk_size = ChunkData.CHUNK_SIZE,
            blocks = chunk_data[_chunk_coord].blocks,
            nbr_blocks = neighbor_blocks,
            face_data = block_data.face_data,
            vertices = vertices,
            normals = normals,
            uvs = uvs,
            triangles = triangles
        };

        JobHandle draw_handle = draw_job.Schedule();
        draw_handle.Complete();

        Vector3[] _vertices = new Vector3[vertices.Length];
        Vector3[] _normals = new Vector3[normals.Length];
        Vector2[] _uvs = new Vector2[uvs.Length];
        int[] _triangles = new int[triangles.Length];

        for (int idx = 0; idx < vertices.Length; ++idx) { _vertices[idx] = vertices[idx]; }
        for (int idx = 0; idx < normals.Length; ++idx) { _normals[idx] = normals[idx]; }
        for (int idx = 0; idx < uvs.Length; ++idx) { _uvs[idx] = uvs[idx]; }
        for (int idx = 0; idx < triangles.Length; ++idx) { _triangles[idx] = triangles[idx]; }

        Mesh mesh = new Mesh();
        mesh.vertices = _vertices;
        mesh.normals = _normals;
        mesh.uv = _uvs;
        mesh.triangles = _triangles;
        chunk_components[_chunk_coord].mesh_filter.mesh = mesh;
        mesh.RecalculateBounds();

        neighbor_blocks.Dispose();
        vertices.Dispose();
        normals.Dispose();
        uvs.Dispose();
        triangles.Dispose();
    }

    public byte GetBlock(int3 coord) {
        int3 chunk_coord = Utility.GetChunkCoord(coord);
        int3 block_coord = coord - (chunk_coord * ChunkData.CHUNK_SIZE);
        if (!chunk_data.ContainsKey(chunk_coord)) { return 0; }
        byte val = chunk_data[chunk_coord].blocks[block_coord.Flatten()];
        return val;
    }

    public bool ChunkInWorld(int3 coord) {
        return 0 <= coord.x && coord.x < WORLD_SIZE_IN_CHUNKS &&
               0 <= coord.y && coord.y < WORLD_HEIGHT_IN_CHUNKS &&
               0 <= coord.z && coord.z < WORLD_SIZE_IN_CHUNKS;
    }

    private byte GetNeighbors(int3 _chunk_coord) {
        byte _neighbors = 0;

        foreach (int3 dir in Utility.dirs) {
            int3 nbr = _chunk_coord + dir;
            if (chunk_data.ContainsKey(nbr) || !ChunkInWorld(nbr)) {
                ++_neighbors;
            }
        }

        return _neighbors;
    }

    public void Dispose() {
        foreach (var pair in chunk_data) { pair.Value.Dispose(); }

        if (chunks.IsCreated) { chunks.Dispose(); }
        if (chunk_data.IsCreated) { chunk_data.Dispose(); }
    }
}