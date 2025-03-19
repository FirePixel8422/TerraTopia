using Unity.Netcode;
using UnityEngine;


public static class Cityhandler
{
    //Array of CityUpgradeData containing city upgrades in a list for each player
    //Get CityUpgradeData from playerGameId
    private static CityUpgradeData[][] cityUpgradesList;



    private static City cityPrefab;
    //Array of City Materials containing Color Equal to playerColor
    //Get Material from playerGameId
    private static Material[] cityMaterialList;




    /// <summary>
    /// Only call this from the server!!! _______ Initialized through TribeSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        cityUpgradesList = new CityUpgradeData[MatchManager.settings.maxPlayers][];
        cityMaterialList = new Material[MatchManager.settings.maxPlayers];
    }


    /// <summary>
    /// Only call this from the server!!! _______ Add tribes cityUpgrades and add City Color Material equal to teamColor (to spare ref data and to easily acces them by playerGameId)
    /// </summary>
    public static void AddCityData_OnServer(CityUpgradeData[] cityUpgrades, Material cityMaterial, int playerGameId)
    {
        cityUpgradesList[playerGameId] = cityUpgrades;

        cityMaterialList[playerGameId] = cityMaterial;
    }


    ///// <summary>
    ///// Only call this from the server!!! _______ Instantiate Unit with correct cosmetic of the requesting client on server, then spawn it on network.
    ///// </summary>
    ///// <returns>The Spawned Unit (Not yet spawned on network)</returns>
    //public static City InstantiateCity_OnServer(ulong clientNetworkId, int playerGameId)
    //{
    //    //spawn City
    //    City spawnedCity = cityPrefab.NetworkObject.InstantiateAndSpawn(NetworkManager.Singleton, clientNetworkId, true).GetComponent<City>();
    //    spawnedCity.cityRenderer.material = cityMaterialList[playerGameId];
    //
    //    //call spawn on that unit, spawning it for everyone on the server
    //    spawnedCity(clientNetworkId, playerGameId);
    //
    //    return spawnedCity;
    //}




    /// <summary>
    /// Get City Upgrade data from playerGameId and cityLevel
    /// </summary>
    /// <returns>The cities next upgrade data</returns>
    public static CityUpgradeData GetCityUpgradeData_OnServer(int playerGameId, int cityLevel)
    {
        return cityUpgradesList[playerGameId][cityLevel];
    }

    /// <summary>
    /// Get City Colormaterial data from playerGameId
    /// </summary>
    /// <returns>The cities ColorMaterial that belongs to playerGameId</returns>
    public static Material GetCityColorMaterial_OnServer(int playerGameId)
    {
        return cityMaterialList[playerGameId];
    }
}
