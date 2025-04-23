using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class World : MonoBehaviour {

    public static int VIEW_DISTANCE = 6;
    public static int LOAD_DISTANCE = 7;

    public static int WORLD_HEIGHT_IN_CHUNKS = 16;
    public static int WORLD_WIDTH_IN_CHUNKS = 16;
    public static int WORLD_HEIGHT { get { return WORLD_HEIGHT_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int WORLD_WIDTH { get { return WORLD_WIDTH_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int HALF_WORLD_HEIGHT_IN_CHUNKS { get { return WORLD_HEIGHT_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_WIDTH_IN_CHUNKS { get { return WORLD_WIDTH_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_HEIGHT { get { return WORLD_HEIGHT >> 1; } }
    public static int HALF_WORLD_WIDTH { get { return WORLD_WIDTH >> 1; } }

    public static Global_Coord GetChunkReadablePosition(Global_Coord position) { return position - new Global_Coord(HALF_WORLD_WIDTH_IN_CHUNKS, HALF_WORLD_WIDTH_IN_CHUNKS, 0); }
    public static Global_Coord GetPlayerReadablePosition(Global_Coord position) { return position - new Global_Coord(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, 0); }
    public static Global_Coord GetChunkWorldPosition(Global_Coord position) { return GetChunkReadablePosition(position) * Chunk.CHUNK_SIZE; }
    public static int GetChunkHash(Global_Coord position) { return position.GetHashCode(); }

    public static Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();

    public static Material material;
    public static Data data;

    public static Global_Coord player_coord;


    void Start() {

        material = Resources.Load<Material>("BlockTextures");
        data = new Data();

        float forest = 0.5f + WorldGen.GetForestWeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH);
        float desert = 0.5f + WorldGen.GetDesertWeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH);
        player_coord = new Global_Coord(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, WorldGen.GetBiomeMeshedHeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, forest, desert) + 2);
        GameObject.Find("Player").GetComponent<Transform>().position = GetPlayerReadablePosition(player_coord).ToVec3();

        Global_Coord player_chunk = GetChunkPosition(player_coord);
        for (int x = player_chunk.x - LOAD_DISTANCE; x < player_chunk.x + LOAD_DISTANCE; ++x) {
            for (int z = player_chunk.z - LOAD_DISTANCE; z < player_chunk.z + LOAD_DISTANCE; ++z) {
                for (int y = player_chunk.y - LOAD_DISTANCE; y < player_chunk.y + LOAD_DISTANCE; ++y) {
                    
                    GameObject curr_chunk = new GameObject("chunk ("+x+" "+y+" "+z+")");
                    Global_Coord curr_coords = new Global_Coord(x, z, y);

                    curr_chunk.transform.position = GetChunkWorldPosition(curr_coords).ToVec3();
                    curr_chunk.AddComponent<Chunk>();

                    Chunk curr_component = curr_chunk.GetComponent<Chunk>();
                    curr_component.Init(curr_coords);
                    chunks[GetChunkHash(curr_coords)] = curr_component;

                }
            }
        }

        for (int x = player_chunk.x - VIEW_DISTANCE; x < player_chunk.x + VIEW_DISTANCE; ++x) {
            for (int z = player_chunk.z - VIEW_DISTANCE; z < player_chunk.z + VIEW_DISTANCE; ++z) {
                for (int y = player_chunk.y - VIEW_DISTANCE; y < player_chunk.y + VIEW_DISTANCE; ++y) {

                    Chunk chunk = chunks[GetChunkHash(new Global_Coord(x, z, y))];
                    chunk.GenerateMesh();

                }
            }
        }

    }

    public static void PlayerMovedChunks() {
        Global_Coord player_chunk = GetChunkPosition(player_coord);
        HashSet<int> chunks_to_remove = new HashSet<int>(chunks.Keys);

        // Load/Unload all necessary chunks
        for (int x = player_chunk.x - LOAD_DISTANCE; x < player_chunk.x + LOAD_DISTANCE; ++x) {
            for (int z = player_chunk.z - LOAD_DISTANCE; z < player_chunk.z + LOAD_DISTANCE; ++z) {
                for (int y = player_chunk.y - LOAD_DISTANCE; y < player_chunk.y + LOAD_DISTANCE; ++y) {

                    Global_Coord chunk_coord = new Global_Coord(x, z, y);
                    int chunk_hash = GetChunkHash(chunk_coord);

                    // If the chunk exists
                    if (chunks.ContainsKey(chunk_hash)) {

                        if (Math.Abs(player_chunk.x - x) > VIEW_DISTANCE &&
                            Math.Abs(player_chunk.z - z) > VIEW_DISTANCE &&
                            Math.Abs(player_chunk.y - y) > VIEW_DISTANCE) { 
                                chunks[chunk_hash].DisableMesh();
                        }

                        chunks_to_remove.Remove(chunk_hash);
                        
                    } else {

                        GameObject curr_chunk = new GameObject("chunk ("+x+" "+y+" "+z+")");
                        Global_Coord curr_coords = new Global_Coord(x, z, y);

                        curr_chunk.transform.position = GetChunkWorldPosition(curr_coords).ToVec3();
                        curr_chunk.AddComponent<Chunk>();

                        Chunk curr_component = curr_chunk.GetComponent<Chunk>();
                        curr_component.Init(curr_coords);
                        chunks[GetChunkHash(curr_coords)] = curr_component;

                    }
                }
            }
        }

        // Render all chunks within view distance
        for (int x = player_chunk.x - VIEW_DISTANCE; x < player_chunk.x + VIEW_DISTANCE; ++x) {
            for (int z = player_chunk.z - VIEW_DISTANCE; z < player_chunk.z + VIEW_DISTANCE; ++z) {
                for (int y = player_chunk.y - VIEW_DISTANCE; y < player_chunk.y + VIEW_DISTANCE; ++y) {
                    Chunk chunk = chunks[GetChunkHash(new Global_Coord(x, z, y))];

                    if (!chunk.mesh_generated) { chunk.GenerateMesh(); }

                    // If the chunk needs to reconsider its mesh faces, do that
                    Global_Coord chunk_coord = new Global_Coord(x, z, y);
                    if (Math.Abs(chunk_coord.x - player_chunk.x) < 2 ||
                        Math.Abs(chunk_coord.z - player_chunk.z) < 2 ||
                        Math.Abs(chunk_coord.y - player_chunk.y) < 2) {
                            chunk.ReconsiderFaces();
                    }
                }
            }
        }

        // Delete all chunks not within the load distance
        foreach (int chunk_hash in chunks_to_remove) {
            Destroy(chunks[chunk_hash].gameObject);
            chunks.Remove(chunk_hash);
        }
        chunks_to_remove.Clear();
    }

    public static bool ChunkIsInWorld(Global_Coord position) {   
        return 0 <= position.x && position.x < WORLD_WIDTH_IN_CHUNKS && 
               0 <= position.z && position.z < WORLD_WIDTH_IN_CHUNKS &&
               0 <= position.y && position.y < WORLD_HEIGHT_IN_CHUNKS;
    }

    public static Global_Coord GetChunkPosition(Global_Coord position) {
        return new Global_Coord(Mathf.FloorToInt(position.x / (float)Chunk.CHUNK_SIZE),
                              Mathf.FloorToInt(position.z / (float)Chunk.CHUNK_SIZE),
                              Mathf.FloorToInt(position.y / (float)Chunk.CHUNK_SIZE));
    }

    public static Coord GetLocalPosition(Global_Coord position) {
        return new Coord((sbyte)(position.x % Chunk.CHUNK_SIZE), 
                         (sbyte)(position.z % Chunk.CHUNK_SIZE), 
                         (sbyte)(position.y % Chunk.CHUNK_SIZE));
    }
    
    public static BlockType GetGlobalBlockType(Global_Coord position) {
        Global_Coord chunk_coord = GetChunkPosition(position);
        if (!ChunkIsInWorld(chunk_coord)) return Data.blockTypes[0];
        return chunks[GetChunkHash(chunk_coord)].GetLocalBlockType(GetLocalPosition(position - (chunk_coord * Chunk.CHUNK_SIZE)));
    }

}