using System;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour {

    public static int WORLD_HEIGHT_IN_CHUNKS = 16;
    public static int WORLD_WIDTH_IN_CHUNKS = 8;
    public static int WORLD_HEIGHT { get { return WORLD_HEIGHT_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int WORLD_WIDTH { get { return WORLD_WIDTH_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int HALF_WORLD_HEIGHT_IN_CHUNKS { get { return WORLD_HEIGHT_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_WIDTH_IN_CHUNKS { get { return WORLD_WIDTH_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_HEIGHT { get { return WORLD_HEIGHT >> 1; } }
    public static int HALF_WORLD_WIDTH { get { return WORLD_WIDTH >> 1; } }

    public static Global_Coord GetChunkReadablePosition(Global_Coord position) { return position - new Global_Coord(HALF_WORLD_WIDTH_IN_CHUNKS, HALF_WORLD_WIDTH_IN_CHUNKS, 0); }
    public static Global_Coord GetPlayerReadablePosition(Global_Coord position) { return position - new Global_Coord(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, 0); }
    public static Global_Coord GetChunkWorldPosition(Global_Coord position) { return GetChunkReadablePosition(position) * Chunk.CHUNK_SIZE; }
    public static int GetChunkHash(Global_Coord position) { return Tuple.Create(position.x, position.y, position.z).GetHashCode(); }

    public static Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();

    public static Material material;
    public static Data data;

    public static Global_Coord player_coord;


    void Start() {

        material = Resources.Load<Material>("BlockTextures");
        data = new Data();

        for (int y = 0; y < WORLD_HEIGHT_IN_CHUNKS; ++y) {
            for (int x = 0; x < WORLD_WIDTH_IN_CHUNKS; ++x) {
                for (int z = 0; z < WORLD_WIDTH_IN_CHUNKS; ++z) {
                    
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

        float forest = 0.5f + WorldGen.GetForestWeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH);
        float desert = 0.5f + WorldGen.GetDesertWeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH);
        player_coord = new Global_Coord(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, WorldGen.GetBiomeMeshedHeight(HALF_WORLD_WIDTH, HALF_WORLD_WIDTH, forest, desert) + 2);
        GameObject.Find("Player").GetComponent<Transform>().position = GetPlayerReadablePosition(player_coord).ToVec3();

    }

    public static void PlayerMovedChunks() {
        Global_Coord player_chunk = GetChunkPosition(player_coord);
        foreach (Chunk chunk in chunks.Values) {
            if (Math.Abs(chunk.chunk_coord.x - player_chunk.x) < 2 ||
                Math.Abs(chunk.chunk_coord.z - player_chunk.z) < 2 ||
                Math.Abs(chunk.chunk_coord.y - player_chunk.y) < 2) {
                    chunk.ReconsiderFaces();
            }
        }
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
        return chunks[Tuple.Create(chunk_coord.x, chunk_coord.y, chunk_coord.z).GetHashCode()]
                   .GetLocalBlockType(GetLocalPosition(position - (chunk_coord * Chunk.CHUNK_SIZE)));
    }

}