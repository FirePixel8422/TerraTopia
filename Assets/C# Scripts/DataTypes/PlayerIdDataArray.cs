using Unity.Burst;
using Unity.Netcode;
using UnityEngine;



[System.Serializable]
[Tooltip("Netcode friendly array to store client data (clientId (also called networkId), gameId (the Xth client this is in the lobby -1) and teamId (what team the clients are part of))")]
[BurstCompile]
public struct PlayerIdDataArray : INetworkSerializable
{
    [Header("[0] = 2? client with networkId 2 is client 0")]
    [SerializeField] private ulong[] networkIds;

    [Header("[0] = 2? client with gameId 0 is in team 3")]
    [SerializeField] private int[] teamIds;



    [Header("Total clients in server that are setup by game/team id system")]
    [SerializeField] private int playerCount;

    [Tooltip("Total clients in server that are setup by game/team id system")]
    public readonly int PlayerCount => playerCount;



    [Header("Amount of teams with atleast 1 player in them")]
    [SerializeField] private int teamCount;

    [Tooltip("Amount of teams with atleast 1 player in them")]
    public readonly int TeamCount => teamCount;



    public PlayerIdDataArray(int maxPlayerCount)
    {
        networkIds = new ulong[maxPlayerCount];
        teamIds = new int[maxPlayerCount];

        //fill team ids with -1s because clients are not part of any team when they join
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
    /// Move client to new team, id -1 means no team
    /// </summary>
    [BurstCompile]
    public void MovePlayerToTeam(int clientGameId, int newTeamId, int newTeamCount)
    {
        //update teamId
        teamIds[clientGameId] = newTeamId;

        //set new TeamCount
        teamCount = newTeamCount;
    }

    #endregion




    #region Retrieve Data

    /// <summary>
    /// Get client gameId by converting that clients networkId (localPlayerId)
    /// </summary>
    /// <returns>The clients gameId</returns>
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
    public int GetPlayerTeamId(ulong toConvertNetworkId)
    {
        return teamIds[GetPlayerGameId(toConvertNetworkId)];
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