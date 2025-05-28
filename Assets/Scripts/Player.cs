using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {

    static int3 player_chunk;

    void Awake() {
        int middle = (ChunkManager.WORLD_SIZE_IN_CHUNKS >> 1) * ChunkData.CHUNK_SIZE + (ChunkData.CHUNK_SIZE / 2);
        int height = WorldGen.GetBlendedTerrainHeight(middle, middle);
        transform.position = new Vector3(middle, height, middle) + new Vector3(0.5f, 1f, 0.5f);
        player_chunk = Utility.GetChunkCoord(new int3(middle, height, middle));
    }

    public static float ChunkDistanceFromPlayer(int3 chunk_coord) {
        return math.distance(chunk_coord, player_chunk);
    }
}
