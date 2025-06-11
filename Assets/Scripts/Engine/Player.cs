using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Diagnostics;

public class Player : MonoBehaviour {

    public static int3 position;
    public static int3 chunk_pos;

    private float pitch = 0f;

    [Header("Movement Settings")]
    private float move_speed = 10f;
    private float mouse_sensitivity = 8f;
    [SerializeField] private Transform camera_transform;
    

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SpawnPlayer();
    }

    void Update() {
        if (Input.GetKey(KeyCode.Escape)) { GameManager.QuitGame(); }
        MovePlayer();
    }

    public static float ChunkDistanceFromPlayer(int3 chunk_coord) {
        return math.length(chunk_coord - chunk_pos);
    }

    private void SpawnPlayer() {
        int middle = (WorldSettings.WORLD_SIZE_IN_CHUNKS >> 1) * 32 + 16;
        int height = WorldGen.GetSurfaceHeight(middle, middle);
        transform.position = new Vector3(middle, height, middle) + new Vector3(0.5f, 1f, 0.5f);
        SetPosition();
    }

    private void SetPosition() {
        position = transform.position.Int3();
        chunk_pos = Utility.GetChunkCoord(position);
    }

    private void MovePlayer() {

        // Move player
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        if (vertical != 0 || horizontal != 0) {
            Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * move_speed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }

        // Rotate Camera
        horizontal = Input.GetAxis("Mouse X");
        if (horizontal != 0) { transform.Rotate(Vector3.up * horizontal * mouse_sensitivity); }

        vertical = Input.GetAxis("Mouse Y");
        if (vertical != 0) {
            pitch = Mathf.Clamp(pitch - (vertical * mouse_sensitivity), -89.9f, 89.9f);
            camera_transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
}
