using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CityUpgradeHandler
{
    //Array of CityUpgradeData containing city upgrades in a list for each player
    //Get CityUpgradeData from playerGameId
    public static List<CityUpgradeData[]> cityUpgradesList;
    



    /// <summary>
    /// Initialized through CharacterSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        cityUpgradesList = new List<CityUpgradeData[]>(GameSettings.maxPlayers);
    }


    /// <summary>
    /// Add cosmetics of units only if a player has selected them (to spare ref data and to easily acces them by playerGameId)
    /// </summary>
    public static void AddTribeCityData_OnServer(CityUpgradeData[] cityUpgrades)
    {
        cityUpgradesList.Add(cityUpgrades);
    }


    /// <summary>
    /// Get City Upgrade data form playerGameId and cityLevel
    /// </summary>
    /// <returns>The cities next upgrade data</returns>
    public static CityUpgradeData GetCityUpgradeMaterial(int playerGameId, int cityLevel)
    {
        return cityUpgradesList[playerGameId][cityLevel];
    }
}
