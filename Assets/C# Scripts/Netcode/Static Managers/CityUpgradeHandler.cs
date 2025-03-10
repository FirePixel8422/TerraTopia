using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CityUpgradeHandler
{
    //Array of CityUpgradeData containing city upgrades in a list for each player
    //Get CityUpgradeData from playerGameId
    private static CityUpgradeData[][] cityUpgradesList;




    /// <summary>
    /// Only call this from the server!!! _______ Initialized through TribeSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        cityUpgradesList = new CityUpgradeData[GameSettings.maxPlayers][];
    }


    /// <summary>
    /// Add cosmetics of units only if a player has selected them (to spare ref data and to easily acces them by playerGameId)
    /// </summary>
    public static void AddTribeCityData_OnServer(CityUpgradeData[] cityUpgrades, int playerGameId)
    {
        cityUpgradesList[playerGameId] = cityUpgrades;
    }


    /// <summary>
    /// Get City Upgrade data from playerGameId and cityLevel
    /// </summary>
    /// <returns>The cities next upgrade data</returns>
    public static CityUpgradeData GetCityUpgradeData(int playerGameId, int cityLevel)
    {
        return cityUpgradesList[playerGameId][cityLevel];
    }
}
