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

        CalculateBorderTilePositions();

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

            CalculateBorderTilePositions();
            RecalculateBorderMesh_OnServer();
        }

        level += 1;
    }

    [BurstCompile]
    private void RecalculateBorderMesh_OnServer()
    {
        ExpandCityBorder_ClientRPC(BorderTilePositions.ToArray(), PlayerColorHandler.GetPlayerColor_OnServer(ownerClientGameId));
    }

    private void CalculateBorderTilePositions()
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
    private void ExpandCityBorder_ClientRPC(Vector3[] borderTilePositions, Vector4 borderColor)
    {
        if (NetworkManager.LocalClientId == OwnerClientId)
        {
            foreach (Vector3 position in borderTilePositions)
            {
                Vector2 tilePos = position.ToVector2();
                if (GridManager.DoesCloudExist(tilePos))
                {
                    GridManager.TryGetTileByPos(tilePos, out GameObject tile);
                    tile.SetActive(true);
                }
            }
        }

        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, borderTilePositions, transform.position);
        borderMaterial.color = borderColor;
    }



    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.N))
        {
            UpgradeCity_ServerRPC();
        }
    }
}