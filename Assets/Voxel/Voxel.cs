using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public struct Voxel
{
    public byte ColorIndex;
    public bool IsActive;
    public int3 Position;
    public int MeshIndex;

    public Voxel(byte x, byte y, byte z, byte c, int meshIndex = 1, bool active = true){
        
        ColorIndex = c;
        IsActive = active;
        MeshIndex = meshIndex;

        Position = new int3(x, y, z);
    }
    public Voxel(int3 p, byte c, int meshIndex = 1, bool active = true){
        
        ColorIndex = c;
        IsActive = active;
        MeshIndex = meshIndex;

        Position = p;
    }
    public Voxel Copy(bool active = true){
        return new Voxel(Position, ColorIndex, MeshIndex, active);
    }

    public readonly static int3 up = new int3(0, 1, 0);
    public readonly static int3 down = new int3(0, -1, 0);
    public readonly static int3 left = new int3(-1, 0, 0);
    public readonly static int3 right = new int3(1, 0, 0);
    public readonly static int3 forward = new int3(0, 0, 1);
    public readonly static int3 back = new int3(0, 0, -1);
    public readonly static int3 zero = new int3(0, 0, 0);
    public readonly static int3 one = new int3(1, 1, 1);
    public readonly static int3 neg = new int3(-1, -1, -1);

    // Burst Compatible Methods
    // They can not use managed type arrays like Vector3[] or int[]

    public static int GetFaceIndexFromDirection(int3 direction){
        if(math.all(direction == up)){
            return 0;
        }else if(math.all(direction == down)){
            return 1;
        }else if(math.all(direction == left)){
            return 2;
        }else if(math.all(direction == right)){
            return 3;
        }else if(math.all(direction == forward)){
            return 4;
        }else if(math.all(direction == back)){
            return 5;
        }
        return -1;
    }

    public static float3 GetFaceVertex(int3 Position, int faceIndex, int vertIndex){
        return GetFaceVertex(Position.x, Position.y, Position.z, faceIndex, vertIndex);
    }
    public static float3 GetFaceVertex(int x, int y, int z, int faceIndex, int vertIndex)
    {
        if (faceIndex == 0) // Top Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x,     y + 1, z);
                case 1:
                    return new float3(x,     y + 1, z + 1);
                case 2:
                    return new float3(x + 1, y + 1, z + 1);
                case 3:
                    return new float3(x + 1, y + 1, z);
            }
        }
        else if (faceIndex == 1) // Bottom Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x,     y, z + 1);
                case 1:
                    return new float3(x,     y, z);
                case 2:
                    return new float3(x + 1, y, z);
                case 3:
                    return new float3(x + 1, y, z + 1);
            }
        }
        else if (faceIndex == 2) // Left Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x, y,     z + 1);
                case 1:
                    return new float3(x, y + 1, z + 1);
                case 2:
                    return new float3(x, y + 1, z);
                case 3:
                    return new float3(x, y,     z);
            }
        }
        else if (faceIndex == 3) // Right Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x + 1, y,     z);
                case 1:
                    return new float3(x + 1, y + 1, z);
                case 2:
                    return new float3(x + 1, y + 1, z + 1);
                case 3:
                    return new float3(x + 1, y,     z + 1);
            }
        }
        else if (faceIndex == 4) // Front Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x,     y, z + 1);
                case 1:
                    return new float3(x + 1, y, z + 1);
                case 2:
                    return new float3(x + 1, y + 1, z + 1);
                case 3:
                    return new float3(x,     y + 1, z + 1);
            }
        }
        else if (faceIndex == 5) // Back Face
        {
            switch(vertIndex){
                case 0:
                    return new float3(x + 1, y, z);
                case 1:
                    return new float3(x,     y, z);
                case 2:
                    return new float3(x,     y + 1, z);
                case 3:
                    return new float3(x + 1, y + 1, z);
            }
        }
        return float3.zero;
    }
    public static float2 GetMaterialUv(int3 pos, int3 direction, int vertIndex){
        int faceIndex = GetFaceIndexFromDirection(direction);
        return GetMaterialUv(pos.x, pos.y, pos.z, faceIndex, vertIndex);
    }
    
    public static float2 GetMaterialUv(int x, int y, int z, int faceIndex, int vertIndex)
    {
        switch (vertIndex)
        {
            case 0:
                return new Vector2(1, 0); // Flipped horizontally
            case 1:
                return new Vector2(0, 0); // Flipped horizontally
            case 2:
                return new Vector2(1, 1); // Flipped horizontally
            case 3:
                return new Vector2(0, 1); // Flipped horizontally
        }

        return float2.zero;
    }



    public static float2 GetColorUv(byte colorIndex){
        return new float2((colorIndex + 0.5f) / 256, 0.5f);
    }

}

