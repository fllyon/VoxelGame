using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

static class Utility {

    public static Vector3 Vector3(this int3 input) {
        return new Vector3(input.x, input.y, input.z);
    }

    public static int3 Int3(this Vector3 input) {
        return new int3((int)input.x, (int)input.y, (int)input.z);
    }

    [BurstCompile]
    public static int Squared(this int input) {
        return input * input;
    }

    [BurstCompile]
    public static int Cubed(this int input) {
        return input * input * input;
    }

    [BurstCompile]
    public static int Flatten(this int3 input) {
        return (input.x << 10) + (input.z << 5) + input.y;
    }

    [BurstCompile]
    public static int Flatten(this int2 input) {
        return (input.x << 5) + input.y;
    }

    [BurstCompile]
    public static int3 Unflatten(this int input) {
        int y = input & 0x1F;
        int z = (input >> 5) & 0x1F;
        int x = (input >> 10) & 0x1F;
        return new int3(x, y, z);
    }

    [BurstCompile]
    public static int3 GetChunkCoord(int3 coord) {
        return new int3((int)math.floor(coord.x / (float)ChunkData.CHUNK_SIZE),
                        (int)math.floor(coord.y / (float)ChunkData.CHUNK_SIZE),
                        (int)math.floor(coord.z / (float)ChunkData.CHUNK_SIZE));
    }

    public static int3[] dirs = {
        new int3(0, 1, 0),
        new int3(0, 0, 1),
        new int3(1, 0, 0),
        new int3(0, 0, -1),
        new int3(-1, 0, 0),
        new int3(0, -1, 0),
    };

    public static int3[] self_dirs = {
        new int3(0, 1, 0),
        new int3(0, 0, 1),
        new int3(1, 0, 0),
        new int3(0, 0, 0),
        new int3(0, 0, -1),
        new int3(-1, 0, 0),
        new int3(0, -1, 0)
    };

}