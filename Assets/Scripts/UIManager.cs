using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class UIManager : MonoBehaviour {

    bool debug_active;
    [SerializeField] GameObject debug_object;
    TextMeshProUGUI debug_text;

    [SerializeField] Player player;

    void Start() {
        debug_active = debug_object.activeInHierarchy;
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.F3)) {
            debug_active = !debug_active;
            debug_object.SetActive(debug_active);
        }

        if (!debug_active) { return; }

        string text = $"Position: ({Player.position.x}, {Player.position.y}, {Player.position.z})\n" + 
                        $"Chunk: ({Player.chunk_pos.x}, {Player.chunk_pos.y}, {Player.chunk_pos.z})\n";
        
        debug_object.GetComponent<TextMeshProUGUI>().text = text;
        debug_object.SetActive(true);

    }
}
