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

    private void SetupCityMeshData(int ownerPlayerGameId)
    {
        cityRenderer.material = Cityhandler.GetCityColorMaterial(ownerPlayerGameId);
        borderMaterial = borderMeshFilter.GetComponent<Renderer>().material;
        borderMeshFilter.mesh = new Mesh();
        borderMeshFilter.transform.parent = null;
    }


    [ServerRpc(RequireOwnership = false)]
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


    /// <summary>
    /// Use Calculated BorderTilePositions to create/update border mesh, and update mesh to all clients too.
    /// </summary>
    [BurstCompile]
    private void RecalculateBorderMesh_OnServer()
    {
        ExpandCityBorder_ClientRPC(PlayerColorHandler.GetPlayerColor_OnServer(ownerClientGameId));
    }


    /// <summary>
    /// Calculate BorderTilePositions by checking each tile in the city's borderSize range and if it's not owned by any player, add it to the city's borderTilePositions.
    /// </summary>
    [BurstCompile]
    private void ReCalculateBorderTilePositions()
    {
        for (int xOffset = -borderSize; xOffset <= borderSize; xOffset++)
        {
            for (int zOffset = -borderSize; zOffset <= borderSize; zOffset++)
            {
                Vector2 tileGridPos = (transform.position + new Vector3(xOffset, 0, zOffset)).ToVector2();
                if (GridManager.TryGetTileByPos(tileGridPos, out TileBase tile))
                {
                    float3 tilePos = tile.transform.position;
                    if (!BorderTilePositions.Contains(tilePos) && tile.ownedByPlayerTeamId == -1)
                    {
                        tile.ownedByPlayerTeamId = ownerClientTeamId;
                        BorderTilePositions.Add(tilePos);
                    }
                }
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    [BurstCompile]
    private void ExpandCityBorder_ClientRPC(Vector4 borderColor)
    {
        ReCalculateBorderTilePositions();

        if (NetworkManager.LocalClientId == OwnerClientId)
        {
            foreach (Vector3 position in BorderTilePositions)
            {
                Vector2 tilePos = position.ToVector2();
                if (GridManager.DoesCloudExist(tilePos))
                {
                    GridManager.TryGetTileByPos(tilePos, out GameObject tile);
                    tile.SetActive(true);
                }
            }
        }

        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, BorderTilePositions.ToArray(), transform.position);
        borderMaterial.color = borderColor;
    }




#if UNITY_EDITOR

    [SerializeField] private bool DEBUG_borderMesh;

    //[SerializeField]
    private Vector3[] vertices;
    //[SerializeField]
    private int[] triangles;


    [BurstCompile]
    private void OnDrawGizmos()
    {
        if (DEBUG_borderMesh == false) return;


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


    [BurstCompile]
    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.N))
        {
            UpgradeCity_ServerRPC();
        }
    }
#endif
}