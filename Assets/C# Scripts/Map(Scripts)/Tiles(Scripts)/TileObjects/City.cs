using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;


[BurstCompile]
public class City : TileObject
{
    public int level;
    public int borderSize = 2;
    public float labSpeed;

    [SerializeField] private MeshRenderer cityRenderer;

    [SerializeField] private MeshFilter borderMeshFilter;
    private Material borderMaterial;

    public List<Vector3> BorderTilePositions { get; private set; } = new List<Vector3>();


    [SerializeField] private int ownerClientGameId;
    [SerializeField] private int ownerClientTeamId;



    public override void OnNetworkSpawn()
    {
        ownerClientGameId = ClientManager.GetClientGameId(OwnerClientId);
        ownerClientTeamId = ClientManager.GetClientTeamId(ownerClientGameId);

        SetupCityMeshData(ownerClientGameId);

        if (IsServer)
        {
            RecalculateBorderMesh_OnServer();
        }
        if (ownerClientGameId == ClientManager.LocalClientGameId)
        {
            LocalGameManager.ownedCities.Add(this);
        }
    }


    /// <summary>
    /// Called on all clients when the city is spawned
    /// </summary>
    private void SetupCityMeshData(int ownerPlayerGameId)
    {
        //get playerColor material
        cityRenderer.material = Cityhandler.GetCityColorMaterial(ownerPlayerGameId);


        //setup borderMeshFilter's MeshData and unparent it.
        borderMaterial = borderMeshFilter.GetComponent<Renderer>().material;
        borderMeshFilter.mesh = new Mesh();

        borderMeshFilter.transform.parent = null;
    }



    [ServerRpc(RequireOwnership = false)]
    [ContextMenu("upgrade")]
    [BurstCompile]
    public void UpgradeCity_ServerRPC()
    {
        CityUpgradeData upgradeData = Cityhandler.GetCityUpgradeData_OnServer(ownerClientGameId, level);

        ResourceManager.ModifyGems_OnServer(ownerClientGameId, upgradeData.gainedGems);
        ResourceManager.ModifyFood_OnServer(ownerClientGameId, upgradeData.gainedFood);

        if (upgradeData.gainedBorderSize != 0)
        {
            borderSize += upgradeData.gainedBorderSize;
            labSpeed += upgradeData.gainedLabSpeed;

            RecalculateBorderMesh_OnServer();
        }

        level += 1;
    }


    [BurstCompile]
    private void RecalculateBorderMesh_OnServer()
    {
        // Loop over all tiles within the border range
        for (int xOffset = -borderSize; xOffset <= borderSize; xOffset++)
        {
            for (int zOffset = -borderSize; zOffset <= borderSize; zOffset++)
            {
                Vector2 tileGridPos = (transform.position + new Vector3(xOffset, 0, zOffset)).ToVector2();

                if (GridManager.TryGetTileByPos(tileGridPos, out TileBase tile))
                {
                    float3 tilePos = tile.transform.position;

                    if (BorderTilePositions.Contains(tilePos) == false && tile.ownedByPlayerTeamId == -1)
                    {
                        tile.ownedByPlayerTeamId = ownerClientTeamId;

                        BorderTilePositions.Add(tilePos);
                    }
                }
            }
        }

        ExpandCityBorder_ClientRPC(BorderTilePositions.ToArray(), PlayerColorHandler.GetPlayerColor_OnServer(ownerClientGameId));
    }



    [ClientRpc(RequireOwnership = false)]
    [BurstCompile]
    private void ExpandCityBorder_ClientRPC(Vector3[] borderTilePositions, Vector4 borderColor)
    {
        //if clientId_ForUpdateRequest is the ownerCLient (updating his border), discover all clouds in the border
        if (NetworkManager.LocalClientId == OwnerClientId)
        {
            int borderTilesCount = borderTilePositions.Length;
            Vector2 tilePos;

            for (int i = 0; i < borderTilesCount; i++)
            {
                tilePos = borderTilePositions[i].ToVector2();

                if (GridManager.DoesCloudExist(tilePos))
                {
                    GridManager.TryGetTileByPos(tilePos, out GameObject tile);
                    tile.SetActive(true);
                }
            }
        }

        //update border mesh
        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, borderTilePositions, transform.position);

        //update border color
        borderMaterial.color = borderColor;
    }




#if UNITY_EDITOR

    //[SerializeField]
    private Vector3[] vertices;
    //[SerializeField]
    private int[] triangles;


    private void OnDrawGizmos()
    {
        for (int i = 0; i < BorderTilePositions.Count; i++)
        {
            Gizmos.DrawWireCube(BorderTilePositions[i], Vector3.one);
        }

        if (Application.isPlaying == false)
        {
            return;
        }


        if (vertices == null || vertices.Length == 0)
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