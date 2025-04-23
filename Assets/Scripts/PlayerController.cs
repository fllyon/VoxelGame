using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour {
    Global_Coord last_chunk;
    float speed = 15;

    void Start() {
        GameObject.Find("Camera").GetComponent<Camera>().nearClipPlane = 0.1f;
        last_chunk = World.GetChunkPosition(World.player_coord);
    }

    void Update() {
        if (Input.GetKey(KeyCode.W)) { transform.Translate(Vector3.forward * speed * Time.deltaTime); }
        if (Input.GetKey(KeyCode.A)) { transform.Translate(Vector3.left * speed * Time.deltaTime); }
        if (Input.GetKey(KeyCode.S)) { transform.Translate(Vector3.back * speed * Time.deltaTime); }
        if (Input.GetKey(KeyCode.D)) { transform.Translate(Vector3.right * speed * Time.deltaTime); }
        if (Input.GetKey(KeyCode.Space)) { transform.Translate(Vector3.up * speed * Time.deltaTime); }
        if (Input.GetKey(KeyCode.LeftShift)) { transform.Translate(Vector3.down * speed * Time.deltaTime); }

        Global_Coord player_coord = new Global_Coord((int)transform.position.x + World.HALF_WORLD_WIDTH,
                                                     (int)transform.position.z + World.HALF_WORLD_WIDTH,
                                                     (int)transform.position.y);
        World.player_coord = player_coord;

        if (World.GetChunkPosition(player_coord) == last_chunk) { return; }

        World.PlayerMovedChunks();
    }
}
