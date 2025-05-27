using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Priority_Queue;

public class ChunkManager {

    public static int MAX_CHUNKS = 4096;
    public static int WORLD_SIZE_IN_CHUNKS = 16;

    private Data.BlockData block_data;

    private NativeHashSet<int3> chunks;
    private NativeHashMap<int3, ChunkData> chunk_data;
    private Dictionary<int3, Chunk> chunk_components;

    // private NativeQueue<int3> chunks_to_generate;
    // private NativeQueue<int3> chunks_to_draw;

    private FastPriorityQueue<ChunkNode> chunks_to_generate;
    private FastPriorityQueue<ChunkNode> chunks_to_draw;

    public ChunkManager(Data.BlockData _block_data) {

        block_data = _block_data;
        chunks = new NativeHashSet<int3>(MAX_CHUNKS, Allocator.Persistent);
        chunk_data = new NativeHashMap<int3, ChunkData>(MAX_CHUNKS, Allocator.Persistent);
        chunk_components = new Dictionary<int3, Chunk>(MAX_CHUNKS);

        // chunks_to_generate = new NativeQueue<int3>(Allocator.Persistent);
        // chunks_to_draw = new NativeQueue<int3>(Allocator.Persistent);
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

        // Create the chunk and generate it
        for (int x = 0; x < WORLD_SIZE_IN_CHUNKS; ++x) {
            for (int z = 0; z < WORLD_SIZE_IN_CHUNKS; ++z) {
                for (int y = 0; y < WORLD_SIZE_IN_CHUNKS; ++y) {

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
                    _chunk_object.GetOrAddComponent<MeshRenderer>().material = Resources.Load<Material>("BlockTextures");

                    // Update hash set + maps with new data
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

        ChunkData _chunk_data = chunk_data[_chunk_coord];

        int _vertex_count = 0;
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector3> _normals = new List<Vector3>();
        List<Vector2> _uvs = new List<Vector2>();
        List<int> _triangles = new List<int>();

        int3[] _directions = {
            new int3(0, 1, 0), new int3(0, 0, 1), new int3(1, 0, 0), new int3(0, 0, -1), new int3(-1, 0, 0), new int3(0, -1, 0)
        };

        Vector3[][] _verts = {
            new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) },
            new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) },
            new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) },
            new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) },
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) },
            new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) }
        };

        Vector3[] norms = {
            new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, -1, 0)
        };

        for (int xx = 0; xx < ChunkData.CHUNK_SIZE; ++xx) {
            for (int zz = 0; zz < ChunkData.CHUNK_SIZE; ++zz) {
                for (int yy = 0; yy < ChunkData.CHUNK_SIZE; ++yy) {
                    
                    int3 _block_pos = new int3(xx, yy, zz);
                    byte _block_type = _chunk_data.blocks[_block_pos.Flatten()];
                    if (_block_type == 0) { continue; }

                    for (int face = 0; face < 6; ++face) {
                        
                        int3 _nbr_pos = _block_pos + _directions[face];
                        byte nbr_type = (0 <= _nbr_pos.x && _nbr_pos.x < 32 &&
                                         0 <= _nbr_pos.y && _nbr_pos.y < 32 &&
                                         0 <= _nbr_pos.z && _nbr_pos.z < 32)
                                            ? _chunk_data.blocks[_nbr_pos.Flatten()]
                                            : GetBlock(_nbr_pos + _chunk_coord * ChunkData.CHUNK_SIZE);

                        if (nbr_type != 0) { continue; }

                        Vector3 pos = _block_pos.Vector3();
                        _vertices.Add(pos + _verts[face][0]);
                        _vertices.Add(pos + _verts[face][1]);
                        _vertices.Add(pos + _verts[face][2]);
                        _vertices.Add(pos + _verts[face][3]);

                        _normals.Add(norms[face]);
                        _normals.Add(norms[face]);
                        _normals.Add(norms[face]);
                        _normals.Add(norms[face]);

                        int texture_idx = block_data.face_data[_block_type * 6 + face];
                        Vector2 base_uv = new Vector2(0.0625f * (texture_idx % 16), 1.0f - (texture_idx / 16) - 0.0625f);

                        _uvs.Add(base_uv);
                        _uvs.Add(base_uv + new Vector2(0.0625f, 0));
                        _uvs.Add(base_uv + new Vector2(0.0625f, 0.0625f));
                        _uvs.Add(base_uv + new Vector2(0, 0.0625f));

                        _triangles.Add(_vertex_count);
                        _triangles.Add(_vertex_count + 1);
                        _triangles.Add(_vertex_count + 2);
                        _triangles.Add(_vertex_count + 2);
                        _triangles.Add(_vertex_count + 3);
                        _triangles.Add(_vertex_count);

                        _vertex_count += 4;
                    }

                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
        mesh.normals = _normals.ToArray();
        mesh.uv = _uvs.ToArray();
        mesh.triangles = _triangles.ToArray();

        chunk_components[_chunk_coord].mesh_filter.mesh = mesh;
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
               0 <= coord.y && coord.y < WORLD_SIZE_IN_CHUNKS &&
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

        // if (chunks_to_generate.IsCreated) { chunks_to_generate.Dispose(); }
        // if (chunks_to_draw.IsCreated) { chunks_to_draw.Dispose(); }
    }
}