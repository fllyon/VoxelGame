public static class WorldSettings {

    public static readonly int WORLD_SIZE_IN_CHUNKS = 128;
    public static readonly int WORLD_HEIGHT_IN_CHUNKS = 128;

    public static readonly int CHUNK_SIZE = 32;

    public static readonly int RENDER_DISTANCE = 16;
    public static readonly int LOAD_DISTANCE = RENDER_DISTANCE + 1;
    public static readonly int LOAD_CONTAINER_SIZE = (2 * LOAD_DISTANCE + 1).Cubed();
    public static readonly int RENDER_CONTAINER_SIZE = (2 * RENDER_DISTANCE + 1).Cubed();
    
}