using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public struct BorderTileData
{
    public List<float3> edgeCenterPositions;
    public List<int3> edgeDirections;

    public int edgeCount;

    public bool IsBorderTile => edgeCount != 4;


    public BorderTileData(int count)
    {
        edgeCenterPositions = new List<float3>(count);
        edgeDirections = new List<int3>(count);
        edgeCount = 0;
    }


    public void Add(float3 tilePos, float3 missingNeighbourTilePos)
    {
        float3 direction = tilePos - missingNeighbourTilePos;

        edgeCenterPositions.Add(tilePos - direction * 0.5f);

        //swap x and y to make a border edge along the tile, instead of away from it
        edgeDirections.Add(new int3((int)math.round(direction.z), 0, (int)math.round(direction.x)));

        edgeCount += 1;
    }
}