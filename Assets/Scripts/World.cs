using Unity.Mathematics;
using UnityEngine;

public class World : MonoBehaviour {
    public static World instance = null;

    private float3 player_chunk;
    private ChunkManager chunk_manager;

    private Data.BlockData block_data;

    // ============================================================= //
    //                      Component Functions                      //
    // ============================================================= //

    void Awake() {
        if (instance != null && instance != this) { Destroy(this); }
        instance = this;

        block_data = Data.LoadData();
        chunk_manager = new ChunkManager(block_data);
    }

    void OnDestroy() {
        chunk_manager.Dispose();
        block_data.Dispose();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

}
