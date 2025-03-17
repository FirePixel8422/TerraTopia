using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Burst;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Manages team data setup on the server and save it in clientManager
/// </summary>
public class CoalitionManager : NetworkBehaviour
{
    public static CoalitionManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    [Tooltip("What Team does PlayerGameId belong to, where playerGameId is the idnex of this array")]
    private static int[] teamIds = new int[GameSettings.maxTeams];


    [Tooltip("Amount of players in each team")]
    private static int[] teamCounts = new int[GameSettings.maxTeams];

    [Tooltip("Amount of teams with atleast 1 player in it")]
    public static int TeamCount { get; private set; }


    [SerializeField] private float startGameTime;
    [SerializeField] private float unfairMatchStartGameTime;

    private static bool gameCanStart;
    private static bool gameIsStarting;

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
            teamIds[i] = GameSettings.maxTeams;
        }
        countDownTimerAnim = countDownTimerText.GetComponent<Animator>();
    }


    #region Team Info Related bool checks

    /// <summary>
    /// Are all clients in a team and there are at least 2 teams that are fair if the matchsettings enforce it
    /// </summary>
    [BurstCompile]
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
            if (teamIds[i] == GameSettings.maxPlayers)
            {
                return (false, false);
            }

            //update mostMembersPerTeam int after increasing teamMemberValue
            if (teamCounts[teamIds[i]] > mostMembersPerTeam)
            {
                mostMembersPerTeam = teamCounts[teamIds[i]];
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
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
            if (teamCounts[i] != GameSettings.maxPlayers && teamCounts[i] != mostMembersPerTeam)
            {
                fairTeams = false;
                break;
            }
        }

#if UNITY_EDITOR
        //if all clients are on 1 team and (the teams are fair or unfair teams are allowed): return true for valid, otherwise return false for valid.
        //return fairTeams for fairTeams
        return (fairTeams || MatchManager.matchSettings.allowUnfairTeams, fairTeams);
#endif


        //if all clients are on 1 team and (the teams are fair or unfair teams are allowed): return true for valid, otherwise return false for valid.
        //return fairTeams for fairTeams
        return (playerCount > 1 && (fairTeams || MatchManager.matchSettings.allowUnfairTeams), fairTeams);
    }


    /// <summary>
    /// Is there still at least 1 open spot free in this team?
    /// </summary>
    public static bool IsTeamFull(int teamId)
    {
        return teamCounts[teamId] == GameSettings.maxPlayersPerTeam;
    }

    #endregion




    #region Set TeamData and save it to ClientManager's PlayerIdDataArray.

    /// <summary>
    /// Update teamIds from client on the server, possibly update "topRowLastSelectedTeamIds" to store what button to move up to when back at start button
    /// </summary>
    [BurstCompile]
    public static void SetNewTeam_OnServer(int clientGameId, int oldTeamId, int newTeamId)
    {
        //check if clientGameId was on no team and swaps to a valid team
        //if clientGameId is equal to GameSettings.maxPlayers (out of bounds) the player is in no team.
        if (teamIds[clientGameId] == GameSettings.maxPlayers && newTeamId != GameSettings.maxPlayers)
        {
            TeamCount += 1;
        }
        //otherwise check if was on a team and swaps to no team
        else if (teamIds[clientGameId] != GameSettings.maxPlayers && newTeamId == GameSettings.maxPlayers)
        {
            TeamCount -= 1;
        }


        //Save TeamCounts, player left oldTeamId team and entered newTeamId Team
        teamCounts[newTeamId] += 1;

        //only subtract teamCount of olfTeamId if the player has selected a team before. because the first time a player selects a team, it has no team to deselect, and default value is 0, so team 0 will then be -1, wrong
        //after the player selected a team atleast once, this if statement will be true and teamcount[oldteamId] may be decremented
        if (teamIds[clientGameId] != GameSettings.maxPlayers)
        {
            teamCounts[oldTeamId] -= 1;
        }

        //save player TeamId
        teamIds[clientGameId] = newTeamId;

        //update data array
        UpdatePlayerIdDataArray_OnServer(clientGameId, newTeamId, TeamCount);
    }


    /// <summary>
    /// Get and update playerIdDataArray with team changes and send it back to ClientManager
    /// </summary>
    private static async void UpdatePlayerIdDataArray_OnServer(int clientGameId, int newTeamId, int newTeamCount)
    {
        PlayerIdDataArray clientIdDataArrayCopy = ClientManager.GetPlayerIdDataArray();

        clientIdDataArrayCopy.MovePlayerToTeam(clientGameId, newTeamId, newTeamCount);

        ClientManager.UpdatePlayerIdDataArray_OnServer(clientIdDataArrayCopy);


        //if teams are valid, start a countDownTimer
        (bool areTeamsValid, bool areTeamsFair) = AreTeamsValid();
        if (areTeamsValid)
        {
            //send serverTime so if the RPC is recieved a second late, the timer stats with a second less time
            Instance.RestartCountDownTimer_ClientRPC(NetworkManager.Singleton.ServerTime.TimeAsFloat, areTeamsFair);

            gameCanStart = await TryLockLobby();

            if (gameCanStart == false)
            {
                Instance.ResetCountDownTimer_ClientRPC();
                return;
            }
        }
    }

    #endregion




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
        float timeLeft;
        if (areTeamsFair)
        {
            timeLeft = startGameTime - alreadyElapsedTime;
        }
        else
        {
            timeLeft = unfairMatchStartGameTime - alreadyElapsedTime;
        }

        countDownTimerText.text = timeLeft.ToString();
        countDownTimerAnim.SetTrigger("Start");

        while (true)
        {
            yield return null;

            timeLeft -= Time.deltaTime;

            countDownTimerText.text = "Game Starts In: " + timeLeft.ToString();

            if (timeLeft <= 0)
            {
                countDownTimerAnim.SetTrigger("Complete");

                //if this client is the server, call TryStartGame
                if (IsServer)
                {
                    StartGame();
                }

                yield break;
            }
        }
    }

    public static async Task<bool> TryLockLobby()
    {
        int playerCount = ClientManager.GetPlayerIdDataArray().PlayerCount;

        await LobbyManager.SetLobbyLockStateAsync(true);

        //if no player joined last second, return true, otehrwise false
        return playerCount == NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    private void StartGame()
    {
        TribeSelecter.Instance.SelectTribe();

        return;

        Scene sceneToUnload = SceneManager.GetSceneByName("Nobe");

        SceneManager.UnLoadSceneOnNetwork(sceneToUnload);
    }




#if UNITY_EDITOR

    [Tooltip("What Team does PlayerGameId belong to, where playerGameId is the idnex of this array")]
    [SerializeField] private int[] DEBUG_teamIds;

    [Tooltip("Amount of players in each team")]
    [SerializeField] private int[] DEBUG_teamCounts;

    [Tooltip("Amount of teams with atleast 1 player in it")]
    [SerializeField] private int debugTeamCount;


    private void Update()
    {
        DEBUG_teamIds = teamIds;
        DEBUG_teamCounts = teamCounts;

        debugTeamCount = TeamCount;
    }
#endif
}
