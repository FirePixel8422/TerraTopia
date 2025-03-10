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
        unitCosmeticsList = new UnitSpawnData[GameSettings.maxPlayers][];
    }


    /// <summary>
    /// Only call this from the server!!! _______ Add copy of cosmetics from units only if a player has selected them (to spare ref data and to easily acces them by playerGameId)
    /// </summary>
    public static void AddTribe_OnServer(UnitSpawnData[] unitCosmetics, int playerGameId)
    {
        unitCosmeticsList[playerGameId] = unitCosmetics;
    }




    /// <summary>
    /// Only call this from the server!!! _______ Instantiate Unit with correct cosmetic of the requesting client on server, then spawn it on network.
    /// </summary>
    /// <returns>The Spawned Unit (Not yet spawned on network)</returns>
    public static UnitBase InstantiateUnit_OnServer(ulong clientNetworkId, int playerGameId, int unitId)
    {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("You Cant call InstantiateUnit_OnServer on a client, it MUST be called from server");
            return null;
        }
#endif

        //get units list from "clientGameId" and get "unitId" from that list 
        UnitBase unityBodyPrefab= unitCosmeticsList[playerGameId][unitId].body;
        UnitBase unitHeadPrefab = unitCosmeticsList[playerGameId][unitId].body;

        //spawn unit (locally on server)
        UnitBase spawnedUnit = Object.Instantiate(unityBodyPrefab);
        Object.Instantiate(unitHeadPrefab, spawnedUnit.headTransform);

        //call spawn on that unit, spawning it for everyone on the server
        spawnedUnit.OnSpawnUnit_OnServer(clientNetworkId, playerGameId);

        return spawnedUnit;
    }


    /// <summary>
    /// Get Unit Material from playerGameId and unitId
    /// </summary>
    /// <returns>The Units body Material</returns>
    public static Material GetTeamColorMaterial_OnServer(int playerGameId, int unitId)
    {
        return unitCosmeticsList[playerGameId][unitId].colorMaterials[playerGameId];
    }
}
