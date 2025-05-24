using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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

        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {

                int _height = (int)(noise.cnoise(new float2((position.x + x) * 0.025f + 0.1f, (position.z + z) * 0.025f + 0.1f)) * 6 + 80);
                for (int y = 0; y < CHUNK_SIZE; ++y) {
                    int global_y = position.y + y;

                    if (global_y > _height) { continue; }
                    int _idx = new int3(x, y, z).Flatten();
                    
                    if (global_y == _height) { blocks[_idx] = 2; }
                    else { blocks[_idx] = 3; }

                }
            }
        }
    }


    // public static int CHUNK_SIZE = 32;

    // MeshFilter mesh_filter;
    // MeshRenderer mesh_renderer;

    // int3 chunk_pos;
    // NativeArray<int> blocks;
    // NativeArray<Vertex> vertex_data;
    // NativeArray<int3> directions;
    // NativeArray<int> texture_map;

    // void Start()
    // {
    //     chunk_pos = (int3)new float3(transform.position.x, transform.position.y, transform.position.z);
    //     mesh_filter = GetComponent<MeshFilter>();
    //     mesh_renderer = GetComponent<MeshRenderer>();
    //     blocks = new NativeArray<int>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    //     vertex_data = VertexData.GetVertices();
    //     directions = VertexData.GetDirections();
    //     texture_map = VertexData.GetTextureMap();

    //     // Chunk generation
    //     ChunkGenerateJob generate_job = new ChunkGenerateJob() { _chunk_pos = chunk_pos, _blocks = blocks };
    //     JobHandle generation_handle = generate_job.Schedule();
    //     generation_handle.Complete();

    //     NativeList<float3> vertices = new NativeList<float3>(Allocator.TempJob);
    //     NativeList<float3> normals = new NativeList<float3>(Allocator.TempJob);
    //     NativeList<float2> uvs = new NativeList<float2>(Allocator.TempJob);
    //     NativeList<int> triangles = new NativeList<int>(Allocator.TempJob);

    //     // Chunk mesh
    //     ChunkMeshJob mesh_job = new ChunkMeshJob() { _blocks = blocks, _vertex_data = vertex_data, _directions = directions, _texture_map = texture_map, _vertices = vertices, _normals = normals, _uvs = uvs, _triangles = triangles };
    //     JobHandle mesh_handle = mesh_job.Schedule();
    //     mesh_handle.Complete();

    //     Vector3[] _vertices = new Vector3[vertices.Length];
    //     Vector3[] _normals = new Vector3[normals.Length];
    //     Vector2[] _uvs = new Vector2[uvs.Length];
    //     int[] _triangles = new int[triangles.Length];

    //     for (int idx = 0; idx < vertices.Length; ++idx) { _vertices[idx] = vertices[idx]; }
    //     for (int idx = 0; idx < normals.Length; ++idx) { _normals[idx] = normals[idx]; }
    //     for (int idx = 0; idx < uvs.Length; ++idx) { _uvs[idx] = uvs[idx]; }
    //     for (int idx = 0; idx < triangles.Length; ++idx) { _triangles[idx] = triangles[idx]; }

    //     Mesh mesh = new Mesh();
    //     mesh.vertices = _vertices;
    //     mesh.normals = _normals;
    //     mesh.uv = _uvs;
    //     mesh.triangles = _triangles;
    //     mesh_filter.mesh = mesh;
    //     mesh_renderer.material = Resources.Load<Material>("BlockTextures");

    //     vertices.Dispose();
    //     normals.Dispose();
    //     uvs.Dispose();
    //     triangles.Dispose();
    // }

    // void OnDestroy()
    // {
    //     blocks.Dispose();
    //     vertex_data.Dispose();
    //     directions.Dispose();
    // }

    // public struct ChunkGenerateJob : IJob
    // {

    //     public int3 _chunk_pos;
    //     public NativeArray<int> _blocks;

    //     public void Execute()
    //     {

    //         for (int x = 0; x < CHUNK_SIZE; ++x)
    //         {
    //             for (int z = 0; z < CHUNK_SIZE; ++z)
    //             {
    //                 int height = (int)(noise.cnoise(new float2(_chunk_pos.x + x, _chunk_pos.z + z) * 0.025f + 0.1f) * 4 + 40);
    //                 for (int y = 0; y < CHUNK_SIZE; ++y)
    //                 {
    //                     int global_y = _chunk_pos.y + y;
    //                     if (global_y > height) continue;

    //                     int idx = x * CHUNK_SIZE * CHUNK_SIZE + z * CHUNK_SIZE + y;
    //                     if (global_y == height)
    //                     {
    //                         _blocks[idx] = 1;
    //                         continue;
    //                     }
    //                     else
    //                     {
    //                         _blocks[idx] = 2;
    //                     }


    //                 }
    //             }
    //         }
    //     }

    // }

    // public struct ChunkMeshJob : IJob
    // {

    //     public NativeArray<int> _blocks;
    //     public NativeArray<Vertex> _vertex_data;
    //     public NativeArray<int3> _directions;
    //     public NativeArray<int> _texture_map;

    //     private int _vertex_count;
    //     public NativeList<float3> _vertices;
    //     public NativeList<float3> _normals;
    //     public NativeList<float2> _uvs;
    //     public NativeList<int> _triangles;

    //     public void Execute()
    //     {

    //         _vertex_count = 0;
    //         for (int x = 0; x < 32; ++x)
    //         {
    //             for (int z = 0; z < 32; ++z)
    //             {
    //                 for (int y = 0; y < 32; ++y)
    //                 {
    //                     int idx = x * 32 * 32 + z * 32 + y;
    //                     int block_type = _blocks[idx];
    //                     if (block_type == 0) { continue; }

    //                     for (int face = 0; face < 6; ++face)
    //                     {
    //                         int3 nbr = new int3(x, z, y) + _directions[face];
    //                         int n_idx = nbr.x * 32 * 32 + nbr.y * 32 + nbr.z;

    //                         if (0 <= nbr.x && nbr.x < 32 &&
    //                             0 <= nbr.z && nbr.z < 32 &&
    //                             0 <= nbr.y && nbr.y < 32 &&
    //                             _blocks[n_idx] != 0) { continue; }

    //                         int vrtx = 4 * face;

    //                         float3 block_pos = new float3(x, y, z);
    //                         _vertices.Add(block_pos + _vertex_data[vrtx].position);
    //                         _vertices.Add(block_pos + _vertex_data[vrtx + 1].position);
    //                         _vertices.Add(block_pos + _vertex_data[vrtx + 2].position);
    //                         _vertices.Add(block_pos + _vertex_data[vrtx + 3].position);

    //                         _normals.Add(_vertex_data[vrtx].normal);
    //                         _normals.Add(_vertex_data[vrtx].normal);
    //                         _normals.Add(_vertex_data[vrtx].normal);
    //                         _normals.Add(_vertex_data[vrtx].normal);

    //                         int texture_idx = _texture_map[(block_type - 1) * 6 + face];
    //                         float2 base_texture = new float2(0.0625f * (texture_idx % 16), 1.0f - (texture_idx / 16) - 0.0625f);
    //                         _uvs.Add(base_texture + _vertex_data[vrtx].uv);
    //                         _uvs.Add(base_texture + _vertex_data[vrtx + 1].uv);
    //                         _uvs.Add(base_texture + _vertex_data[vrtx + 2].uv);
    //                         _uvs.Add(base_texture + _vertex_data[vrtx + 3].uv);

    //                         _triangles.Add(_vertex_count);
    //                         _triangles.Add(_vertex_count + 1);
    //                         _triangles.Add(_vertex_count + 2);
    //                         _triangles.Add(_vertex_count + 2);
    //                         _triangles.Add(_vertex_count + 3);
    //                         _triangles.Add(_vertex_count);

    //                         _vertex_count += 4;
    //                     }

    //                 }
    //             }
    //         }
    //     }

    // }
}