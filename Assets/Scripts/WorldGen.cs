using System;
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

    // Desert Surface Height
    const int desert_surface_base = 416;
    const int desert_surface_variance = 16;

    const float desert_surface_scale = 0.01f;

    public static int GetDesertSurfaceHeight(int global_x, int global_z) {
        
        return desert_surface_base + (int)(Perlin.Noise((global_x - 0.1f) * desert_surface_scale, 
                                                        (global_z - 0.1f) * desert_surface_scale) * desert_surface_variance);
    }

    // Sand Depth
    const int sand_base = 5;
    const int sand_variance = 3;

    const int sand_offset = 500;
    const float sand_scale = 0.1f;

    public static int GetSandDepth(int global_x, int global_z) {
        return sand_base + (int)(Perlin.Noise((global_x + sand_offset - 0.1f) * sand_scale, 
                                              (global_z + sand_offset - 0.1f) * sand_scale) * sand_variance);
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
    
    // Hell Ceiling Generation
    const int hell_ceiling_height = 80;
    const int hell_ceiling_variance = 6;

    const int hell_ceiling_offset = 5000;
    const float hell_ceiling_scale = 0.1f;

    public static int GetHellCeilingHeight(int global_x, int global_z) {
        return hell_ceiling_height + (int)(Perlin.Noise((global_x + hell_ceiling_offset - 0.1f) * hell_ceiling_scale, 
                                                        (global_z + hell_ceiling_offset - 0.1f) * hell_ceiling_scale) * hell_ceiling_variance);
    }

    // Hell Floor Generation
    const int hell_floor_height = 15;
    const int hell_floor_variance = 12;

    const int hell_floot_offset = 2400;
    const float hell_floor_scale = 0.05f;

    public static int GetHellFloorHeight(int global_x, int global_z) {
        return hell_floor_height + (int)(Perlin.Noise((global_x + hell_floot_offset - 0.1f) * hell_floor_scale, 
                                                      (global_z + hell_floot_offset - 0.1f) * hell_floor_scale) * hell_floor_variance);
    }

    // Cave Generation
    public static int GetUndergroundBlock(int global_x, int global_y, int global_z, int block_id) {
        if (IsCave(global_x, global_y, global_z)) { return 0; }
        return block_id;
    }

    // Biome Values
    const int forest_offset = 12345;
    const float forest_scale = 0.005f;
    public static float GetForestWeight(int global_x, int global_y) {
        return Perlin.Noise((global_x + forest_offset - 0.1f) * forest_scale, 
                            (global_y + forest_offset - 0.1f) * forest_scale);
    }

    const int desert_offset = 8008135;
    const float desert_scale = 0.005f;
    public static float GetDesertWeight(int global_x, int global_y) {
        return Perlin.Noise((global_x + desert_offset - 0.1f) * desert_scale, 
                            (global_y + desert_offset - 0.1f) * desert_scale);
    }
}