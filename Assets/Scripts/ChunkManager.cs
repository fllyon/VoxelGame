using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class ChunkManager {

    public static int MAX_CHUNKS = 64;
    public static int LOAD_DIST = 3;
    public static int DRAW_DIST = 2;

    private Data.BlockData block_data;

    private NativeHashSet<int3> chunks;
    private NativeHashMap<int3, ChunkData> chunk_data;
    private Dictionary<int3, Chunk> chunk_components;

    List<JobHandle> generate_handles;
    List<JobHandle> draw_handles;

    public ChunkManager(Data.BlockData _block_data) {

        block_data = _block_data;
        chunks = new NativeHashSet<int3>(MAX_CHUNKS, Allocator.Persistent);
        chunk_data = new NativeHashMap<int3, ChunkData>(MAX_CHUNKS, Allocator.Persistent);
        chunk_components = new Dictionary<int3, Chunk>(MAX_CHUNKS);

        Initialize();
    }

    // ============================================================= //
    //                        Utility Structs                        //
    // ============================================================= //

    // public struct int3_type : FastPriorityQueueNode {

    // }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    public void Dispose() {
        foreach (var pair in chunk_data) { pair.Value.Dispose(); }

        if (chunks.IsCreated) { chunks.Dispose(); }
        if (chunk_data.IsCreated) { chunk_data.Dispose(); }
    }

    void Initialize() {

        Transform world_transform = GameObject.Find("World").transform;

        // Create the chunk and generate it
        for (int x = 0; x < 4; ++x) {
            for (int z = 0; z < 4; ++z) {
                for (int y = 0; y < 4; ++y) {

                    int3 _chunk_coord = new int3(x, y, z);
                    if (chunks.Contains(_chunk_coord)) { continue; }

                    Chunk _chunk_component;
                    GameObject _chunk_object;
                    ChunkData _chunk_data;

                    // Create a new chunk gameObject and add required components
                    _chunk_object = new GameObject("Chunk " + _chunk_coord, typeof(MeshFilter), typeof(MeshRenderer));
                    _chunk_object.transform.position = (_chunk_coord * ChunkData.CHUNK_SIZE).Vector3();
                    _chunk_object.transform.parent = world_transform;
                    _chunk_component = _chunk_object.AddComponent<Chunk>();
                    _chunk_object.GetOrAddComponent<MeshRenderer>().material = Resources.Load<Material>("BlockTextures");

                    // Create data for the new chunk
                    _chunk_data = new ChunkData(_chunk_coord * ChunkData.CHUNK_SIZE);

                    // Update hash set + maps with new data
                    chunks.Add(_chunk_coord);
                    chunk_data.Add(_chunk_coord, _chunk_data);
                    chunk_components.Add(_chunk_coord, _chunk_component);

                    // Build a mesh and add it to the component
                    int vertex_count = 0;
                    List<Vector3> vertices = new List<Vector3>();
                    List<Vector3> normals = new List<Vector3>();
                    List<Vector2> uvs = new List<Vector2>();
                    List<int> triangles = new List<int>();

                    for (int xx = 0; xx < ChunkData.CHUNK_SIZE; ++xx) {
                        for (int zz = 0; zz < ChunkData.CHUNK_SIZE; ++zz) {
                            for (int yy = 0; yy < ChunkData.CHUNK_SIZE; ++yy) {
                                
                                int3 block_pos = new int3(xx, yy, zz);
                                byte block_type = _chunk_data.blocks[block_pos.Flatten()];
                                if (block_type == 0) { continue; }

                                int3[] directions = {
                                    new int3(0, 1, 0), new int3(0, 0, 1), new int3(1, 0, 0), new int3(0, 0, -1), new int3(-1, 0, 0), new int3(0, -1, 0)
                                };

                                Vector3[][] verts = {
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

                                for (int face = 0; face < 6; ++face) {
                                    int3 nbr_pos = block_pos + directions[face];
                                    if (0 <= nbr_pos.x && nbr_pos.x < 32 &&
                                        0 <= nbr_pos.y && nbr_pos.y < 32 &&
                                        0 <= nbr_pos.z && nbr_pos.z < 32 &&
                                        _chunk_data.blocks[nbr_pos.Flatten()] != 0) { continue; }

                                    Vector3 pos = block_pos.Vector3();
                                    vertices.Add(pos + verts[face][0]);
                                    vertices.Add(pos + verts[face][1]);
                                    vertices.Add(pos + verts[face][2]);
                                    vertices.Add(pos + verts[face][3]);

                                    normals.Add(norms[face]);
                                    normals.Add(norms[face]);
                                    normals.Add(norms[face]);
                                    normals.Add(norms[face]);

                                    int texture_idx = block_data.face_data[block_type * 6 + face];
                                    Vector2 base_uv = new Vector2(0.0625f * (texture_idx % 16), 1.0f - (texture_idx / 16) - 0.0625f);

                                    uvs.Add(base_uv);
                                    uvs.Add(base_uv + new Vector2(0.0625f, 0));
                                    uvs.Add(base_uv + new Vector2(0.0625f, 0.0625f));
                                    uvs.Add(base_uv + new Vector2(0, 0.0625f));

                                    triangles.Add(vertex_count);
                                    triangles.Add(vertex_count + 1);
                                    triangles.Add(vertex_count + 2);
                                    triangles.Add(vertex_count + 2);
                                    triangles.Add(vertex_count + 3);
                                    triangles.Add(vertex_count);

                                    vertex_count += 4;
                                }

                            }
                        }
                    }

                    Mesh mesh = new Mesh();
                    mesh.vertices = vertices.ToArray();
                    mesh.normals = normals.ToArray();
                    mesh.uv = uvs.ToArray();
                    mesh.triangles = triangles.ToArray();

                    chunk_components[_chunk_coord].mesh_filter.mesh = mesh;
                }
            }
        }

    }

    
}