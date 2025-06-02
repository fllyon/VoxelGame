public static class WorldSettings {

    public static readonly int WORLD_SIZE_IN_CHUNKS = 16;
    public static readonly int WORLD_HEIGHT_IN_CHUNKS = 16;

    public static readonly int CHUNK_SIZE = 32;

    public static readonly int LOAD_DISTANCE = 5;
    public static readonly int LOAD_CONTAINER_SIZE = 1331; // (LOAD_DISTANCE * 2 + 1) ^ 2

    public static readonly int RENDER_DISTANCE = 4;
    public static readonly int RENDER_CONTAINER_SIZE = 729; // (RENDER_DISTANCE * 2 + 1) ^ 2

    
}