using Unity.Burst;
using Unity.Mathematics;

public static class WorldGen {

    // ============================================================= //
    //                        Forest Functions                       //
    // ============================================================= //

    const int forest_offset = 12345;
    const float forest_scale = 0.0025f;

    [BurstCompile]
    public static float GetForestWeight(int x, int y) {
        return noise.snoise(new float2((x + forest_offset - 0.1f) * forest_scale, 
                                       (y + forest_offset - 0.1f) * forest_scale));
    }

    const int surface_base = 416;
    const int surface_variance = 48;
    const float surface_scale = 0.0125f;

    [BurstCompile]
    public static int GetSurfaceHeight(int x, int z) {
        return (int)(noise.snoise(new float2((x + 0.1f) * surface_scale,
                                             (z + 0.1f) * surface_scale)) * surface_variance + surface_base);
    }

    // ============================================================= //
    //                        Desert Functions                       //
    // ============================================================= //

    const int desert_offset = 8008135;
    const float desert_scale = 0.0025f;

    [BurstCompile]
    public static float GetDesertWeight(int x, int y) {
        return noise.snoise(new float2((x + desert_offset - 0.1f) * desert_scale, 
                                       (y + desert_offset - 0.1f) * desert_scale));
    }

    const int desert_surface_base = 368;
    const int desert_surface_variance = 10;
    const float desert_surface_scale = 0.01f;

    [BurstCompile]
    public static int GetDesertSurfaceHeight(int x, int z) {
        return (int)(noise.snoise(new float2((x - 0.1f) * desert_surface_scale, 
                                             (z - 0.1f) * desert_surface_scale)) * desert_surface_variance + desert_surface_base);
    }

}