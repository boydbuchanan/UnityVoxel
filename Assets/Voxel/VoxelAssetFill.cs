using System.Collections.Generic;

using UnityEngine;
using System.IO;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class VoxelAssetFill : VoxelAsset
{
    public VoxelAsset FullVoxelAsset;
    public bool DisableFullWhenFullyLoaded = true;
    public int LastShownVoxels = -1;

    protected override void OnValidate()
    {
        if(FullVoxelAsset == null || FullVoxelAsset == this){
            // get child not this object
            FullVoxelAsset = GetComponentsInChildren<VoxelAsset>().FirstOrDefault(v => v != this);
        }
        if(FullVoxelAsset != null && FullVoxelAsset.VoxelAssetReference != VoxelAssetReference){
            FullVoxelAsset.VoxelAssetReference = VoxelAssetReference;
            FullVoxelAsset.LoadFromAsset(true);
        }
        
        base.OnValidate();

        if(IsLoaded){
            LastShownVoxels = -1;
            CheckShownVoxels();
        }
        if(!OnlyLoadActive){
            Debug.LogWarning("OnlyLoadActive is not enabled. Use VoxelAsset instead.");
        }
    }
    

    [ContextMenu("Shown Voxels +1")]
    public void IncrementShownVoxels(){
        IncrementShownVoxels(1);
    }
    public void IncrementShownVoxels(int amount){
        SetShownVoxels(NumActive + amount);
    }

    public void SetShownVoxels(int amount){
        NumActive = amount;
        CheckShownVoxels();
    }
    protected virtual void CheckShownVoxels(){
        
        if(!IsLoaded || TotalVoxels <= 0){
            return;
        }
        if(NumActive < 0){
            NumActive = 0;
        }
        if(NumActive > TotalVoxels){
            NumActive = TotalVoxels;
        }
        
        if(LastShownVoxels != NumActive){
            GenerateMesh();
            LastShownVoxels = NumActive;
        }
        if(DisableFullWhenFullyLoaded){
            FullVoxelAsset.gameObject.SetActive(NumActive < TotalVoxels);
        }
    }
}