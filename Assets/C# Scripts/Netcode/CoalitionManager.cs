using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Manages team data setup on the server and save it in clientManager, then this script gets destroyed
/// </summary>
[BurstCompile]
public class CoalitionManager : NetworkBehaviour
{
    public static CoalitionManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    [Tooltip("What Team does PlayerGameId belong to, where playerGameId is the idnex of this array")]
    private static int[] teamIds = new int[MatchManager.settings.maxPlayers];


    [Tooltip("Amount of players in each team")]
    private static int[] teamCounts = new int[MatchManager.settings.maxPlayers];

    [Tooltip("Amount of teams with atleast 1 player in it")]
    public static int TotalTeamCount { get; private set; }


    [SerializeField] private float startTime;
    [SerializeField] private float unfairTeamsStartTime;

    [SerializeField]
    private TextMeshProUGUI countDownTimerText;
    private Animator countDownTimerAnim;




    /// <summary>
    /// fill teamIds with "GameSettings.maxTeams" (GameSettings.maxTeams" is 1 higher than highest id team > no team), setup buttons
    /// </summary>
    [BurstCompile]
    private void Start()
    {
        for (int i = 0; i < teamIds.Length; i++)
        {
            teamIds[i] = -1;
        }
        countDownTimerAnim = countDownTimerText.GetComponent<Animator>();
    }


    #region Team Info Related bool checks

