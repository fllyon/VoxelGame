using Unity.Mathematics;
using UnityEngine;

public class World : MonoBehaviour {

    public static Transform world_object;

    public static Material material = null;
    public static Data.BlockData block_data;

    private static ChunkManager chunk_manager;
    private static ChunkScheduler chunk_scheduler;

    public static int3 player_chunk;

    // ============================================================= //
    //                        Unity Functions                        //
    // ============================================================= //

    void Awake() {

        world_object = transform;

        material = Resources.Load<Material>("BlockTextures");
        block_data = Data.LoadData();

        chunk_manager = new ChunkManager();
        chunk_scheduler = new ChunkScheduler(block_data);

        chunk_manager.chunk_scheduler = chunk_scheduler;
        chunk_scheduler.chunk_manager = chunk_manager;

        UpdatePlayerChunk(Player.chunk_pos);
    }

    void Update() {
        chunk_scheduler.Update();
    }

    void LateUpdate() {
        if (math.any(player_chunk != Player.chunk_pos)) { UpdatePlayerChunk(Player.chunk_pos); }
        chunk_scheduler.LateUpdate();  
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void UpdatePlayerChunk(int3 chunk_coord) {
        player_chunk = chunk_coord;
        chunk_manager.UpdatePlayerChunk(chunk_coord);
    }

    public void Dispose() {
        chunk_scheduler.Dispose();
        chunk_manager.Dispose();
        block_data.Dispose();
    }

}
