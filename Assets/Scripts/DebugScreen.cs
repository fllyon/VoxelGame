using TMPro;
using UnityEngine;


public class DebugScreen : MonoBehaviour {

    World world;
    bool viewing;
    TextMeshProUGUI text;
    string program_name;
    float framerate;
    float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        viewing = false;

        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TextMeshProUGUI>();
        Debug.Log(text);

        program_name = "Flynn Lyon - Voxel Engine\n";
        framerate = 0;
        timer = 0;
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.F3)) { viewing = !viewing; }
        if (!viewing) { 
            text.text = "";
            return;
        }

        // Set text of debug screen
        Global_Coord player = World.player_coord;

        string text_output = program_name;
        text_output += "Framerate: " + framerate + " fps\n";
        text_output += "Position (x, y, z): (" + player.x + ", " + player.y + ", " + player.z + ")\n";
        text.text = text_output;

        // Calculate framerate
        if (timer > 1.0f) {
            framerate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        } else {
            timer += Time.unscaledDeltaTime;
        }
    }
}
