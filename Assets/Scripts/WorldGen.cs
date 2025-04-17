using UnityEngine;

class WorldGen {

    // Eventually, have the WorldGen class read in files from json on launch
    // For now, hardcode values and call it a day

    // Surface Height
    const int surface_base = 416;
    const int surface_variance = 48;

    const float surface_scale = 0.0125f;

    public static int GetSurfaceHeight(int global_x, int global_z) {
        
        return surface_base + (int)(Perlin.Noise((global_x - 0.1f) * surface_scale, 
                                                 (global_z - 0.1f) * surface_scale) * surface_variance);
    }

    // Dirt Depth
    const int dirt_base = 5;
    const int dirt_variance = 3;

    const int dirt_offset = 500;
    const float dirt_scale = 0.1f;

    public static int GetDirtDepth(int global_x, int global_z) {
        return dirt_base + (int)(Perlin.Noise((global_x + dirt_offset - 0.1f) * dirt_scale, 
                                              (global_z + dirt_offset - 0.1f) * dirt_scale) * dirt_variance);
    }

    // Deepstone Height
    const int deepstone_base = 256;
    const int deepstone_variance = 10;

    const int deepstone_offset = -2500;
    const float deepstone_scale = 0.1f;

    public static int GetDeepstoneHeight(int global_x, int global_z) {
        return deepstone_base + (int)(Perlin.Noise((global_x + deepstone_offset - 0.1f) * deepstone_scale, 
                                                  (global_z + deepstone_offset - 0.1f) * deepstone_scale) * deepstone_variance);
    }

    // Hellstone Height
    const int hellstone_base = 96;
    const int hellstone_variance = 10;

    const int hellstone_offset = 4800;
    const float hellstone_scale = 0.1f;

    public static int GetHellstoneHeight(int global_x, int global_z) {
        return hellstone_base + (int)(Perlin.Noise((global_x + hellstone_offset - 0.1f) * hellstone_scale, 
                                                  (global_z + hellstone_offset - 0.1f) * hellstone_scale) * hellstone_variance);
    }

    // Cave Generation
    const float cave_scale = 0.08f;
    const int cave_offset = 32768;
    const float cave_threshold = 0.2f;
    
    public static bool IsCave(int global_x, int global_y, int global_z) {
        float output = Perlin.Noise((global_x + cave_offset) * cave_scale,
                                    (global_y + cave_offset) * cave_scale,
                                    (global_z + cave_offset) * cave_scale);
        return cave_threshold <= output;
    }
    
    public static bool IsHellCave(int global_x, int global_y, int global_z) {
        float output = Perlin.Noise((global_x + cave_offset) * cave_scale,
                                    (global_y + cave_offset) * cave_scale,
                                    (global_z + cave_offset) * cave_scale);
        return cave_threshold <= output;
    }

    // Generation functions for underground layers
    public static int GetStoneLayerBlock(int global_x, int global_y, int global_z) {
        if (IsCave(global_x, global_y, global_z)) { return 0; }
        return 4;
    }
    public static int GetDeepstoneLayerBlock(int global_x, int global_y, int global_z) {
        if (IsCave(global_x, global_y, global_z)) { return 0; }
        return 5;
    }
    public static int GetHellstoneLayerBlock(int global_x, int global_y, int global_z) {
        if (IsHellCave(global_x, global_y, global_z)) { return 0; }
        return 6;
    }
}