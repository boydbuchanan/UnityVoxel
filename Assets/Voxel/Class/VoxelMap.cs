using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelMap
{
    public Vector3Int size;
    [HideInInspector] public List<Voxel> v;


    public VoxelMap()
    {
        v = new List<Voxel>();
    }

    internal byte GetVoxel(Vector3Int pos)
    {
        for (int i = 0; i < v.Count; i++)
        {
            if (v[i].Position.Equals(pos))
            {
                return v[i].ColorIndex;
            }
        }

        return 0;
    }

    internal byte GetVoxel(int x, int y, int z)
    {
        Vector3Int v3 = new Vector3Int(x, y, z);
        for (int i = 0; i < v.Count; i++)
        {
            if (v[i].Position.Equals(v3))
            {
                return v[i].ColorIndex;
            }
        }

        return 0;
    }

    public byte[,,] ToGrid()
    {
        byte[,,] grid = new byte[size.x, size.y, size.z];
        for (int i = 0; i < v.Count; i++)
        {
            grid[v[i].Position.x, v[i].Position.y, v[i].Position.z] = v[i].ColorIndex;
        }
        return grid;
    }
}