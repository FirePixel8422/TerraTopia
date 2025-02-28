using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class BorderMeshCalculator
{
    public static Mesh CreateBorderMesh(List<TileBase> tilesInBorder)
    {
        

        return null;
    }


    public static List<float3> GetTilesOnEdgeOfBorder(List<TileBase> tilesInBorder)
    {
        int tilesInBorderCount = tilesInBorder.Count;

        for (int i = 0; i < tilesInBorderCount; i++)
        {
            //GridManager.TryGetTileByPos(tilesInBorder[i].transform.position)
        }

        return null;
    }
}