using System;
using UnityEngine;

[Serializable]
public class VoxModel : MonoBehaviour
{
    public Vector3Int size;
    public byte[,,] grid;
}
