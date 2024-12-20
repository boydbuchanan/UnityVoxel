using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class VoxelBuildSystem : VoxelObject
{
    public Transform targetingTransform;  // The player's camera
    public LayerMask voxelLayer;       // The layer where voxels exist
    public float maxDistance = 10f;   // Maximum raycast distance
    public Material highlightMaterial; // Material for highlighting

    public GameObject highlightObject;       // Object used to show the highlight
    private int3? highlightedPosition;  // Currently highlighted voxel position
    private int3? hitPosition;  // Currently hit voxel position

    protected override void OnValidate()
    {
        base.OnValidate();
        if(targetingTransform == null)
            Debug.LogWarning("Targeting Transform is null. Please assign a transform.");
    }
    protected virtual void Awake()
    {
        if(highlightObject == null){
            CreateHighlightObject();
        }
        
        highlightObject.SetActive(true);
        if(VoxelWorld.Instance != null)
            highlightObject.transform.localScale = VoxelWorld.Instance.VoxelScale;

        voxels = new Dictionary<int3, Voxel>();
    }
    [ContextMenu("Create Highlight Object")]
    void CreateHighlightObject()
    {
        if(highlightObject != null){
            // if application is playing, destroy the object
            if(Application.isPlaying)
                Destroy(highlightObject);
            else
                DestroyImmediate(highlightObject);
        }
        highlightObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightObject.name = "Voxel Highlighter";
        highlightObject.GetComponent<Collider>().enabled = false;
        highlightObject.GetComponent<Renderer>().material = highlightMaterial;
        highlightObject.SetActive(false);
        highlightObject.transform.localScale = transform.localScale;
    }
    [ContextMenu("Add Hit to Voxel")]
    void AddHitToVoxel()
    {
        if(voxels == null)
            voxels = new Dictionary<int3, Voxel>();

        if (highlightedPosition.HasValue)
        {
            // Place the voxel at the highlighted position
            int3 voxelPosition = highlightedPosition.Value;

            if (!voxels.ContainsKey(voxelPosition))
            {
                // Add voxel to the active dictionary
                voxels[voxelPosition] = new Voxel(voxelPosition, 0); // You can adjust the color if needed
                GenerateMesh(); // Update the mesh to reflect changes
            }
        }
    }

    private void Update()
    {
        HandleVoxelHighlight();
    }

    public float hitAdjustment = 0.5f;
    public Vector3 hitPoint;
    public Vector3 offSet;
    public Vector3 hitNormal;
    private void HandleVoxelHighlight()
    {
        if (!targetingTransform)
            return;
        // Perform a raycast
        if (Physics.Raycast(targetingTransform.position, targetingTransform.forward, out RaycastHit hit, maxDistance, voxelLayer))
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
            // Calculate the voxel position nearest to the hit point
            var negatedNormal = -hitNormal;
            var hitLocalPoint = transform.InverseTransformPoint(hitPoint + (negatedNormal * hitAdjustment));
            hitPosition = hitLocalPoint.ToInt3(true);
            if(!voxels.ContainsKey(hitPosition.Value)){
                hitPosition = null;
            }
            
            var highlightLocalPoint = transform.InverseTransformPoint(hitPoint + (hitNormal/2));
            //localPoint += hitNormal;
            var highlightVoxPosition = highlightLocalPoint.ToInt3(true);

            highlightObject.transform.position = VoxelCenter(highlightVoxPosition);
            highlightObject.SetActive(true);
            highlightedPosition = highlightVoxPosition;
        }
        else
        {
            // No voxel is being hit, disable the highlight
            highlightObject.SetActive(false);
            highlightedPosition = null;
            hitPosition = null;
        }
    }

    public void RemoveVoxel(){
        if(hitPosition.HasValue && voxels.ContainsKey(hitPosition.Value)){
            voxels.Remove(hitPosition.Value);
            GenerateMesh();
        }
    }
    public void AddVoxel()
    {
        if (highlightedPosition.HasValue)
        {
            // Place the voxel at the highlighted position
            int3 voxelPosition = highlightedPosition.Value;

            if (!voxels.ContainsKey(voxelPosition))
            {
                // Add voxel to the active dictionary
                voxels[voxelPosition] = new Voxel(voxelPosition, 0); // You can adjust the color if needed
                GenerateMesh(); // Update the mesh to reflect changes
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if(!targetingTransform || !highlightedPosition.HasValue)
            return;
        DrawGizmo.Line(targetingTransform.position, hitPoint, Color.red);
        DrawGizmo.Line(hitPoint, hitPoint + hitNormal.Times(transform.localScale), Color.magenta);
        if(hitPosition.HasValue)
            DrawGizmo.CubeOutline(VoxelCenter(hitPosition.Value), 0.1f, Color.red);

        var localPoint = transform.InverseTransformPoint(hitPoint + (hitNormal/2));
        //localPoint += hitNormal;
        var voxPosition = localPoint.ToInt3(true);
        
        var coords = transform.TransformPoint(voxPosition.ToFloat3());
        DrawGizmo.Sphere(coords, 0.1f, Color.magenta);

        var newPosition = VoxelCenter(voxPosition);

        var scale = VoxelWorld.Instance != null ? VoxelWorld.Instance.Scale : transform.localScale.x;
        
        DrawGizmo.CubeOutline(newPosition, scale, Color.black);
        DrawGizmo.FloatingText(newPosition, $"{voxPosition.x}, {voxPosition.y}, {voxPosition.z}", Color.black);
    }
}
