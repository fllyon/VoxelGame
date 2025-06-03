using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Diagnostics;

public class Player : MonoBehaviour {

    private static float move_speed = 5f;
    public static int3 chunk_pos;

    void Awake() {
        int middle = (WorldSettings.WORLD_SIZE_IN_CHUNKS >> 1) * 32 + 16;
        int height = WorldGen.GetSurfaceHeight(middle, middle);
        transform.position = new Vector3(middle, height, middle) + new Vector3(0.5f, 1f, 0.5f);
        chunk_pos = Utility.GetChunkCoord(new int3(middle, height, middle));
    }

    void Update() {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0f, moveZ) * move_speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space)) move.y = 1f * move_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift)) move.y = -1f * move_speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Escape)) {

            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects) { Destroy(obj); }
            Application.Quit();
            
        }

        transform.Translate(move, Space.World);
        chunk_pos = Utility.GetChunkCoord(transform.position.Int3());

    }

    public static float ChunkDistanceFromPlayer(int3 chunk_coord) {
        return math.length(chunk_coord - chunk_pos);
    }
}
