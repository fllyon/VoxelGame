using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public struct VertexData
{

    public static NativeArray<Vertex> GetVertices()
    {
        NativeArray<Vertex> array = new NativeArray<Vertex>(24, Allocator.Persistent);

        // Up
        array[0] = new Vertex(new float3(0, 1, 0), new float3(0, 1, 0), new float2(0, 0));
        array[1] = new Vertex(new float3(0, 1, 1), new float3(0, 1, 0), new float2(0.0625f, 0));
        array[2] = new Vertex(new float3(1, 1, 1), new float3(0, 1, 0), new float2(0.0625f, 0.0625f));
        array[3] = new Vertex(new float3(1, 1, 0), new float3(0, 1, 0), new float2(0, 0.0625f));

        // Front
        array[4] = new Vertex(new float3(0, 0, 1), new float3(0, 0, 1), new float2(0, 0));
        array[5] = new Vertex(new float3(1, 0, 1), new float3(0, 0, 1), new float2(0.0625f, 0));
        array[6] = new Vertex(new float3(1, 1, 1), new float3(0, 0, 1), new float2(0.0625f, 0.0625f));
        array[7] = new Vertex(new float3(0, 1, 1), new float3(0, 0, 1), new float2(0, 0.0625f));

        // Right
        array[8] = new Vertex(new float3(1, 0, 1), new float3(1, 0, 0), new float2(0, 0));
        array[9] = new Vertex(new float3(1, 0, 0), new float3(1, 0, 0), new float2(0.0625f, 0));
        array[10] = new Vertex(new float3(1, 1, 0), new float3(1, 0, 0), new float2(0.0625f, 0.0625f));
        array[11] = new Vertex(new float3(1, 1, 1), new float3(1, 0, 0), new float2(0, 0.0625f));

        // Back
        array[12] = new Vertex(new float3(1, 0, 0), new float3(0, 0, -1), new float2(0, 0));
        array[13] = new Vertex(new float3(0, 0, 0), new float3(0, 0, -1), new float2(0.0625f, 0));
        array[14] = new Vertex(new float3(0, 1, 0), new float3(0, 0, -1), new float2(0.0625f, 0.0625f));
        array[15] = new Vertex(new float3(1, 1, 0), new float3(0, 0, -1), new float2(0, 0.0625f));

        // Left
        array[16] = new Vertex(new float3(0, 0, 0), new float3(-1, 0, 0), new float2(0, 0));
        array[17] = new Vertex(new float3(0, 0, 1), new float3(-1, 0, 0), new float2(0.0625f, 0));
        array[18] = new Vertex(new float3(0, 1, 1), new float3(-1, 0, 0), new float2(0.0625f, 0.0625f));
        array[19] = new Vertex(new float3(0, 1, 0), new float3(-1, 0, 0), new float2(0, 0.0625f));

        // Down
        array[20] = new Vertex(new float3(0, 0, 0), new float3(0, -1, 0), new float2(0, 0));
        array[21] = new Vertex(new float3(1, 0, 0), new float3(0, -1, 0), new float2(0.0625f, 0));
        array[22] = new Vertex(new float3(1, 0, 1), new float3(0, -1, 0), new float2(0.0625f, 0.0625f));
        array[23] = new Vertex(new float3(0, 0, 1), new float3(0, -1, 0), new float2(0, 0.0625f));

        return array;
    }

    public static NativeArray<int3> GetDirections()
    {
        NativeArray<int3> directions = new NativeArray<int3>(6, Allocator.Persistent);

        directions[0] = new int3(0, 0, 1);
        directions[1] = new int3(0, 1, 0);
        directions[2] = new int3(1, 0, 0);
        directions[3] = new int3(0, -1, 0);
        directions[4] = new int3(-1, 0, 0);
        directions[5] = new int3(0, 0, -1);

        return directions;
    }

    public static NativeArray<int> GetTextureMap()
    { 
        NativeArray<int> texture_map = new NativeArray<int>(12, Allocator.Persistent);

        // Grass
        texture_map[0] = 1;
        texture_map[1] = 2;
        texture_map[2] = 2;
        texture_map[3] = 2;
        texture_map[4] = 2;
        texture_map[5] = 3;

        // Dirt
        texture_map[6] = 3;
        texture_map[7] = 3;
        texture_map[8] = 3;
        texture_map[9] = 3;
        texture_map[10] = 3;
        texture_map[11] = 3;

        return texture_map;
    }
    
}



[BurstCompile]
public struct Vertex
{

    public Vertex(float3 _position, float3 _normal, float2 _uv)
    {
        position = _position;
        normal = _normal;
        uv = _uv;
    }

    public float3 position;
    public float3 normal;
    public float2 uv;

}