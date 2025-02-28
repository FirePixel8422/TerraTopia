using System;
using Unity.Netcode;
using UnityEngine;



public class City : TileBase
{
    public int level;

    public TileBase[] cityTiles;


    public void UpgradeCity()
    {
        CityUpgradeData upgradeData = CityUpgradeHandler.GetCityUpgradeData(ClientManager.LocalClientGameId, level);
        level += 1;
    }
}