using UnityEngine;

public static class VoxelData {
    
    public static readonly Vector3Int[] Vertices = new Vector3Int[8] {
        new Vector3Int(0, 1, 0), new Vector3Int(0, 1, 1), new Vector3Int(1, 1, 1), new Vector3Int(1, 1, 0), 
        new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0)
    };

    public static readonly Vector2[] UVs = new Vector2[6] {
        new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2(0, 0)
    };

    public static readonly int[,] Triangles = new int[6,6] {
        {0,  1,  2,  2,  3,  0}, // Top
        {4,  0,  3,  3,  7,  4}, // Front
        {5,  1,  0,  0,  4,  5}, // Left
        {6,  2,  1,  1,  5,  6}, // Back
        {7,  3,  2,  2,  6,  7}, // Right
        {5,  4,  7,  7,  6,  5}  // Bottom
    };

    public static readonly Vector3Int[] directions = new Vector3Int[6] {
        new Vector3Int(0, 1, 0), new Vector3Int(0, 0, -1), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0)
    };

}
