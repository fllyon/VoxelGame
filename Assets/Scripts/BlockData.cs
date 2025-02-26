using UnityEngine;

public static class BlockData {
    
    public static Vector3[] Vertices = {
        new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), // Top
    };

    public static Vector2[] UV = {
        new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0),  // Top
    };

    public static int[] Triangles = {
        0, 1, 2, 2, 3, 0, // Top
    };

}
