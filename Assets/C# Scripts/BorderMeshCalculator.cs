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

        List<float3> tilePositionsInBorder = new List<float3>(tilesInBorderCount);

        for (int i = 0; i < tilesInBorderCount; i++)
        {
            if (GridManager.TryGetTileByPos(tilesInBorder[i].transform.position.ToVector2(), out GameObject tile))
            {
                tilePositionsInBorder.Add(tile.transform.position);
            }
        }


        int tilePositionsInBorderCount = tilePositionsInBorder.Count;

        float3 tilePos;
        float3 neighbourPos;


        for (int i = 0; i < tilePositionsInBorderCount; i++)
        {
            EdgeData edgeData = new EdgeData(4);

            for (int i2 = 0; i2 < tilePositionsInBorderCount; i2++)
            {
                tilePos = tilePositionsInBorder[i];
                neighbourPos = tilePositionsInBorder[i2];

                //check for every tile if that tile is direct neighbour of currentTile, if so increment neighbourCount by 1
                if (Mathf.RoundToInt(tilePos.DistanceFrom(neighbourPos)) == 1)
                {
                    edgeData.Add(tilePos, neighbourPos);
                }
            }

            //if currentTile doesnt have 4 neighbours, ALWAYS meaning it on the edge, save the border positions
            if (edgeData.IsCenterTile)
            {

            }
        }

        return null;
    }


    public static void GenerateThinCuboid(float3 centerPos, int3 direction, out float3[] vertices, out int[] triangles)
    {
        // Define width & depth based on direction
        float3 widthDir = direction.x != 0 ? new float3(1, 0, 0) : new float3(0, 0, 1);
        float3 depthDir = direction.x != 0 ? new float3(0, 1, 0) : new float3(1, 0, 0); // Thin in perpendicular axis

        // Half sizes
        float halfWidth = 0.5f;  // Always 1 unit wide
        float halfDepth = 0.1f;  // Thickness of the plane

        // Compute 8 vertices (front & back face)
        vertices = new float3[]
        {
            // Front face
            centerPos + (-widthDir * halfWidth) + (-depthDir * halfDepth), // 0
            centerPos + ( widthDir * halfWidth) + (-depthDir * halfDepth), // 1
            centerPos + ( widthDir * halfWidth) + ( depthDir * halfDepth), // 2
            centerPos + (-widthDir * halfWidth) + ( depthDir * halfDepth), // 3

            // Back face (offset slightly in depth direction)
            centerPos + (-widthDir * halfWidth) + (-depthDir * halfDepth) + depthDir * 0.0001f, // 4
            centerPos + ( widthDir * halfWidth) + (-depthDir * halfDepth) + depthDir * 0.0001f, // 5
            centerPos + ( widthDir * halfWidth) + ( depthDir * halfDepth) + depthDir * 0.0001f, // 6
            centerPos + (-widthDir * halfWidth) + ( depthDir * halfDepth) + depthDir * 0.0001f  // 7
        };

        // Define two quads (4 triangles) for front & back
        triangles = new int[]
        {
            // Front face
            0, 1, 2,  0, 2, 3,
            // Back face (flipped winding order)
            4, 6, 5,  4, 7, 6
        };
    }
}


public struct EdgeData
{
    public float3[] edgeCenterPositions;
    public int3[] edgeDirections;

    public int neighbourId;

    public bool IsCenterTile => neighbourId != 4;


    public EdgeData(int count)
    {
        edgeCenterPositions = new float3[count];
        edgeDirections = new int3[count];
        neighbourId = 0;
    }


    public void Add(float3 tilePos, float3 neighbourTilePos)
    {
        edgeCenterPositions[neighbourId] = new float3(neighbourTilePos - tilePos);

        float3 direction = neighbourTilePos.DistanceFrom(tilePos);
        edgeDirections[neighbourId] = new int3(Mathf.RoundToInt(direction.x), 0, Mathf.RoundToInt(direction.z));

        neighbourId += 1;
    }
}