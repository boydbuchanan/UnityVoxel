using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class VoxelAsset : VoxelObject
{
    public VoxelData VoxelAssetReference;
    protected override void OnValidate()
    {
        base.OnValidate();
        if (VoxelAssetReference == null)
        {
            Debug.LogWarning("VoxelAssetReference is null. Please assign a VoxelAssetReference.");
            return;
        }
        LoadFromAsset(true);
    }

    /// Start Voxel Methods

    [ContextMenu("Load Asset And Mesh")]
    public void LoadMeshFromAsset(){
        LoadFromAsset(true);
    }
    public void LoadFromAsset(bool generateMesh){
        voxels = VoxelAssetReference.map.v.ToDictionary(v => v.Position, v => v);
        activeVoxelIndexes = voxels.Keys.OrderBy(x => x.y).ThenBy(x => x.x + x.z).Take(NumActive).ToHashSet();
        TotalVoxels = voxels.Count;
        if(generateMesh){
            GenerateMesh();
            SetMaterial();
        }
    }

    /// End Voxel Methods
    /// Start Mesh Methods
    public override void SetMaterial(Material newMat)
    {
        if(UseColorFromAsset && VoxelAssetReference.palette != null)
            newMat.mainTexture = VoxelAssetReference.palette.GetTexture();
        base.SetMaterial(newMat);
    }

    public override Color GetVoxelColor(Voxel vox)
    {
        return VoxelAssetReference.palette.GetColor(vox.ColorIndex);
    }

}
