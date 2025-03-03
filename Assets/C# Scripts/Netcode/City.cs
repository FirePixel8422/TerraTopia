using System;
using Unity.Netcode;
using UnityEngine;



public class City : TileObject
{
    public int level;

    public TileBase[] cityTiles;


    private int ownerClientGameId;



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ownerClientGameId = ClientManager.LocalClientGameId;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpgradeCity_ServerRPC()
    {
        CityUpgradeData upgradeData = CityUpgradeHandler.GetCityUpgradeData(ownerClientGameId, level);

        ResourceManager.ModifyGems_OnServer(ownerClientGameId, upgradeData.gainedGems);

        level += 1;
    }
}