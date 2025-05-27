using Unity.Mathematics;

public static class WorldGen {

    const int surface_base = 416;
    const int surface_variance = 48;
    const float surface_scale = 0.0125f;

    public static int GetSurfaceHeight(int x, int z) {
        return (int)(noise.cnoise(new float2((x + 0.1f) * surface_scale,
                                             (z + 0.1f) * surface_scale)) * surface_variance + surface_base);
    }

}