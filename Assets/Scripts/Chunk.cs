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
    Vector3Int chunk_pos;

    public void Init(Vector3Int chunk_coord_in) {
        chunk_coord = chunk_coord_in;
        chunk_pos = chunk_coord_in * CHUNK_SIZE;
        GenerateChunk();
    }

    void GenerateChunk() {
        
        
        
        

        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {

                int variance = 48;
                float scale = 0.0125f;
                int base_height = 208;
                float var = Perlin.Noise((chunk_pos.x + x) * scale, (chunk_pos.z + z) * scale);
                int ground_height = base_height + (int)(variance * var);

                int min_dirt = 5;
                int dirt_offset = 50;
                int dirt_variance = 5;
                float dirt_scale = 0.05f;
                int dirt_size = (int)(Perlin.Noise((chunk_pos.x + x + dirt_offset) * dirt_scale, (chunk_pos.z + z + dirt_offset) * dirt_scale) * dirt_variance);
                int stone_height = ground_height - min_dirt - dirt_size;

                for (int y = 0; y < CHUNK_SIZE; ++y) {
                    int height = chunk_pos.y + y;
                    if (height == 0) { chunk_data[x, y, z] = 1; }
                    else if (height < stone_height) { chunk_data[x, y, z] = 4; }
                    else if (height < ground_height) { chunk_data[x, y, z] = 3; }
                    else if (height == ground_height) { chunk_data[x, y, z] = 2; }
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
                    BlockType blockType = Data.blockTypes[chunk_data[x, y, z]];

                    Vector3 block_position = new Vector3(x, y, z);
                    for (int face = 0; face < 6; ++face) {

                        int nbr_x = x + VoxelData.directions[face].x;
                        int nbr_y = y + VoxelData.directions[face].y;
                        int nbr_z = z + VoxelData.directions[face].z;
                        Vector3Int nbr = new Vector3Int(nbr_x, nbr_y, nbr_z);
                        if (!GetLocalBlockType(nbr).isTransparent) { continue; }

                        int textureID = blockType.faces[face];

                        float xUV = VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE * (textureID % VoxelData.TEXTURE_ATLAS_SIZE);
                        float yUV = 1.0f - (textureID / VoxelData.TEXTURE_ATLAS_SIZE) - VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE;

                        for (int tri_idx = 0; tri_idx < 6; ++tri_idx) {

                            int vrtx_idx = VoxelData.Triangles[face, tri_idx];
                            vertices.Add(block_position + VoxelData.Vertices[vrtx_idx]);
                            uvs.Add(new Vector2(xUV, yUV) + (VoxelData.UVs[tri_idx] * VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE));

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
        return chunk_pos + position;
    }

    public BlockType GetLocalBlockType(Vector3Int position) {
        if (LocalPositionIsInChunk(position)) return Data.blockTypes[chunk_data[position.x, position.y, position.z]];
        return World.GetGlobalBlockType(GetGlobalPosition(position));
    }
}
