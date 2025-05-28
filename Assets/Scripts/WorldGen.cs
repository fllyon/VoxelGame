using Unity.Burst;
using Unity.Mathematics;

public static class WorldGen {

    public static int GetBlendedTerrainHeight(int x, int z) {

        float forest = GetForestWeight(x, z);
        float desert = GetDesertWeight(x, z);
        float diff = math.abs(forest - desert);

        if (diff < 0.05f) {

            diff *= 10;
            float upper = 0.5f + diff;
            float lower = 0.5f - diff;
            upper *= upper *= upper;
            lower *= lower *= lower;

            if (desert > forest) { 
                desert = upper / (upper + lower);
                forest = lower / (upper + lower);
            } else {
                forest = upper / (upper + lower);
                desert = lower / (upper + lower);
            }

            int forest_surface = GetSurfaceHeight(x, z);
            int desert_surface = GetDesertSurfaceHeight(x, z);

            return (int)(forest_surface * forest + desert_surface * desert);
        } else if (desert > forest) { 
            return GetDesertSurfaceHeight(x, z); 
        } else { 
            return GetSurfaceHeight(x, z);
        }
    }

    // ============================================================= //
    //                        Forest Functions                       //
    // ============================================================= //

    const int forest_offset = 12345;
    const float forest_scale = 0.0025f;

    [BurstCompile]
    public static float GetForestWeight(int x, int z) {
        return noise.cnoise(new float2((x + forest_offset - 0.1f) * forest_scale, 
                                       (z + forest_offset - 0.1f) * forest_scale));
    }

    const int surface_base = 416;
    const int surface_variance = 48;
    const float surface_scale = 0.0125f;

    [BurstCompile]
    public static int GetSurfaceHeight(int x, int z) {
        return (int)(noise.cnoise(new float2((x + 0.1f) * surface_scale,
                                             (z + 0.1f) * surface_scale)) * surface_variance + surface_base);
    }

    // ============================================================= //
    //                        Ocean Functions                        //
    // ============================================================= //

    const int desert_offset = 8008135;
    const float desert_scale = 0.0025f;

    [BurstCompile]
    public static float GetDesertWeight(int x, int z) {
        return noise.cnoise(new float2((x + desert_offset - 0.1f) * desert_scale, 
                                       (z + desert_offset - 0.1f) * desert_scale));
    }

    const int desert_surface_base = 368;
    const int desert_surface_variance = 10;
    const float desert_surface_scale = 0.01f;

    [BurstCompile]
    public static int GetDesertSurfaceHeight(int x, int z) {
        return (int)(noise.cnoise(new float2((x - 0.1f) * desert_surface_scale, 
                                             (z - 0.1f) * desert_surface_scale)) * desert_surface_variance + desert_surface_base);
    }

}