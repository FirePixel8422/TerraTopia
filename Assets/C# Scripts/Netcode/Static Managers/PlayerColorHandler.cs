using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerColorHandler
{
    //Array of Colors aasigned to each player
    //Get Color from playerGameId
    private static Color[] playerColors;




    /// <summary>
    /// Only call this from the server!!! _______  Initialized through TribeSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        playerColors = new Color[GameSettings.maxPlayers];
    }


    /// <summary>
    /// Add Color and bind it to player their gameId
    /// </summary>
    public static void AddPlayerColors(Color playerColor, int playerGameId)
    {
        playerColors[playerGameId] = playerColor;
    }


    /// <summary>
    /// Get Player Color from playerGameId
    /// </summary>
    /// <returns>The Color assigned to this player</returns>
    public static Color GetPlayerColor(int playerGameId)
    {
        return playerColors[playerGameId];
    }
}
