using UnityEngine;

public static class VoxelData {

    public static int TEXTURE_ATLAS_SIZE = 16;
    public static float NORMALISED_TEXTURE_ATLAS_SIZE { get { return 1f / TEXTURE_ATLAS_SIZE; } }
    
    public static readonly Vector3Int[] Vertices = new Vector3Int[8] {
        new Vector3Int(0, 1, 0), new Vector3Int(0, 1, 1), new Vector3Int(1, 1, 1), new Vector3Int(1, 1, 0), 
        new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0)
    };

    public static readonly Vector2[] UVs = new Vector2[6] {
        new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(0, 0)
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

public struct Coord {
    public sbyte x;
    public sbyte z;
    public sbyte y;

    public Coord(sbyte x_in, sbyte z_in, sbyte y_in) { x = x_in; y = y_in; z = z_in; }
    public Coord(Global_Coord coord) { x = (sbyte)coord.x; z = (sbyte)coord.z; y = (sbyte)coord.y; }
    public Coord(Vector3Int coord) { x = (sbyte)coord.x; z = (sbyte)coord.z; y = (sbyte)coord.y; }
}

public struct Global_Coord {
    public int x;
    public int z;
    public int y;

    public Global_Coord(int x_in, int z_in, int y_in) { x = x_in; y = y_in; z = z_in; }
    public Global_Coord(Global_Coord coord) { x = coord.x; z = coord.z; y = coord.y; }
    public Global_Coord(Vector3Int coord) { x = coord.x; z = coord.z; y = coord.y; }

    public static Global_Coord operator *(Global_Coord coord, float multiplier) { return new Global_Coord((int)(coord.x * multiplier), (int)(coord.z * multiplier), (int)(coord.y * multiplier)); }
    public static Global_Coord operator +(Global_Coord global_coord, Coord local_coord) { return new Global_Coord(global_coord.x + local_coord.x, global_coord.z + local_coord.z, global_coord.y + local_coord.y); }
    public static Global_Coord operator -(Global_Coord global_coord, Global_Coord local_coord) { return new Global_Coord(global_coord.x - local_coord.x, global_coord.z - local_coord.z, global_coord.y - local_coord.y); }
}
