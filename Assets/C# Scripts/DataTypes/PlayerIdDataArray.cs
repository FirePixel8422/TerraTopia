using Unity.Burst;
using Unity.Netcode;
using UnityEngine;



[System.Serializable]
[Tooltip("Netcode friendly array to store player data (clientId (also called networkId), gameId (the Xth player this is in the lobby -1) and teamId (what team the players are part of))")]
[BurstCompile]
public struct PlayerIdDataArray : INetworkSerializable
{
    [Header("Total players in server")]
    [SerializeField] private ulong[] networkIds;

    [Header("[0] = 2? player with gameId 0 is in team 3")]
    [SerializeField] private int[] teamIds;



    /// <summary>
    /// Are all players in a team and are there at least 2 teams
    /// </summary>
    [BurstCompile]
    public bool AreTeamsValid()
    {
        int lastTeamId = -1;

        //loop over all teamIds (for current playerCount, eg: 2 players == loop over teh first 2 teamIds)
        for (int i = 0; i < playerCount; i++)
        {
            //if a player is not part of a team return false
            if (teamIds[i] == -1)
            {
                return false;
            }
            else
            {
                //check if a team different then the previous one was found (to check if not all players are in 1 team)
                if (lastTeamId != -1 && teamIds[i] != lastTeamId)
                {
                    //set value to -10 to check down below (-10 is used as true here)
                    lastTeamId = -10;
                }

                lastTeamId = teamIds[i];
            }
        }

        //if all players are on 1 team
        if (lastTeamId != -10)
        {
            return false;
        }

        return true;
    }



    [Header("Total players in server")]
    [SerializeField] private int playerCount;

    [Header("Total teams in server")]
    [SerializeField] private int teamCount;


    public PlayerIdDataArray(int maxPlayerCount)
    {
        networkIds = new ulong[maxPlayerCount];
        teamIds = new int[maxPlayerCount];

        //fill team ids with -1s because players are not part of any team when they join
        for (int i = 0; i < maxPlayerCount; i++)
        {
            teamIds[i] = -1;
        }

        playerCount = 0;
        teamCount = 0;
    }




    #region Update Data

    [BurstCompile]
    public void AddPlayer(ulong addedNetworkId)
    {
        networkIds[playerCount] = addedNetworkId;

        playerCount += 1;
    }


    [BurstCompile]
    public void RemovePlayer(ulong removedNetworkId)
    {
        int removedGameId = GetPlayerGameId(removedNetworkId);

        playerCount -= 1;

        for (int i = removedGameId; i < playerCount; i++)
        {
            //move down all the networkIds and teamIds in the arrays by 1.
            networkIds[i] = networkIds[i + 1];
            teamIds[i] = teamIds[i + 1];
        }
    }


    /// <summary>
    /// Move player to new team, id -1 means no team
    /// </summary>
    [BurstCompile]
    public void MovePlayerToTeam(int playerGameId, int newTeamId)
    {
        teamIds[playerGameId] = newTeamId;
    }

    #endregion




    #region Retrieve Data

    /// <summary>
    /// Get player gameId by converting that players networkId (localClientId)
    /// </summary>
    /// <returns>The players gameId</returns>
    public int GetPlayerGameId(ulong toConvertNetworkId)
    {
        //since dictionaries are not netcode friendly, there is just an networkId array, and the place in the array of the value "toConvertNetworkId" is the equivelent gameId
        for (int i = 0; i < playerCount; i++)
        {
            if (networkIds[i] == toConvertNetworkId)
            {
                return i;
            }
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string errorString = "Cant Convert Id: " + toConvertNetworkId + ", networkIds are: ";
        for (int i = 0; i < playerCount; i++)
        {
            errorString += networkIds[i] + ", ";
        }

        Debug.LogError(errorString);
#endif

        return -1;
    }

    public ulong GetPlayerNetworkId(int toConvertGameId)
    {
        return networkIds[toConvertGameId];
    }

    public int GetPlayerTeamId(int toConvertGameId)
    {
        return teamIds[toConvertGameId];
    }

    #endregion



    //Method from unity netcode to serialize this struct (PlayerIdDataArray) before sending it through a network function (RPC) so it doesnt throw an error
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref networkIds);
        serializer.SerializeValue(ref teamIds);

        serializer.SerializeValue(ref playerCount);
        serializer.SerializeValue(ref teamCount);
    }
}