using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public struct ChunkJob : IJobParallelFor {

    [ReadOnly] public NativeArray<int3> jobs;
    public NativeParallelHashMap<int3, ChunkData>.ParallelWriter output;

    public void Execute(int idx) {

        int3 chunk_coord = jobs[idx];
        int3 chunk_pos = chunk_coord * 32;
        ChunkData chunk_data = new ChunkData(chunk_pos);

        for (int x = 0; x < 32; ++x) {
            int global_x = chunk_pos.x + x;

            for (int z = 0; z < 32; ++z) {
                int global_z = chunk_pos.z + z;
                
                int surface_height = WorldGen.GetSurfaceHeight(global_x, global_z);

                for (int y = 0; y < 32; ++y) {
                    int global_y = chunk_pos.y + y;

                    int block_idx = new int3(x, y, z).Flatten();
                    if (global_y == 0) { chunk_data.blocks[block_idx] = 1; }
                    else if (global_y < surface_height) { chunk_data.blocks[block_idx] = 3; }
                    else if (global_y == surface_height) { chunk_data.blocks[block_idx] = 2; }
                    else { break; }

                }
            }
        }

        output.TryAdd(chunk_coord, chunk_data); 
    }

}

[BurstCompile]
public struct RenderJob : IJobParallelFor {

    [ReadOnly] public NativeArray<int3> jobs;
    [ReadOnly] public ChunkAccessor accessor;
    [ReadOnly] public NativeArray<int3> verts;
    [ReadOnly] public NativeArray<int3> dirs;
    [ReadOnly] public NativeArray<VertexAttributeDescriptor> vertex_attributes;
    [ReadOnly] public Data.BlockData block_data;
    public Mesh.MeshDataArray output;

    public void Execute(int idx) {

        int3 chunk_coord = jobs[idx];
        int3 chunk_pos = chunk_coord * 32;

        int vertex_count = 0;
        NativeList<float3> vertices = new NativeList<float3>(Allocator.Temp);
        NativeList<float3> normals = new NativeList<float3>(Allocator.Temp);
        NativeList<float2> uvs = new NativeList<float2>(Allocator.Temp);
        NativeList<int> triangles = new NativeList<int>(Allocator.Temp);

        for (int x = 0; x < 32; ++x) {
            for (int z = 0; z < 32; ++z) {
                for (int y = 0; y < 32; ++y) {

                    int3 block_pos = new int3(x, y, z);
                    int block_idx = block_pos.Flatten();
                    if (accessor.chunk_data.ContainsKey(chunk_coord) &&
                        accessor.chunk_data[chunk_coord].blocks[block_idx] == 0) { continue; }
                    int block_type = accessor.chunk_data[chunk_coord].blocks[block_idx];

                    for (int face = 0; face < 6; ++face) {

                        int3 nbr_global = chunk_pos + block_pos + dirs[face];
                        int3 nbr_chunk = Utility.GetChunkCoord(nbr_global);
                        int nbr_idx = Utility.GetLocalPos(nbr_global).Flatten();
                        int nbr_type = accessor.chunk_data[nbr_chunk].blocks[nbr_idx];
                        if (nbr_type != 0) { continue; }

                        vertices.Add(block_pos + verts[face * 4]);
                        vertices.Add(block_pos + verts[face * 4 + 1]);
                        vertices.Add(block_pos + verts[face * 4 + 2]);
                        vertices.Add(block_pos + verts[face * 4 + 3]);

                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);
                        normals.Add(dirs[face]);

                        int texture_idx = block_data.face_data[block_type * 6 + face];
                        float2 base_uv = new float2(0.0625f * (texture_idx % 16), 1.0f - (texture_idx / 16) - 0.0625f);

                        uvs.Add(base_uv);
                        uvs.Add(base_uv + new float2(0.0625f, 0));
                        uvs.Add(base_uv + new float2(0.0625f, 0.0625f));
                        uvs.Add(base_uv + new float2(0, 0.0625f));

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

        Mesh.MeshData mesh = output[idx];

        mesh.SetVertexBufferParams(vertices.Length, vertex_attributes);
        ApplyMeshData(mesh, vertices, normals, uvs, triangles);

        vertices.Dispose();
        normals.Dispose();
        uvs.Dispose();
        triangles.Dispose();

    }

    unsafe public void ApplyMeshData(
        Mesh.MeshData mesh,
        NativeList<float3> vertices,
        NativeList<float3> normals,
        NativeList<float2> uvs,
        NativeList<int> triangles) {

        unsafe {

            mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);

            var vertexData = mesh.GetVertexData<byte>(); // Use byte because it’s interleaved
            int stride = mesh.GetVertexBufferStride(0);  // Get per-vertex stride (e.g. 32 bytes)

            for (int i = 0; i < vertices.Length; i++) {
                int offset = i * stride;

                float3 vertice = vertices[i];
                float3 normal = normals[i];
                float2 uv = uvs[i];

                UnsafeUtility.CopyStructureToPtr(ref vertice, (byte*)vertexData.GetUnsafePtr() + offset + 0);
                UnsafeUtility.CopyStructureToPtr(ref normal, (byte*)vertexData.GetUnsafePtr() + offset + 12);
                UnsafeUtility.CopyStructureToPtr(ref uv,     (byte*)vertexData.GetUnsafePtr() + offset + 24);
            }

            var indexData = mesh.GetIndexData<int>();
            indexData.CopyFrom(triangles.AsArray());

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length));
            
        }

    }

}