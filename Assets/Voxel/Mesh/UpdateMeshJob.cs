using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct UpdateMeshJob : IJob
{
    [ReadOnly] public NativeHashMap<int3, Voxel> voxels;
    [ReadOnly] public NativeHashSet<int3> activeVoxels;
    [ReadOnly] public NativeArray<int3> keys;
    [ReadOnly] public int numActive;
    [ReadOnly] public bool useColorFromAsset;

    [NativeDisableParallelForRestriction] public NativeList<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeList<float3> normals;
    [NativeDisableParallelForRestriction] public NativeList<int> triangles;
    [NativeDisableParallelForRestriction] public NativeList<float2> uv;

    public void Execute()
    {
        foreach (var key in activeVoxels)
        {
            var voxel = voxels[key];
            AddData(voxel, Voxel.up);
            AddData(voxel, Voxel.down);
            AddData(voxel, Voxel.left);
            AddData(voxel, Voxel.right);
            AddData(voxel, Voxel.forward);
            AddData(voxel, Voxel.back);
        }
    }

    public void AddData(Voxel voxel, int3 direction){
        int3 position = voxel.Position;
        bool faceVisible = FaceVisible(position, direction);
        
        if (faceVisible){
            var d = vertices.Length;
            
            for (int j = 0; j < 4; j++){
                var vertex = VoxelMesh.GetFaceVertex(position, direction, j);
                vertices.Add(vertex);
                normals.Add(direction);
                if (useColorFromAsset)
                    uv.Add(Voxel.GetColorUv(voxel.ColorIndex));
                else
                    uv.Add(Voxel.GetMaterialUv(position, direction, j));
            }
            for (int t = 0; t < VoxelMesh.Triangles.Length; t++){
                triangles.Add(VoxelMesh.GetSubMeshTriangle(d, t));
            }
        }
    }
    
    public bool FaceVisible(int3 position, int3 direction){
        // If there is no active voxel next to face, it is visible
        var neighborKey = position + direction;
        return !activeVoxels.Contains(neighborKey);
    }
}


public struct UpdateMesh
{
    public Dictionary<int3, Voxel> voxels;
    public HashSet<int3> activeVoxels;
    public int3 DebugPosition;

    public List<float3> vertices;
    public List<float3> normals;
    public List<int> triangles;
    public List<int>[] subMeshTriangles;
    public List<float2> uv;
    public int numActive;
    public bool useColorFromAsset;

    public void Execute()
    {
        var takeAmount = numActive <= 0 ? voxels.Count : numActive;
        activeVoxels = voxels.Values
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.Position.y)
                        .ThenBy(x => x.Position.x + x.Position.z)
                        .Take(takeAmount)
                        .Select(x => x.Position).ToHashSet();

        foreach (var key in activeVoxels)
        {
            var voxel = voxels[key];
            AddData(voxel, Voxel.up);
            AddData(voxel, Voxel.down);
            AddData(voxel, Voxel.left);
            AddData(voxel, Voxel.right);
            AddData(voxel, Voxel.forward);
            AddData(voxel, Voxel.back);
        }
    }

    public void AddData(Voxel voxel, int3 direction){
        int3 position = voxel.Position;
        bool faceVisible = FaceVisible(position, direction);
        
        if (faceVisible){
            var d = vertices.Count;
            
            for (int j = 0; j < 4; j++){
                var vertex = VoxelMesh.GetFaceVertex(position, direction, j);
                vertices.Add(vertex);
                normals.Add(direction);
                if (useColorFromAsset)
                    uv.Add(Voxel.GetColorUv(voxel.ColorIndex));
                else
                    uv.Add(Voxel.GetMaterialUv(position, direction, j));
            }
            for (int t = 0; t < VoxelMesh.Triangles.Length; t++){
                //VoxelMesh.GetSubMeshTriangle(d, t);
                subMeshTriangles[voxel.MeshIndex].Add(VoxelMesh.GetSubMeshTriangle(d, t));
                //triangles.Add(VoxelMesh.GetSubMeshTriangle(d, t));
            }
            if(vertices.Count == d){
                Debug.Log("No vertices added");
            }
        }
    }
    
    public bool FaceVisible(int3 position, int3 direction){
        // If there is no active voxel next to face, it is visible
        var neighborKey = position + direction;
        return !activeVoxels.Contains(neighborKey);
    }
}
