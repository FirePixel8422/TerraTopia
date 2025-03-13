using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;



public class City : TileObject
{
    public int level;
    public int borderSize = 1;

    public MeshRenderer cityRenderer;

    [SerializeField] private MeshFilter borderMeshFilter;
    private Material borderMaterial;

    [SerializeField] private List<Vector3> borderTilePositions;


    private int ownerClientGameId;



    public override void OnNetworkSpawn()
    {
        borderMaterial = borderMeshFilter.GetComponent<Renderer>().material;
        borderMeshFilter.mesh = new Mesh();

        borderMeshFilter.transform.parent = null;

        if (IsServer)
        {
            ownerClientGameId = ClientManager.GetClientGameIdFromNetworkId(OwnerClientId);

            OnSpawn_ClientRPC(ownerClientGameId);
        }
    }


    public Material mat;


    [ClientRpc(RequireOwnership = false)]
    private void OnSpawn_ClientRPC(int ownerPlayerGameId)
    {
        cityRenderer.material = Cityhandler.GetCityColorMaterial_OnServer(ownerPlayerGameId);

        mat = cityRenderer.material;
    }



    [ServerRpc(RequireOwnership = false)]
    public void UpgradeCity_ServerRPC()
    {
        CityUpgradeData upgradeData = Cityhandler.GetCityUpgradeData_OnServer(ownerClientGameId, level);

        ResourceManager.ModifyGems_OnServer(ownerClientGameId, upgradeData.gainedGems);
        ResourceManager.ModifyFood_OnServer(ownerClientGameId, upgradeData.gainedFood);

        if (upgradeData.gainedBorderSize != 0)
        {
            borderSize += upgradeData.gainedBorderSize;

            RecalculateBorderMesh_ServerRPC();
        }

        level += 1;
    }


    [ServerRpc(RequireOwnership = false)]
    private void RecalculateBorderMesh_ServerRPC(ulong clientId_ForUpdateRequest = ulong.MaxValue)
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

                    if (borderTilePositions.Contains(tilePos) == false && tile.ownedByPlayerGameId == -1)
                    {
                        tile.ownedByPlayerGameId = ownerClientGameId;

                        borderTilePositions.Add(tilePos);
                    }
                }
            }
        }

        ExpandCityBorder_ClientRPC(borderTilePositions.ToArray(), PlayerColorHandler.GetPlayerColor_OnServer(ownerClientGameId), clientId_ForUpdateRequest);
    }



    [ClientRpc(RequireOwnership = false)]
    private void ExpandCityBorder_ClientRPC(Vector3[] borderTilePositions, Vector4 borderColor, ulong clientId_ForUpdateRequest = ulong.MaxValue)
    {
        if (clientId_ForUpdateRequest != ulong.MaxValue && clientId_ForUpdateRequest != NetworkManager.LocalClientId)
        {
            return;
        }

        //update border mesh
        BorderMeshCalculator.CreateBorderMesh(borderMeshFilter.mesh, borderTilePositions, transform.position);

        //update border color
        borderMaterial.color = borderColor;
    }


    public override void DiscoverObject()
    {
        base.DiscoverObject();

        RecalculateBorderMesh_ServerRPC(NetworkManager.LocalClientId);
    }




#if UNITY_EDITOR || DEVELOPMENT_BUILD

    //[SerializeField]
    private Vector3[] vertices;
    //[SerializeField]
    private int[] triangles;


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