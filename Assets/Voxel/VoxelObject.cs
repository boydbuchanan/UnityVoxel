using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class VoxelObject : MonoBehaviour
{
    
    public Material VoxelMaterial;
    [SerializeField] protected bool UseColorFromAsset = true;
    
    [SerializeField] protected MeshFilter meshFilter;
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected MeshCollider meshCollider;
    
    [SerializeField] protected bool OnlyLoadActive;
    [SerializeField] protected int NumActive;
    [SerializeField] protected int TotalVoxels;

    private int _numActive;

    protected virtual void OnValidate()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();
        // set scale to world scale
        if(VoxelWorld.Instance != null)
            transform.localScale = VoxelWorld.Instance.VoxelScale;

        if(_numActive != NumActive){
            
            _numActive = NumActive;
            GenerateMesh();
        }
        if(NumActive > TotalVoxels){
            NumActive = TotalVoxels;
            _numActive = NumActive;
        }
        if(VoxelMaterial != null && (meshRenderer.sharedMaterial == null || meshRenderer.sharedMaterial != VoxelMaterial)){
            SetMaterial();
        }
    }

    /// Start Voxel Methods

    protected Dictionary<int3, Voxel> voxels;
    public HashSet<int3> activeVoxelIndexes;
    public bool IsLoaded => voxels != null;

    public virtual float3 VoxelCenter(int3 voxelPosition)
    {
        float3 centerPosition = new float3(
            voxelPosition.x + 0.5f,
            voxelPosition.y + 0.5f,
            voxelPosition.z + 0.5f
        );

        return transform.TransformPoint(centerPosition);
    }

    /// End Voxel Methods
    /// Start Mesh Methods
    
    [ContextMenu("Generate Mesh")]
    public void GenerateMesh(){
        if(!IsLoaded){
            return;
        }
        var mesh =  voxels.CreateMeshJob(OnlyLoadActive ? NumActive : -1, UseColorFromAsset);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    
    public virtual void SetMaterial(){
        var newMat = new Material(VoxelMaterial);
        SetMaterial(newMat);
    }
    public virtual void SetMaterial(Material newMat){
        meshRenderer.sharedMaterial = newMat;
    }
    public virtual Color GetVoxelColor(Voxel vox){
        return new Color(vox.ColorIndex, vox.ColorIndex, vox.ColorIndex);
    }

#region Debug

    [SerializeField] protected DebugProps debugProps;
    [Serializable]
    public class DebugProps
    {
        public bool showVoxels => showColorIndex || showCubes;
        public bool showPosition = false;
        public bool showColorIndex = false;
        public bool showCubes = false;
        public bool showActiveCubes = false;
        public float ActiveAlpha = 0.4f;
        public float InactiveAlpha = 0.1f;
        public float DistanceCutoff = 0.1f;

        public bool forceRaycastDebug = false;
    }
    
    protected bool[] FacesVisible(int3 key){
        bool[] facesVisible = new bool[6]{
            FaceVisible(key, Voxel.up),
            FaceVisible(key, Voxel.down),
            FaceVisible(key, Voxel.left),
            FaceVisible(key, Voxel.right),
            FaceVisible(key, Voxel.forward),
            FaceVisible(key, Voxel.back)
        };

        return facesVisible;
    }
    
    public bool FaceVisible(int3 position, int3 direction){
        // If there is no active voxel next to face, it is visible
        return !IsActiveVoxel(position + direction); // Top
    }
    bool IsActiveVoxel(int3 pos){
        return voxels.ContainsKey(pos) && voxels[pos].IsActive;
    }

    protected virtual void OnDrawGizmos()
    {
        if(!DrawGizmo.IsEditorFocus(transform)){
            return;
        }
        if(!IsLoaded){
            return;
        }
        
        if(debugProps.forceRaycastDebug || TotalVoxels > 1000){
            RaycastVoxels(SceneView.currentDrawingSceneView.camera.transform.position, SceneView.currentDrawingSceneView.camera.transform.forward, debugProps.DistanceCutoff, 0.1f);
            return;
        }

        if(debugProps.showVoxels){

            Gizmos.color = new Color(1, 0, 0, 0.2f);

            foreach (var key in voxels.Keys)
            {
                Voxel vox = voxels[key];
                
                var facesVisible = FacesVisible(key);
                // if none visible, skip
                if (!facesVisible.Any(f => f))
                    continue;
                var cubeCenter = transform.TransformPoint(new Vector3(vox.Position.x + 0.5f, vox.Position.y + 0.5f, vox.Position.z + 0.5f));
                DrawGizmo.CubeOutline(cubeCenter, Vector3.one.x, Color.black);

                if(debugProps.showActiveCubes && !activeVoxelIndexes.Contains(key)){
                    continue;
                }
                RenderVoxel(vox);
            }
        }
    }
    public virtual void RenderVoxel(Voxel vox){
        var cubeCenter = transform.TransformPoint(new Vector3(vox.Position.x + 0.5f, vox.Position.y + 0.5f, vox.Position.z + 0.5f));
        var voxColor = GetVoxelColor(vox);
        float colorAlpha = vox.IsActive && activeVoxelIndexes.Contains(vox.Position) ? debugProps.ActiveAlpha : debugProps.InactiveAlpha;
        DrawGizmo.CubeOutline(cubeCenter, Vector3.one.x, Color.black);
        if(debugProps.showCubes){
            Gizmos.color = new Color(voxColor.r, voxColor.g, voxColor.b, colorAlpha);
            Gizmos.DrawCube(cubeCenter, Vector3.one);
        }
        if(debugProps.showPosition){
            Gizmos.color = new Color(voxColor.r, voxColor.g, voxColor.b, colorAlpha);
            var posText = vox.Position.ToString();
            DrawGizmo.FloatingText(cubeCenter, posText, Color.black);
        }
        
        else if(debugProps.showColorIndex){
            Gizmos.color = new Color(voxColor.r, voxColor.g, voxColor.b, colorAlpha);
            var colIndx = vox.ColorIndex.ToString();
            DrawGizmo.FloatingText(cubeCenter, colIndx, Color.black);
            if(string.IsNullOrEmpty(colIndx)){
                DrawGizmo.FloatingText(cubeCenter, "Empty", Color.black);
                Debug.Log($"Empty Color Index {colIndx} @ {vox.Position}");
            }
        }
    }
    public void RaycastVoxels(Vector3 cameraPosition, Vector3 cameraForward, float maxDistance, float stepSize)
    {
        // Direction of the ray (normalized camera forward)
        Vector3 direction = cameraForward.normalized;
        var endPosition = cameraPosition + direction * maxDistance;
        DrawGizmo.Sphere(endPosition, 0.1f, Color.red);

        // Continue stepping until the maximum distance is reached
        for (float t = 0; t < maxDistance; t += stepSize)
        {
            // Calculate the current position along the ray
            Vector3 rayPosition = cameraPosition + direction * t;

            // Convert the world position to voxel grid coordinates (assuming integer voxel positions)
            //int3 voxelPosition = new int3(Mathf.FloorToInt(rayPosition.x), Mathf.FloorToInt(rayPosition.y), Mathf.FloorToInt(rayPosition.z));
            var voxelPosition = transform.InverseTransformPoint(rayPosition).ToInt3();

            if(voxels.ContainsKey(voxelPosition)){
                var voxel = voxels[voxelPosition];
                RenderVoxel(voxel);
            }

            // Stop if the ray has traversed all relevant voxels in the direction
            if (t >= maxDistance)
                break;
        }
    }
#endregion

}
