using System.Diagnostics;
using Unity.Burst;
using Unity.Mathematics;

public static class WorldGen {

    [BurstCompile]
    public static float GetNoise(float x, float z, float variance, int offset, float scale, int octaves) {

        float output = 0;
        x += offset + 0.01f;
        z += offset + 0.01f;

        for (int layer = 0; layer < octaves; ++layer) {
            output += variance * noise.cnoise(new float2(x * scale, z * scale));

            variance /= 2;
            scale *= 2;
        }

        return output;
    }

    [BurstCompile]
    public static float GetCellNoise(float x, float z, int offset, float scale) {
        x += (offset + 0.01f) * scale;
        z += (offset + 0.01f) * scale;
        return math.length(noise.cellular(new float2(x, z)));
    }

    // ============================================================= //
    //                            Forest                             //
    // ============================================================= //

    const int surface_base = 416;
    const int surface_variance = 25;
    const int surface_offset = 0;
    const float surface_scale = 0.0125f;
    const int surface_octaves = 4;
    
    public static int GetSurfaceHeight(int x, int z) {
        return surface_base + (int)GetNoise(x, z, surface_variance, surface_offset, surface_scale, surface_octaves);
    }

    // ============================================================= //
    //                             Tree                              //
    // ============================================================= //

    public const int tree_height = 10;

    const int tree_offset = 1047;
    const float tree_scale = 0.35f;
    const float tree_threshold = 0.7f;

    public static bool SpawnTreeHere(int x, int z) {
        return tree_threshold < GetNoise(x, z, 1, tree_offset, tree_scale, 1);
    }
    
}