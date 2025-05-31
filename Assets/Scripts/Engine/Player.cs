using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour {

    public static int3 chunk_pos;

    void Awake() {
        int middle = (WorldSettings.WORLD_SIZE_IN_CHUNKS >> 1) * 32 + 16;
        int height = WorldGen.GetSurfaceHeight(middle, middle);
        transform.position = new Vector3(middle, height, middle) + new Vector3(0.5f, 1f, 0.5f);
        chunk_pos = Utility.GetChunkCoord(new int3(middle, height, middle));
    }

    public static float ChunkDistanceFromPlayer(int3 chunk_coord) {
        return math.length(chunk_coord - chunk_pos);
    }
}