[BurstCompile]
public static class VoxelMesh
{
    public readonly static int[] Triangles = new int[6] { 
        0, 1, 2, // First triangle (clockwise)
        2, 1, 3  // Second triangle (clockwise)
    };
    
    public static int GetSubMeshTriangle(int verticesCount, int index) {
        return Triangles[index] + verticesCount;
    }
    public static int[] GetSubMeshTriangles(int verticesCount) {
        int[] result = new int[Triangles.Length];
        for (int i = 0; i < Triangles.Length; i++)
        {
            result[i] = GetSubMeshTriangle(verticesCount, i);
        }
        return result;
    }

    
    private static float3[] GetVertexFrom(int3 position, float3[] verts, float3 pivot = default){
        float3[] vertices = new float3[4];
        for (int i = 0; i < 4; i++)
        {
            vertices[i] = (verts[i] + position) - pivot;
        }
        return vertices;
    }

    public static float3[] GetFaceVertex(int3 position, int3 direction, float3 pivot = default) {
        return GetVertexFrom(position, Vertices.From(direction), pivot);
    }
    public static float3 GetFaceVertex(int3 Position, int3 direction, int vertIndex, float3 pivot = default){
        return Vertices.From(direction, vertIndex) + Position - pivot;
    }

    public static class Vertices{
        
        public static float3[] From(int3 direction){
            float3[] vertices = new float3[4];
            vertices[0] = From(direction, 0);
            vertices[1] = From(direction, 1);
            vertices[2] = From(direction, 2);
            vertices[3] = From(direction, 3);
            return vertices;
        }

        public static float3 From(int3 direction, int index){
            if(math.all(direction == Voxel.down)){
                if(index == 0){
                    return new float3(0, 0, 0); // Bottom-left
                }else if(index == 1){
                    return new float3(1, 0, 0); // Bottom-right
                }else if(index == 2){
                    return new float3(0, 0, 1); // Top-left (in bottom face)
                }else if(index == 3){
                    return new float3(1, 0, 1); // Top-right (in bottom face)
                }
            }else if(math.all(direction == Voxel.up)){
                if(index == 0){
                    return new float3(0, 1, 1); // Bottom-left (in top face)
                }else if(index == 1){
                    return new float3(1, 1, 1); // Bottom-right (in top face)
                }else if(index == 2){
                    return new float3(0, 1, 0); // Top-left
                }else if(index == 3){
                    return new float3(1, 1, 0); // Top-right
                }
            }else if(math.all(direction == Voxel.forward)){
                if(index == 0){
                    return new float3(0, 0, 1); // Bottom-left
                }else if(index == 1){
                    return new float3(1, 0, 1); // Bottom-right
                }else if(index == 2){
                    return new float3(0, 1, 1); // Top-left
                }else if(index == 3){
                    return new float3(1, 1, 1); // Top-right
                }
            }else if(math.all(direction == Voxel.back)){
                if(index == 0){
                    return new float3(1, 0, 0); // Bottom-left
                }else if(index == 1){
                    return new float3(0, 0, 0); // Bottom-right
                }else if(index == 2){
                    return new float3(1, 1, 0); // Top-left
                }else if(index == 3){
                    return new float3(0, 1, 0); // Top-right
                }
            }else if(math.all(direction == Voxel.left)){
                if(index == 0){
                    return new float3(0, 0, 0); // Bottom-left
                }else if(index == 1){
                    return new float3(0, 0, 1); // Bottom-right
                }else if(index == 2){
                    return new float3(0, 1, 0); // Top-left
                }else if(index == 3){
                    return new float3(0, 1, 1); // Top-right
                }
            }else if(math.all(direction == Voxel.right)){
                if(index == 0){
                    return new float3(1, 0, 1); // Bottom-left
                }else if(index == 1){
                    return new float3(1, 0, 0); // Bottom-right
                }else if(index == 2){
                    return new float3(1, 1, 1); // Top-left
                }else if(index == 3){
                    return new float3(1, 1, 0); // Top-right
                }
            }
            return float3.zero;
        }
    }
}
