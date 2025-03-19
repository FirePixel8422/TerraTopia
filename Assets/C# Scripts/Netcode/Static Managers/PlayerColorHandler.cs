using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerColorHandler
{
    //Array of Colors aasigned to each player
    //Get Color from playerGameId
    private static Vector4[] playerColors;




    /// <summary>
    /// Only call this from the server!!! _______  Initialized through TribeSelecter.cs
    /// </summary>
    public static void Initialize()
    {
        playerColors = new Vector4[MatchManager.settings.maxPlayers];
    }


    /// <summary>
    /// Only call this from the server!!! _______ Add Color and bind it to player their gameId
    /// </summary>
    public static void AddPlayerColors_OnServer(Color playerColor, int playerGameId)
    {
        playerColors[playerGameId] = playerColor;
    }


    /// <summary>
    /// Only call this from the server!!! _______ Get Player Color from playerGameId
    /// </summary>
    /// <returns>The Color assigned to this player</returns>
    public static Vector4 GetPlayerColor_OnServer(int playerGameId)
    {
        return playerColors[playerGameId];
    }
}
