using UnityEngine;

public class GameManager : MonoBehaviour {

    public World world { get; private set; }

    // ============================================================= //
    //                      Component Functions                      //
    // ============================================================= //

    void Awake() {
        SpawnWorld();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void SpawnWorld() {
        GameObject world_object = new GameObject("World");
        world_object.transform.position = Vector3.zero;
        world = world_object.AddComponent<World>();
    }
    
}
