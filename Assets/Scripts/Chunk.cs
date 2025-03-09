using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {

    public static int CHUNK_SIZE = 32;
    public static int CHUNK_SIZE_BIT_SHIFT = 5; // To save runtime, manually calculate

    int[,,] chunk_data = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    Vector3Int chunk_coord;

    public void Init(Vector3Int chunk_coord_in) {
        chunk_coord = chunk_coord_in;
        GenerateChunk();
    }

    void GenerateChunk() {
        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {

                float ground_height = 120;
                for (int y = 0; y < CHUNK_SIZE; ++y) {
                    if (y + chunk_coord.y < ground_height) { chunk_data[x, y, z] = 1; }
                    else { chunk_data[x, y, z] = 0; }
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
                        Vector3Int nbr = new Vector3Int(nbr_x, nbr_y, nbr_z);
                        if (GetLocalBlockType(nbr) == 1) { continue; }

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
        gameObject.GetComponent<MeshRenderer>().material = World.material;
    }

    static bool LocalPositionIsInChunk(Vector3Int position) {
        return 0 <= position.x && position.x < CHUNK_SIZE && 
               0 <= position.y && position.y < CHUNK_SIZE &&
               0 <= position.z && position.z < CHUNK_SIZE;
    }

    public Vector3Int GetGlobalPosition(Vector3Int position) {
        return (chunk_coord * CHUNK_SIZE) + position;
    }

    public int GetLocalBlockType(Vector3Int position) {
        if (LocalPositionIsInChunk(position)) return chunk_data[position.x, position.y, position.z];
        return 0;

        // return World.GetGlobalBlockType(GetGlobalPosition(position));
    }
}
