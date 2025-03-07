using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;


[BurstCompile]
public static class BorderMeshCalculator
{
    public static void CreateBorderMesh(Mesh mesh, List<float3> tilePositionsInBorder)
    {
        int tilePositionsInBorderCount = tilePositionsInBorder.Count;

        float3 tilePos;
        float3 neighbourPos;
        float3 toCheckTilePos;

        NativeList<float3> verticesList = new NativeList<float3>(Allocator.TempJob);
        NativeList<int> trianglesList = new NativeList<int>(Allocator.TempJob);

        BorderTileData borderTileData;
        int edgeId = 0;

        float3[] directions = new float3[4]
        {
            new float3(-1, 0, 0), // Left
            new float3( 1, 0, 0), // Right
            new float3( 0, 0, 1), // Up
            new float3( 0, 0, -1) // Down
        };

        for (int i = 0; i < tilePositionsInBorderCount; i++)
        {
            borderTileData = new BorderTileData(4);

            tilePos = tilePositionsInBorder[i];

            for (int i2 = 0; i2 < 4; i2++)
            {
                neighbourPos = tilePos + directions[i2];

                for (int i3 = 0; i3 < tilePositionsInBorderCount; i3++)
                {
                    toCheckTilePos = tilePositionsInBorder[i3];

                    //if tile does have a neigbour in direction[i] break loop
                    if (toCheckTilePos.x == neighbourPos.x && toCheckTilePos.z == neighbourPos.z)
                    {
                        break;
                    }

                    //if tile has no neigbour in direction[i] in entire list, mark that direction for border creation
                    if (i3 == tilePositionsInBorderCount - 1)
                    {
                        borderTileData.Add(tilePos, neighbourPos);
                    }
                }
            }

            //if currentTile doesnt have 4 neighbours, ALWAYS meaning it on the edge, generate a (border)plane for the end mesh for it
            if (borderTileData.IsBorderTile)
            {
                //int trueEdgeCount = borderTileData.CalculateTrueEdges();

                //for every edge this border tile has, generate a mesh part for that edge
                for (int i2 = 0; i2 < borderTileData.edgeCount; i2++)
                {
                    GenerateEdgeMeshData(borderTileData.edgeCenterPositions[i2], borderTileData.edgeDirections[i2], edgeId, ref verticesList, ref trianglesList);
                    edgeId += 1;
                }
            }
        }


        NativeArray<float3> verticesArray = new NativeArray<float3>(verticesList.Length, Allocator.TempJob);
        NativeArray<int> trianglesArray = new NativeArray<int>(trianglesList.Length, Allocator.TempJob);

        MeshDataListsToArrays_JobParallel listConversion_JobParallel = new MeshDataListsToArrays_JobParallel()
        {
            verticesList = verticesList,
            trianglesList = trianglesList,

            verticesArray = verticesArray,
            trianglesArray = trianglesArray
        };

        // schedule and force complete job to convert list to array
        listConversion_JobParallel.Schedule(verticesList.Length / 8, 1).Complete(); // Process in chunks of 8 vertices per cycle

        //dispose lists, their content is now in the arrays
        verticesList.Dispose();
        trianglesList.Dispose();

        GenerateMesh(mesh, verticesArray, trianglesArray);

        //dispose arrays after mesh is created
        verticesArray.Dispose();
        trianglesArray.Dispose();
    }


    private static void GenerateEdgeMeshData(float3 centerPos, int3 direction, int edgeId, ref NativeList<float3> vertices, ref NativeList<int> triangles)
    {
        // Define width & depth based on direction
        float3 widthDir = direction.x != 0 ? new float3(1, 0, 0) : new float3(0, 0, 1);
        float3 depthDir = direction.x != 0 ? new float3(0, 1, 0) : new float3(0, -1, 0); // Thin in perpendicular axis

        // Half sizes
        float halfWidth = 0.5f;  // Always 1 unit wide
        float halfDepth = 0.1f;  // Thickness of the plane

        centerPos.y += halfDepth;




        // Front face
        vertices.Add(centerPos + (-widthDir * halfWidth) + (-depthDir * halfDepth));
        vertices.Add(centerPos + (widthDir * halfWidth) + (-depthDir * halfDepth));
        vertices.Add(centerPos + (widthDir * halfWidth) + (depthDir * halfDepth));
        vertices.Add(centerPos + (-widthDir * halfWidth) + (depthDir * halfDepth));

        // Back face (offset slightly in depth direction)
        vertices.Add(centerPos + (-widthDir * halfWidth) + (-depthDir * halfDepth));
        vertices.Add(centerPos + (widthDir * halfWidth) + (-depthDir * halfDepth));
        vertices.Add(centerPos + (widthDir * halfWidth) + (depthDir * halfDepth));
        vertices.Add(centerPos + (-widthDir * halfWidth) + (depthDir * halfDepth));


        int startVertexId = edgeId * 8;

        // Front face
        triangles.Add(startVertexId + 0); triangles.Add(startVertexId + 1); triangles.Add(startVertexId + 2);
        triangles.Add(startVertexId + 0); triangles.Add(startVertexId + 2); triangles.Add(startVertexId + 3);

        // Back face (flipped winding order)
        triangles.Add(startVertexId + 4); triangles.Add(startVertexId + 6); triangles.Add(startVertexId + 5);
        triangles.Add(startVertexId + 4); triangles.Add(startVertexId + 7); triangles.Add(startVertexId + 6);
    }


    private static void GenerateMesh(Mesh mesh, NativeArray<float3> vertices, NativeArray<int> triangles)
    {
        mesh.Clear();

        if (vertices.Length > 65535)
        {
            mesh.indexFormat = IndexFormat.UInt32;
        }

        // Use SetVertexBufferData instead of SetVertices
        mesh.SetVertexBufferParams(vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3));
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

        // Use SetIndexBufferData instead of SetTriangles
        mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
        mesh.SetIndexBufferData(triangles, 0, 0, triangles.Length);

        mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Length));

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}