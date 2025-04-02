using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Spawn All Units Through this class (MUST be through the server), so cosmetics work properly
/// </summary>
public static class UnitSpawnHandler
{
    //Array of UnitSpawnData containing unit prefab and materials for color, in a list for each player
    //Get unit data from playerGameId and get unitSpawnData by unitId
    //Get unit material color Data by playerGameId
    private static UnitSpawnData[][] unitCosmeticsList;




    /// <summary>
    /// Only call this from the server!!! _______ Initialized through TribeSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        //setup list by adding space for maxPlayers
        unitCosmeticsList = new UnitSpawnData[MatchManager.settings.maxPlayers][];
    }


    /// <summary>
    /// Only call this from the server!!! _______ Add copy of cosmetics from units only if a player has selected them (to spare ref data and to easily acces them by playerGameId)
    /// </summary>
    public static void AddTribe(UnitSpawnData[] unitCosmetics, int playerGameId)
    {
        unitCosmeticsList[playerGameId] = unitCosmetics;
    }




    /// <summary>
    /// Only call this from the server!!! _______ Instantiate Unit with correct cosmetic of the requesting client on server, then spawn it on network.
    /// </summary>
    /// <returns>The Spawned Unit</returns>
    public static UnitBase SpawnUnit_OnServer(int clientGameId, int unitId, Vector3 pos, Quaternion rot)
    {
        ulong clientNetworkId = ClientManager.GetClientNetworkId(clientGameId);

        //get units list from "clientGameId" and get "unitId" from that list 
        UnitBase unitBodyPrefab = unitCosmeticsList[clientGameId][unitId].body;
 
        UnitBase spawnedUnit = unitBodyPrefab.GetComponent<NetworkObject>().InstantiateAndSpawn(NetworkManager.Singleton, clientNetworkId, true, false, false, pos, rot).GetComponent<UnitBase>();

        return spawnedUnit;
    }


    /// <summary>
    /// Only call this from the server!!! _______ Instantiate Unit with correct cosmetic of the requesting client on server, then spawn it on network.
    /// </summary>
    public static void SpawnUnitHead(Transform headHolderTransform, int clientGameId, int unitId)
    {
        NetworkObject unitHeadPrefab = unitCosmeticsList[clientGameId][unitId].head;

        Object.Instantiate(unitHeadPrefab, headHolderTransform, false);
    }


    /// <summary>
    /// Get Unit Material from playerGameId and unitId
    /// </summary>
    /// <returns>The Units body Material</returns>
    public static Material GetTeamColorMaterial(int playerGameId, int unitId)
    {
        return unitCosmeticsList[playerGameId][unitId].colorMaterials[playerGameId];
    }
}
