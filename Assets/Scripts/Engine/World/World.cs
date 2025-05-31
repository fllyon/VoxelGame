using Unity.Mathematics;
using UnityEngine;

public class World : MonoBehaviour {

    public static Data.BlockData block_data;

    private static ChunkManager chunk_manager;
    private static ChunkScheduler chunk_scheduler;

    public static int3 player_chunk;

    // ============================================================= //
    //                        Unity Functions                        //
    // ============================================================= //

    void Awake() {

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
        if (math.all(player_chunk != Player.chunk_pos)) { UpdatePlayerChunk(Player.chunk_pos); }
        chunk_scheduler.LateUpdate();  
    }

    void OnDestroy() {
        chunk_manager.Dispose();
        chunk_scheduler.Dispose();
        block_data.Dispose();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void UpdatePlayerChunk(int3 chunk_coord) {
        player_chunk = chunk_coord;
        chunk_manager.UpdatePlayerChunk();
    }

}
