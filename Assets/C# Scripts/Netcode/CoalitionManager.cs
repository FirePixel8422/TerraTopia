using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalitionManager : MonoBehaviour
{
    public static CoalitionManager Instance { get; private set; }


    private List<int>[] teamIds;
    public int teamCount;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        teamIds = new List<int>[MatchManager.matchSettings.maxTeams];

        for (int i = 0; i < teamIds.Length; i++)
        {
            teamIds[i] = new List<int>(MatchManager.matchSettings.maxPlayersPerTeam);
        }
    }


    public List<int> GetAllPlayerGameIdsFromTeam(int teamId)
    {
        return teamIds[teamId];
    }
}