    /// <summary>
    /// Are all clients in a team and there are at least 2 teams that are fair if the matchsettings enforce it
    /// </summary>
    public static (bool valid, bool fairTeams) AreTeamsValid()
    {
        int mostMembersPerTeam = 0;

        bool fairTeams = true;

        int playerCount = ClientManager.PlayerCount;

        //calculate how many clients are in each team
        //and check if every player is in a team
        for (int i = 0; i < playerCount; i++)
        {
            //a player has not selected a team yet
            if (teamIds[i] == -1)
            {
                return (false, false);
            }

            //update mostMembersPerTeam int after increasing teamMemberValue
            if (teamCounts[teamIds[i]] > mostMembersPerTeam)
            {
                mostMembersPerTeam = teamCounts[teamIds[i]];
            }
        }


        //check if all teams have the same amount of clients
        for (int i = 0; i < playerCount; i++)
        {
            //check if all teams have the same amount of clients, if not set fairTeams to false
            if (teamCounts[i] != 0 && teamCounts[i] != mostMembersPerTeam)
            {
                fairTeams = false;
                break;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        //if all clients are on 1 team and (the teams are fair or unfair teams are allowed): return true for valid, otherwise return false for valid.
        //return fairTeams for fairTeams
        return (playerCount > 0, fairTeams);
#endif


        //if all clients are on 1 team and (the teams are fair or unfair teams are allowed): return true for valid, otherwise return false for valid.
        //return fairTeams for fairTeams
        return (playerCount > 1 && (fairTeams || MatchManager.settings.allowUnfairTeams), fairTeams);
    }


    /// <summary>
    /// Is there still at least 1 open spot free in this team?
    /// </summary>
    public static bool IsTeamFull(int teamId)
    {
        return teamCounts[teamId] == MatchManager.settings.maxPlayersPerTeam;
    }

    #endregion




    #region Set TeamData and save it to ClientManager's PlayerIdDataArray.

    /// <summary>
    /// Update teamIds from client on the server, possibly update "topRowLastSelectedTeamIds" to store what button to move up to when back at start button
    /// </summary>
    public static void SetNewTeam_OnServer(int clientGameId, int oldTeamId, int newTeamId)
    {
        //check if clientGameId was on no team and swaps to a valid team
        //if clientGameId is equal to GameSettings.maxPlayers (out of bounds) the player is in no team.
        if (teamIds[clientGameId] == -1 && newTeamId != -1)
        {
            TotalTeamCount += 1;
        }
        //otherwise check if was on a team and swaps to no team
        else if (teamIds[clientGameId] != -1 && newTeamId == -1)
        {
            TotalTeamCount -= 1;
        }


        //Save TotalTeamCounts, player left oldTeamId team and entered newTeamId Team
        teamCounts[newTeamId] += 1;

        //only subtract teamCount of olfTeamId if the player has selected a team before. because the first time a player selects a team, it has no team to deselect, and default value is 0, so team 0 will then be -1, wrong
        //after the player selected a team atleast once, this if statement will be true and teamcount[oldteamId] may be decremented
        if (teamIds[clientGameId] != -1)
        {
            teamCounts[oldTeamId] -= 1;
        }

        //save player TeamId
        teamIds[clientGameId] = newTeamId;

        //update data array
        SetTeamInPlayerIdDataArray_OnServer(clientGameId, newTeamId, TotalTeamCount);
    }


    /// <summary>
    /// Get and update playerIdDataArray with team changes and send it back to ClientManager
    /// </summary>
    private static async void SetTeamInPlayerIdDataArray_OnServer(int clientGameId, int newTeamId, int newTotalTeamCount)
    {
        PlayerIdDataArray clientIdDataArrayCopy = ClientManager.GetPlayerIdDataArray();

        clientIdDataArrayCopy.MovePlayerToTeam(clientGameId, newTeamId, newTotalTeamCount);

        ClientManager.UpdatePlayerIdDataArray_OnServer(clientIdDataArrayCopy);


        //if teams are valid, start a countDownTimer
        (bool areTeamsValid, bool areTeamsFair) = AreTeamsValid();
        if (areTeamsValid)
        {
            //send serverTime so if the RPC is recieved a second late, the timer stats with a second less time
            Instance.RestartCountDownTimer_ClientRPC(NetworkManager.Singleton.ServerTime.TimeAsFloat, areTeamsFair);

            bool gameCanStart = await TryLockLobby();

            if (gameCanStart == false)
            {
                Instance.ResetCountDownTimer_ClientRPC();
                return;
            }
        }
        else
        {
            Instance.ResetCountDownTimer_ClientRPC();
        }
    }

    #endregion




    #region CountDown Timer

    [ClientRpc(RequireOwnership = false)]
    private void RestartCountDownTimer_ClientRPC(float serverTimeWhenSent, bool areTeamsFair)
    {
        //reset any buzy countDownTimer
        StopAllCoroutines();

        StartCoroutine(StartGameDelay(NetworkManager.ServerTime.TimeAsFloat - serverTimeWhenSent, areTeamsFair));
    }

    [ClientRpc(RequireOwnership = false)]
    private void ResetCountDownTimer_ClientRPC()
    {
        //reset any buzy countDownTimer
        StopAllCoroutines();
        countDownTimerAnim.SetTrigger("Stop");
    }

    private IEnumerator StartGameDelay(float alreadyElapsedTime, bool areTeamsFair)
    {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (IsServer && DEBUG_instaStartMatch)
        {
            StartGame_OnServer();
        }
#endif


        float timeLeft;
        if (areTeamsFair)
        {
            timeLeft = startTime- alreadyElapsedTime;
        }
        else
        {
            timeLeft = unfairTeamsStartTime - alreadyElapsedTime;
        }

        countDownTimerText.text = timeLeft.ToString();
        countDownTimerAnim.SetTrigger("Start");

        while (true)
        {
            yield return null;

            timeLeft -= Time.deltaTime;

            countDownTimerText.text = "Game Starts In: " + (math.round(timeLeft * 10) / 10).ToString();

            if (timeLeft <= 0)
            {
                countDownTimerAnim.SetTrigger("Complete");

                //if this client is the server, call TryStartGame
                if (IsServer)
                {
                    StartGame_OnServer();
                }

                yield break;
            }
        }
    }

    #endregion




    #region Starting The Lobby, Loading Next Scene and Updating MatchData (tribeData teamData)

    public static async Task<bool> TryLockLobby()
    {
        int playerCount = ClientManager.PlayerCount;

        await LobbyManager.SetLobbyLockStateAsync(true);

        //if no player joined last second, return true, otehrwise false
        return playerCount == NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    [BurstCompile]
    private void StartGame_OnServer()
    {
        SceneManager.LoadSceneOnNetwork("Nobe");

        //ReOrganiseTeamData();
    }


    /// <summary>
    /// Fill team gaps
    /// </summary>
    [BurstCompile]
    private void ReOrganiseTeamData()
    {
        PlayerIdDataArray playerIdDataArrayCopy = ClientManager.GetPlayerIdDataArray();

        int maxTotalTeamCount = teamCounts.Length;
        bool teamExists;

        int cPlayerTeamId;

        int lowestTeamId;
        List<int> playersWithLowestTeamId = new List<int>(MatchManager.settings.maxPlayersPerTeam);

        //fill team gaps
        //cycle over all teams, except the last one, if that one is empty, it cant be filled because there are no teams after that one
        for (int cTeamId = 0; cTeamId < maxTotalTeamCount - 1; cTeamId++)
        {
            teamExists = false;

            for (int playerGameId = 0; playerGameId < maxTotalTeamCount; playerGameId++)
            {
                if (teamIds[playerGameId] == cTeamId)
                {
                    teamExists = true;
                    break;
                }
            }
            //if a team of cTeamId exists, check next teamId.
            if (teamExists) continue;


            //otherwise set player with lowest teamId's their team to cTeamId 
            lowestTeamId = MatchManager.settings.maxPlayers;
            playersWithLowestTeamId.Clear();

            for (int playerGameId = 0; playerGameId < maxTotalTeamCount; playerGameId++)
            {
                cPlayerTeamId = teamIds[playerGameId];

                //Skip unassigned or lower teamIds then cTeamId
                if (cPlayerTeamId == -1 || cPlayerTeamId < cTeamId) continue;


                //if teamIds[playerGameId] is the new lowest teamId, update lowestTeamId, clear list and add the playerGameId
                if (cPlayerTeamId < lowestTeamId)
                {
                    lowestTeamId = teamIds[playerGameId];

                    playersWithLowestTeamId.Clear();
                    playersWithLowestTeamId.Add(playerGameId);
                }
                //if teamIds[playerGameId] is equal to the lowest teamId, add the playerGameId
                else if (cPlayerTeamId == lowestTeamId)
                {
                    playersWithLowestTeamId.Add(playerGameId);
                }
            }


            //swap all players with lowest teamId above cteamId to cTeamId
            for (int i = 0; i < playersWithLowestTeamId.Count; i++)
            {
                playerIdDataArrayCopy.MovePlayerToTeam(playersWithLowestTeamId[i], cTeamId, TotalTeamCount);
                teamIds[playersWithLowestTeamId[i]] = cTeamId;
            }
        }

        ClientManager.UpdatePlayerIdDataArray_OnServer(playerIdDataArrayCopy);
    }

    #endregion



#if UNITY_EDITOR

    [Tooltip("What Team does PlayerGameId belong to, where playerGameId is the idnex of this array")]
    [SerializeField] private int[] DEBUG_teamIds;

    [Tooltip("Amount of players in each team")]
    [SerializeField] private int[] DEBUG_teamCounts;

    [Tooltip("Amount of teams with atleast 1 player in it")]
    [SerializeField] private int debugTotalTeamCount;



    [SerializeField] private bool DEBUG_instaStartMatch;

    [BurstCompile]
    private void Update()
    {
        DEBUG_teamIds = teamIds;
        DEBUG_teamCounts = teamCounts;

        debugTotalTeamCount = TotalTeamCount;
    }
#endif
}
