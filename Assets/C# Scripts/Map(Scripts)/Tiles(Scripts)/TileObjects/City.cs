using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;



public class City : TileObject
{
    public int level;
    public int borderSize = 1;

    [SerializeField] private MeshFilter borderMeshFilter;

    [SerializeField] List<float3> borderTilePositions;


    private int ownerClientGameId;



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ownerClientGameId = ClientManager.LocalClientGameId;
        }
    }


    [ContextMenu("Generate")]
    public void GenerateBorder()
    {
        Stopwatch sw = Stopwatch.StartNew();

        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, borderTilePositions);

        sw.Stop();
        print("Generating Border Took: " + sw.ElapsedMilliseconds + " ms and: " + sw.ElapsedTicks + " ticks");
    }



    [ServerRpc(RequireOwnership = false)]
    public void UpgradeCity_ServerRPC()
    {
        CityUpgradeData upgradeData = CityUpgradeHandler.GetCityUpgradeData(ownerClientGameId, level);

        ResourceManager.ModifyGems_OnServer(ownerClientGameId, upgradeData.gainedGems);
        ResourceManager.ModifyFood_OnServer(ownerClientGameId, upgradeData.gainedFood);

        if (upgradeData.gainedBorderSize != 0)
        {
            borderSize += upgradeData.gainedBorderSize;

            int tileCount = CalculateTileCount(borderSize);


            // Loop over all tiles within the border range
            for (int xOffset = -borderSize; xOffset <= borderSize; xOffset++)
            {
                for (int zOffset = -borderSize; zOffset <= borderSize; zOffset++)
                {
                    Vector2 tileGridPos = (transform.position + new Vector3(xOffset, 0, zOffset)).ToVector2();


                    if (GridManager.TryGetTileByPos(tileGridPos, out TileBase tile))
                    {
                        float3 tilePos = tile.transform.position;

                        if (borderTilePositions.Contains(tilePos) == false && tile.ownedByPlayerGameId == -1)
                        {
                            borderTilePositions.Add(tilePos);
                        }
                    }
                }
            }

            ExpandCityBorder_ClientRPC();
        }

        level += 1;
    }

    /// <summary>
    /// Get Tilecount from 
    /// </summary>
    private int CalculateTileCount(int range)
    {
        // Formula: (2 * range + 1) ^ 2
        return (2 * range + 1) * (2 * range + 1);
    }


    [ClientRpc(RequireOwnership = false)]
    private void ExpandCityBorder_ClientRPC()
    {
        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, borderTilePositions);
    }





#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [SerializeField] private Vector3[] vertices;
    [SerializeField] private int[] triangles;


    private void OnDrawGizmos()
    {
        for (int i = 0; i < borderTilePositions.Count; i++)
        {
            Gizmos.DrawWireCube(borderTilePositions[i], Vector3.one);
        }

        if (Application.isPlaying == false)
        {
            return;
        }


        if (vertices.Length == 0)
        {
            vertices = borderMeshFilter.mesh.vertices;
            triangles = borderMeshFilter.mesh.triangles;
        }
        else
        {
            borderMeshFilter.mesh.vertices = vertices;
            borderMeshFilter.mesh.triangles = triangles;

            Gizmos.color = Color.black;

            Vector3 bPos = borderMeshFilter.transform.position;

            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawCube(bPos + vertices[i], Vector3.one * .075f);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Gizmos.DrawLine(bPos + vertices[triangles[i + 0]], bPos + vertices[triangles[i + 1]]);
                Gizmos.DrawLine(bPos + vertices[triangles[i + 1]], bPos + vertices[triangles[i + 2]]);
                Gizmos.DrawLine(bPos + vertices[triangles[i + 2]], bPos + vertices[triangles[i + 0]]);
            }
        }
    }
#endif
}