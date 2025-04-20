using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour {

    // To save runtime, hardcode bit shift instead of manually calculating
    public static int CHUNK_SIZE = 32;
    public static int CHUNK_SIZE_BIT_SHIFT = 5;

    int[,,] chunk_data = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    Vector3Int chunk_coord;
    Vector3Int chunk_pos;

    public void Init(Vector3Int chunk_coord_in) {
        chunk_coord = chunk_coord_in;
        chunk_pos = chunk_coord_in * CHUNK_SIZE;
        GenerateChunk();
    }

    void GenerateChunk() {

        // Uncomment to only render surface
        // if (chunk_coord.y < 10) return;

        for (int x = 0; x < CHUNK_SIZE; ++x) {
            for (int z = 0; z < CHUNK_SIZE; ++z) {

                // Determine Biome
                float forest = 0.5f + WorldGen.GetForestWeight(chunk_pos.x + x, chunk_pos.z + z);
                float desert = 0.5f + WorldGen.GetDesertWeight(chunk_pos.x + x, chunk_pos.z + z);

                int surface_height = WorldGen.GetBiomeMeshedHeight(chunk_pos.x + x, chunk_pos.z + z, forest, desert);
                int stone_height = surface_height - WorldGen.GetDirtDepth(chunk_pos.x + x, chunk_pos.z + z);
                int deepstone_height = WorldGen.GetDeepstoneHeight(chunk_pos.x + x, chunk_pos.z + z);
                int hellstone_height = WorldGen.GetHellstoneHeight(chunk_pos.x + x, chunk_pos.z + z);
                int hell_ceiling_height = WorldGen.GetHellCeilingHeight(chunk_pos.x + x, chunk_pos.z + z);
                int hell_floor_height = WorldGen.GetHellFloorHeight(chunk_pos.x + x, chunk_pos.z + z);

                if (forest > desert) {

                    int height = chunk_pos.y;
                    for (int y = 0; y < CHUNK_SIZE; ++y) {
                        if (height == 0) { chunk_data[x, y, z] = 1; }
                        else if (height < hell_floor_height) { chunk_data[x, y, z] = 6; }
                        else if (height < hell_ceiling_height) { chunk_data[x, y, z] = 0; }
                        else if (height < hellstone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 6); }
                        else if (height < deepstone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 5); }
                        else if (height < stone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 4); }
                        else if (height < surface_height) { chunk_data[x, y, z] = 3; }
                        else if (height == surface_height) { chunk_data[x, y, z] = 2; }
                        else { break; }

                        height += 1;
                    }

                } else {

                    int height = chunk_pos.y;
                    for (int y = 0; y < CHUNK_SIZE; ++y) {
                        if (height == 0) { chunk_data[x, y, z] = 1; }
                        else if (height < hell_floor_height) { chunk_data[x, y, z] = 6; }
                        else if (height < hell_ceiling_height) { chunk_data[x, y, z] = 0; }
                        else if (height < hellstone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 6); }
                        else if (height < deepstone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 5); }
                        else if (height < stone_height) { chunk_data[x, y, z] = WorldGen.GetUndergroundBlock(chunk_pos.x + x, chunk_pos.y + y, chunk_pos.z + z, 4); }
                        else if (height <= surface_height) { chunk_data[x, y, z] = 7; }
                        else { break; }

                        height += 1;
                    }

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
