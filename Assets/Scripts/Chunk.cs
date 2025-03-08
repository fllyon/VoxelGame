using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {
    public static int CHUNK_SIZE = 32;

    Material material;
    Vector3Int chunkCoord;
    int[,,] chunk_data = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    public void Init(Vector3Int chunkCoord_in, Material material_in) {
        chunkCoord = chunkCoord_in;
        material = material_in;
        GenerateChunk();
    }

    void GenerateChunk() {
        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {

                for (int y = 0; y < CHUNK_SIZE; ++y) {
                    chunk_data[x, y, z] = 1;
                }
            }
        }
    }

    void Start() {
        
        int vertex_count = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // For each block
        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {
                for (int y = 0; y < CHUNK_SIZE; ++y) {

                    if (chunk_data[x, y, z] == 0) { continue; }

                    Vector3 block_position = new Vector3(x, y, z);
                    for (int face = 0; face < 6; ++face) {

                        int nbr_x = x + VoxelData.directions[face].x;
                        int nbr_y = y + VoxelData.directions[face].y;
                        int nbr_z = z + VoxelData.directions[face].z;

                        if (0 <= nbr_x && nbr_x < 32 && 0 <= nbr_y && nbr_y < 32 && 0 <= nbr_z && nbr_z < 32) {
                            if (chunk_data[x + VoxelData.directions[face].x, y + VoxelData.directions[face].y, z + VoxelData.directions[face].z] == 1) { continue; }
                        }

                        for (int tri_idx = 0; tri_idx < 6; ++tri_idx) {
                            int vrtx_idx = VoxelData.Triangles[face, tri_idx];
                            vertices.Add(block_position + VoxelData.Vertices[vrtx_idx]);
                            uvs.Add(VoxelData.UVs[tri_idx]);
                            triangles.Add(vertex_count);
                            ++vertex_count;
                        }
                    }
                
                }
            }
        }
        
        

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;
    }
}
