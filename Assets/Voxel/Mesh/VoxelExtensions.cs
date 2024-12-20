using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class VoxelExtensions
{
    public static Mesh CreateMeshJob(this Dictionary<int3, Voxel> source, int numActive = -1, bool useColorFromAsset = true)
    {
        int takeAmount = numActive < 0 ? source.Count : numActive;
        HashSet<int3> activeKeys = source.Values
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.Position.y)
                        .ThenBy(x => x.Position.x + x.Position.z)
                        .Take(takeAmount)
                        .Select(x => x.Position).ToHashSet();

        var vertices = new NativeList<float3>(Allocator.TempJob);
        var normals = new NativeList<float3>(Allocator.TempJob);
        var triangles = new NativeList<int>(Allocator.TempJob);
        var uv = new NativeList<float2>(Allocator.TempJob);

        var voxels = new NativeHashMap<int3, Voxel>(source.Count, Allocator.TempJob);
        foreach (var voxel in source.Values) {
            voxels.Add(voxel.Position, voxel);
        }
        var keys = voxels.GetKeyArray(Allocator.TempJob);

        var activeVoxels = new NativeHashSet<int3>(activeKeys.Count, Allocator.TempJob);
        foreach (var key in activeKeys){
            activeVoxels.Add(key);
        }

        vertices.Capacity = source.Count * 4;
        normals.Capacity = source.Count * 4;
        triangles.Capacity = source.Count * 6;
        uv.Capacity = source.Count * 4;
        var mesh = new Mesh();
        try
        {

            var job = new UpdateMeshJob
            {
                keys = keys,
                voxels = voxels,
                activeVoxels = activeVoxels,
                vertices = vertices,
                normals = normals,
                triangles = triangles,
                uv = uv,
                useColorFromAsset = useColorFromAsset
            };

            // job.Schedule(keys.Length, 64).Complete();
            job.Schedule().Complete();
            
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 0),
            };
            var verts = vertices.AsArray().ToVector3();
            mesh.indexFormat =  verts.Length > UInt16.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = verts.ToArray();

            mesh.triangles = triangles.AsArray().ToArray();
            mesh.uv = uv.AsArray().ToVector2().ToArray();
            mesh.normals = normals.AsArray().ToVector3().ToArray();

            mesh.SetVertexBufferParams(verts.Length, layout);
        }
        finally
        {
            vertices.Dispose();
            normals.Dispose();
            triangles.Dispose();
            uv.Dispose();
            voxels.Dispose();
            keys.Dispose();
            activeVoxels.Dispose();
        }

        return mesh;

    }

    public static Mesh CreateMesh(this Dictionary<int3, Voxel> source, int numActive, bool useColorFromAsset = true){
        
        UpdateMesh updateMesh = new UpdateMesh
        {
            vertices = new List<float3>(),
            normals = new List<float3>(),
            triangles = new List<int>(),
            subMeshTriangles = new List<int>[4],
            uv = new List<float2>(),
            numActive = numActive,
            voxels = source,
            useColorFromAsset = useColorFromAsset
        };
        
        updateMesh.Execute();

        var mesh = new Mesh();
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 0),
        };
        
        var vertexCount = updateMesh.vertices.Count;
        mesh.indexFormat =  vertexCount > UInt16.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;

        mesh.vertices = updateMesh.vertices.ToVector3().ToArray();
        //mesh.triangles = updateMesh.triangles.ToArray();
        mesh.uv = updateMesh.uv.ToVector2().ToArray();
        mesh.normals = updateMesh.normals.ToVector3().ToArray();

        byte subMeshes = 0;
        for (int j = 0; j < 4; j++)
        {
            if (updateMesh.subMeshTriangles[j] != null)
            {
                mesh.SetTriangles(updateMesh.subMeshTriangles[j].ToArray(), subMeshes);
                subMeshes++;
            }
        }

        mesh.subMeshCount = subMeshes;
        mesh.SetVertexBufferParams(vertexCount, layout);


        updateMesh.vertices.Clear();
        updateMesh.normals.Clear();
        updateMesh.triangles.Clear();
        updateMesh.uv.Clear();

        return mesh;
    }
}