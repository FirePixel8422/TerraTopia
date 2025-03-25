using Unity.Collections;
using Unity.Netcode;
using UnityEngine;




[System.Serializable]
public struct MatchSettings : INetworkSerializable
{
    public int GetSavedInt(int id)
    {
        return id switch
        {
            0 => maxPlayers,
            1 => maxTeams,
            2 => maxPlayersPerTeam,
            3 => allowUnfairTeams ? 1 : 0,
            4 => seed,
            5 => mapId,
            6 => mapSize,
            7 => privateLobby ? 1 : 0,
            _ => -1,
        };
    }
    public void SetIntData(int id, int value)
    {
        switch (id)
        {
            case 0:
                maxPlayers = value;
                break;
            case 1:
                maxTeams = value;
                break;
            case 2:
                maxPlayersPerTeam = value;
                break;
            case 3:
                allowUnfairTeams = value == 1;
                break;
            case 4:
                seed = value;
                break;
            case 5:
                mapId = value;
                break;
            case 6:
                mapSize = value;
                break;
            case 7:
                privateLobby = value == 1;
                break;
            default:
#if UNITY_EDITOR
                Debug.LogError("Error asigning value in MatchSettings.cs");
#endif
                break;
        }
    }


    public int maxPlayers;
    public int maxTeams;
    public int maxPlayersPerTeam;

    public bool allowUnfairTeams;

    public int seed;
    public int mapId;
    public int mapSize;
    public bool privateLobby;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref maxPlayers);
        serializer.SerializeValue(ref maxTeams);
        serializer.SerializeValue(ref maxPlayersPerTeam);

        serializer.SerializeValue(ref allowUnfairTeams);

        serializer.SerializeValue(ref seed);
        serializer.SerializeValue(ref mapId);
        serializer.SerializeValue(ref mapSize);
        serializer.SerializeValue(ref privateLobby);
    }
}