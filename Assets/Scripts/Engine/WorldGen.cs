using Unity.Burst;
using Unity.Mathematics;

public static class WorldGen {

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
    
}