using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using System.Linq;

public static partial class VoxelExtensions
{
    public static bool Is(this float3 value, float3 other)
    {
        return math.all(value == other);
    }   
    public static bool Is(this int3 value, int3 other)
    {
        return math.all(value == other);
    }   
    public static T[] ToArray<T>(this T value, int length)
    {
        T[] array = new T[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = value;
        }
        return array;
    }
    public static float3[] GetFaceVertices(this Voxel voxel, int faceIndex, int amount){
        float3[] vertices = new float3[amount];
        for (int i = 0; i < amount; i++){
            vertices[i] = Voxel.GetFaceVertex(voxel.Position, faceIndex, i);
        }
        return vertices;
    }
    
    public static Vector3[] ToVector3(this int3[] values){
        return values.Select(v => new Vector3(v.x, v.y, v.z) ).ToArray();
    }
    public static Vector3[] ToVector3(this float3[] values)
    {
        return values.Select(value => (Vector3)value).ToArray();
    }
    
    public static Vector3[] ToVector3(this IEnumerable<float3> values)
    {
        return values.Select(value => (Vector3)value).ToArray();
    }

    public static Vector2[] ToVector2(this IEnumerable<float2> values)
    {
        return values.Select(value => (Vector2)value).ToArray();
    }
    public static float3 ToFloat3(this int3 vector){
        return new float3(vector.x, vector.y, vector.z);
    }
    public static int3 ToInt3(this Vector3 vector, bool roundDown = false)
    {
        if(roundDown){
            return new int3(
                Mathf.FloorToInt(vector.x),
                Mathf.FloorToInt(vector.y),
                Mathf.FloorToInt(vector.z)
            );
        }

        // round to nearest int
        return new int3(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y),
            Mathf.RoundToInt(vector.z)
        );
    }
    public static Vector3 Times(this Vector3 vector, Vector3 other)
    {
        return new Vector3(vector.x * other.x, vector.y * other.y, vector.z * other.z);
    }
}
