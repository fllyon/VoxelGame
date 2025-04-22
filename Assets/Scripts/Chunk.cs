using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class Chunk : MonoBehaviour {

    // To save runtime, hardcode bit shift instead of manually calculating
    public static int CHUNK_SIZE = 32;
    public static int CHUNK_SIZE_BIT_SHIFT = 5;

    byte[,,] chunk_data = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    GameObject up, left, back, right, front, down;

    public Global_Coord chunk_coord;
    public Global_Coord chunk_pos;


    public void Init(Global_Coord chunk_coord_in) {
        chunk_coord = chunk_coord_in;
        chunk_pos = chunk_coord * CHUNK_SIZE;

        up = new GameObject();
        up.transform.parent = gameObject.transform;
        up.transform.localPosition = new Vector3(0, 0, 0);
        up.AddComponent<MeshFilter>();
        up.AddComponent<MeshRenderer>();
        up.name = "up";

        left = new GameObject();
        left.transform.parent = gameObject.transform;
        left.transform.localPosition = new Vector3(0, 0, 0);
        left.AddComponent<MeshFilter>();
        left.AddComponent<MeshRenderer>();
        left.name = "left";

        back = new GameObject();
        back.transform.parent = gameObject.transform;
        back.transform.localPosition = new Vector3(0, 0, 0);
        back.AddComponent<MeshFilter>();
        back.AddComponent<MeshRenderer>();
        back.name = "back";

        right = new GameObject();
        right.transform.parent = gameObject.transform;
        right.transform.localPosition = new Vector3(0, 0, 0);
        right.AddComponent<MeshFilter>();
        right.AddComponent<MeshRenderer>();
        right.name = "right";

        front = new GameObject();
        front.transform.parent = gameObject.transform;
        front.transform.localPosition = new Vector3(0, 0, 0);
        front.AddComponent<MeshFilter>();
        front.AddComponent<MeshRenderer>();
        front.name = "front";

        down = new GameObject();
        down.transform.parent = gameObject.transform;
        down.transform.localPosition = new Vector3(0, 0, 0);
        down.AddComponent<MeshFilter>();
        down.AddComponent<MeshRenderer>();
        down.name = "down";

        GenerateChunk();
    }

    void GenerateChunk() {

        // Uncomment to only render surface
        // if (chunk_coord.y < 10) return;

        for (byte x = 0; x < CHUNK_SIZE; ++x) {
            for (byte z = 0; z < CHUNK_SIZE; ++z) {

                int global_x = chunk_pos.x + x;
                int global_z = chunk_pos.z + z;

                // Determine Biome
                float forest = 0.5f + WorldGen.GetForestWeight(global_x, global_z);
                float desert = 0.5f + WorldGen.GetDesertWeight(global_x, global_z);

                int surface_height = WorldGen.GetBiomeMeshedHeight(global_x, global_z, forest, desert);
                int stone_height = surface_height - WorldGen.GetDirtDepth(global_x, global_z);
                int deepstone_height = WorldGen.GetDeepstoneHeight(global_x, global_z);
                int hellstone_height = WorldGen.GetHellstoneHeight(global_x, global_z);
                int hell_ceiling_height = WorldGen.GetHellCeilingHeight(global_x, global_z);
                int hell_floor_height = WorldGen.GetHellFloorHeight(global_x, global_z);

                if (forest > desert) {

                    short height = (short)chunk_pos.y;
                    for (byte y = 0; y < CHUNK_SIZE; ++y) {
                        if (height == 0) { chunk_data[x, z, y] = 1; }
                        else if (height < hell_floor_height) { chunk_data[x, z, y] = 6; }
                        else if (height < hell_ceiling_height) { chunk_data[x, z, y] = 0; }
                        else if (height < hellstone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 6); }
                        else if (height < deepstone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 5); }
                        else if (height < stone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 4); }
                        else if (height < surface_height) { chunk_data[x, z, y] = 3; }
                        else if (height == surface_height) { chunk_data[x, z, y] = 2; }
                        else { break; }
                        height += 1;
                    }

                } else {

                    short height = (short)chunk_pos.y;
                    for (byte y = 0; y < CHUNK_SIZE; ++y) {
                        if (height == 0) { chunk_data[x, z, y] = 1; }
                        else if (height < hell_floor_height) { chunk_data[x, z, y] = 6; }
                        else if (height < hell_ceiling_height) { chunk_data[x, z, y] = 0; }
                        else if (height < hellstone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 6); }
                        else if (height < deepstone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 5); }
                        else if (height < stone_height) { chunk_data[x, z, y] = WorldGen.GetUndergroundBlock(global_x, height, global_z, 4); }
                        else if (height <= surface_height) { chunk_data[x, z, y] = 7; }
                        else { break; }
                        height += 1;
                    }

                }
            }
        }
    }

    void Start() {

        MeshFilter[] mesh_filters = {
            up.GetComponent<MeshFilter>(),
            left.GetComponent<MeshFilter>(),
            back.GetComponent<MeshFilter>(),
            right.GetComponent<MeshFilter>(),
            front.GetComponent<MeshFilter>(),
            down.GetComponent<MeshFilter>(),
        };

        MeshRenderer[] mesh_renderers = {
            up.GetComponent<MeshRenderer>(),
            left.GetComponent<MeshRenderer>(),
            back.GetComponent<MeshRenderer>(),
            right.GetComponent<MeshRenderer>(),
            front.GetComponent<MeshRenderer>(),
            down.GetComponent<MeshRenderer>(),
        };

        int[] vertex_count = {0, 0, 0, 0, 0, 0};
        List<Vector3>[] vertices = {new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>()};
        List<Vector3>[] normals = {new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>()};
        List<Vector2>[] uvs = {new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>()};
        List<int>[] triangles = {new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>()};

        // For each block, add its face data to the relevant mesh
        for (sbyte x = 0; x < CHUNK_SIZE; ++x) {
            for (sbyte z = 0; z < CHUNK_SIZE; ++z) {
                for (sbyte y = 0; y < CHUNK_SIZE; ++y) {

                    if (chunk_data[x, z, y] == 0) { continue; }
                    BlockType blockType = Data.blockTypes[chunk_data[x, z, y]];

                    Coord block_position = new Coord(x, z, y);
                    for (byte face = 0; face < 6; ++face) {

                        sbyte nbr_x = (sbyte)(x + VoxelData.directions[face].x);
                        sbyte nbr_z = (sbyte)(z + VoxelData.directions[face].z);
                        sbyte nbr_y = (sbyte)(y + VoxelData.directions[face].y);
                        if (!GetLocalBlockType(new Coord(nbr_x, nbr_z, nbr_y)).isTransparent) { continue; }

                        int textureID = blockType.faces[face];

                        float xUV = VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE * (textureID % VoxelData.TEXTURE_ATLAS_SIZE);
                        float yUV = 1.0f - (textureID / VoxelData.TEXTURE_ATLAS_SIZE) - VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE;

                        for (int tri_idx = 0; tri_idx < 6; ++tri_idx) {
                            int vrtx_idx = VoxelData.Triangles[face, tri_idx];

                            vertices[face].Add(block_position.ToVec3() + VoxelData.Vertices[vrtx_idx]);
                            normals[face].Add(VoxelData.directions[face]);
                            uvs[face].Add(new Vector2(xUV, yUV) + (VoxelData.UVs[tri_idx] * VoxelData.NORMALISED_TEXTURE_ATLAS_SIZE));
                            triangles[face].Add(vertex_count[face]);
                            vertex_count[face]++;
                        }
                    }
                
                }
            }
        }

        for (int face = 0; face < 6; ++face) {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices[face].ToArray();
            mesh.normals = normals[face].ToArray();
            mesh.uv = uvs[face].ToArray();
            mesh.triangles = triangles[face].ToArray();

            mesh_filters[face].mesh = mesh;
            mesh_renderers[face].material = World.material;
        }

        ReconsiderFaces();
    }

    public void ReconsiderFaces() {
        up.SetActive(World.player_coord.y >= chunk_pos.y);
        left.SetActive(World.player_coord.z <= chunk_pos.z+32);
        back.SetActive(World.player_coord.x <= chunk_pos.x+32);
        right.SetActive(World.player_coord.z >= chunk_pos.z);
        front.SetActive(World.player_coord.x >= chunk_pos.x);
        down.SetActive(World.player_coord.y <= chunk_pos.y+32);
    }

    static bool LocalPositionIsInChunk(Coord position) {
        return 0 <= position.x && position.x < CHUNK_SIZE &&
               0 <= position.z && position.z < CHUNK_SIZE &&
               0 <= position.y && position.y < CHUNK_SIZE ;
    }

    public Global_Coord GetGlobalPosition(Coord position) {
        return chunk_pos + position;
    }

    public BlockType GetLocalBlockType(Coord position) {
        if (LocalPositionIsInChunk(position)) return Data.blockTypes[chunk_data[position.x, position.z, position.y]];
        return World.GetGlobalBlockType(GetGlobalPosition(position));
    }
}
