using UnityEngine;

public class GameManager : MonoBehaviour {

    public World world { get; private set; }


    void Awake() { }

    void Start() {
        GameObject world_object = new GameObject("World");
        world_object.transform.position = Vector3.zero;
        world = world_object.AddComponent<World>();
    }

    void Update() { }
    
}
