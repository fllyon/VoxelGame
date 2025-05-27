using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {

    static int3 player_chunk;

    void Awake() {
        int middle = (ChunkManager.WORLD_SIZE_IN_CHUNKS >> 1) * ChunkData.CHUNK_SIZE;
        transform.position = new Vector3(middle, 400, middle);
        player_chunk = Utility.GetChunkCoord(new int3(middle, 400, middle));
        new int3(0, 0, 1).Vector3();
    }

    public static float ChunkDistanceFromPlayer(int3 chunk_coord) {
        return math.distance(chunk_coord, player_chunk);
    }
}
