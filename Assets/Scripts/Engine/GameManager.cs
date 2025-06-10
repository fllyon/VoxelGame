using UnityEngine;

public class GameManager : MonoBehaviour {

    public World world { get; private set; }

    // ============================================================= //
    //                      Component Functions                      //
    // ============================================================= //

    void Awake() {
        SpawnWorld();
    }

    void OnApplicationQuit() {
        world.Dispose();
    }

    // ============================================================= //
    //                       Utility Functions                       //
    // ============================================================= //

    private void SpawnWorld() {
        GameObject world_object = new GameObject("World");
        world_object.transform.position = Vector3.zero;
        world = world_object.AddComponent<World>();
    }

    public static void QuitGame() {
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects) { Destroy(obj); }
        Application.Quit();
    }
    
}
