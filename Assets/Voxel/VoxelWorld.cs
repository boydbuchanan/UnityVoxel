using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public static VoxelWorld Instance;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject.GetComponent(Instance.GetType()));
        DontDestroyOnLoad(gameObject);
    }
    public float Scale = 1;
    public Vector3 VoxelScale => new Vector3(Scale, Scale, Scale);

}