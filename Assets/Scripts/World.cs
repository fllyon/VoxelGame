using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class World : MonoBehaviour {

    public static int WORLD_HEIGHT_IN_CHUNKS = 1;
    public static int WORLD_WIDTH_IN_CHUNKS = 4;
    public static int WORLD_HEIGHT { get { return WORLD_HEIGHT_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int WORLD_WIDTH { get { return WORLD_WIDTH_IN_CHUNKS << Chunk.CHUNK_SIZE_BIT_SHIFT; } }
    public static int HALF_WORLD_HEIGHT_IN_CHUNKS { get { return WORLD_HEIGHT_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_WIDTH_IN_CHUNKS { get { return WORLD_WIDTH_IN_CHUNKS >> 1; } }
    public static int HALF_WORLD_HEIGHT { get { return WORLD_HEIGHT >> 1; } }
    public static int HALF_WORLD_WIDTH { get { return WORLD_WIDTH >> 1; } }

    // We should always keep the dictionary position as the default, these should be a temporary use
    public static Vector3Int GetChunkReadablePosition(Vector3Int position) { return position - new Vector3Int(HALF_WORLD_WIDTH_IN_CHUNKS, 0, HALF_WORLD_WIDTH_IN_CHUNKS); }
    public static Vector3Int GetChunkWorldPosition(Vector3Int position) { return GetChunkReadablePosition(position) * new Vector3Int(Chunk.CHUNK_SIZE, 0, Chunk.CHUNK_SIZE); }
    public static int GetChunkHash(Vector3Int position) { return Tuple.Create(position.x, position.y, position.z).GetHashCode(); }

    public static Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();
    public static Material material;
    public static Data data;


    void Start() {

        material = Resources.Load<Material>("BlockTextures");
        data = new Data();

        for (int y = 0; y < WORLD_HEIGHT_IN_CHUNKS; ++y) {
            for (int x = 0; x < WORLD_WIDTH_IN_CHUNKS; ++x) {
                for (int z = 0; z < WORLD_WIDTH_IN_CHUNKS; ++z) {
                    
                    GameObject curr_chunk = new GameObject("chunk ("+x+" "+y+" "+z+")");
                    Vector3Int curr_coords = new Vector3Int(x, y, z);

                    curr_chunk.transform.position = GetChunkWorldPosition(curr_coords);
                    curr_chunk.AddComponent<MeshFilter>();
                    curr_chunk.AddComponent<MeshRenderer>();
                    curr_chunk.AddComponent<Chunk>();

                    Chunk curr_component = curr_chunk.GetComponent<Chunk>();
                    curr_component.Init(curr_coords);
                    chunks[GetChunkHash(curr_coords)] = curr_component;
                }
            }
        }
    }

    public static bool ChunkIsInWorld(Vector3Int position) {   
        return 0 <= position.x && position.x < WORLD_WIDTH_IN_CHUNKS && 
               0 <= position.y && position.y < WORLD_HEIGHT_IN_CHUNKS &&
               0 <= position.z && position.z < WORLD_WIDTH_IN_CHUNKS;
    }

    public static Vector3Int GetChunkPosition(Vector3Int position) {
        int x = position.x < 0 ? Mathf.CeilToInt(position.x / Chunk.CHUNK_SIZE) : Mathf.FloorToInt(position.x / Chunk.CHUNK_SIZE);
        int y = position.x < 0 ? Mathf.CeilToInt(position.y / Chunk.CHUNK_SIZE) : Mathf.FloorToInt(position.y / Chunk.CHUNK_SIZE);
        int z = position.x < 0 ? Mathf.CeilToInt(position.z / Chunk.CHUNK_SIZE) : Mathf.FloorToInt(position.z / Chunk.CHUNK_SIZE);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int GetLocalPosition(Vector3Int position) {
        return new Vector3Int(position.x % Chunk.CHUNK_SIZE, 
                              position.y % Chunk.CHUNK_SIZE, 
                              position.z % Chunk.CHUNK_SIZE);
    }
    

    public static int GetGlobalBlockType(Vector3Int position) {
        Vector3Int chunk_coord = GetChunkPosition(position);
        if (!ChunkIsInWorld(chunk_coord)) return 0;
        return chunks[Tuple.Create(chunk_coord.x, chunk_coord.y, chunk_coord.z).GetHashCode()]
                   .GetLocalBlockType(GetLocalPosition(position));
    }

}
