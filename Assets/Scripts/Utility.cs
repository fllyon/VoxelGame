using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

static class Utility {

    public static Vector3 Vector3(this int3 input) {
        return new Vector3(input.x, input.y, input.z);
    }

    [BurstCompile]
    public static int Cubed(this int input) {
        return input * input * input;
    }

    [BurstCompile]
    public static int Flatten(this int3 input) {
        return input.x * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE + input.z * ChunkData.CHUNK_SIZE + input.y;
    }

}