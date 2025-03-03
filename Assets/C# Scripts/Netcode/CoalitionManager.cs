using System.Threading.Tasks;
using Unity.Burst;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// Manages team data on the server
/// </summary>
public class CoalitionManager : NetworkBehaviour
{
    [Header("UI Movement Keys")]
    [SerializeField] private InputAction UIMoveInput;

    [Space(10)]

    [SerializeField] private Animator[] buttonAnims;
    [SerializeField] private Button startGameButton;

    [SerializeField] private int[] teamIds = new int[GameSettings.maxTeams];
    [SerializeField] private int[] topRowLastSelectedTeamIds = new int[GameSettings.maxTeams];


    [Tooltip("Amount of players in each team")]
    [SerializeField] private static int[] teamCounts = new int[GameSettings.maxTeams];

    [Tooltip("Amount of teams with atleast 1 player in it")]
    public static int TeamCount { get; private set; }


    /// <summary>
    /// Are all clients in a team and there are at least 2 teams that are fair if the matchsettings enforce it
    /// </summary>
    [BurstCompile]
    public (bool valid, bool fairTeams) AreTeamsValid()
    {
        int mostMembersPerTeam = 0;

        bool fairTeams = true;

        int playerCount = ClientManager.PlayerCount;

        //calculate how many clients are in each team
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
            if (teamCounts[i] != mostMembersPerTeam)
            {
                fairTeams = false;
                break;
            }
        }


        //if all clients are on 1 team and (the teams are fair or unfair teams are allowed): return true for valid, otherwise return false for valid.
        //return fairTeams for fairTeams
        return (playerCount > 1 && (fairTeams || MatchManager.matchSettings.allowUnfairTeams), fairTeams);
    }



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.onClick.AddListener(async () => await OnStartGame());
        }
    }

    private async Task OnStartGame()
    {
        int playerCount = ClientManager.GetPlayerIdDataArray().PlayerCount;

        await LobbyManager.SetLobbyLockStateAsync(true);

        SceneManager.LoadSceneOnNetwork("Nobe");
    }



    #region Input Management + Button And Data Setup

    /// <summary>
    /// fill teamIds with "GameSettings.maxTeams" (GameSettings.maxTeams" is 1 higher than highest id team > no team), setup buttons
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < teamIds.Length; i++)
        {
            teamIds[i] = GameSettings.maxTeams;
        }

        for (int i = 0; i < buttonAnims.Length; i++)
        {
            int iTemp = i;

            buttonAnims[i].GetComponent<Button>().onClick.AddListener(() => SetNewTeam_ViaMouseButton(iTemp));
        }
    }



    /// <summary>
    /// setup input events
    /// </summary>
    private void OnEnable()
    {
        UIMoveInput.Enable();
        UIMoveInput.performed += (InputAction.CallbackContext ctx) => OnWASDInputForSwapTeams_ServerRPC(ClientManager.LocalClientGameId, ctx.ReadValue<Vector2>());

        //select the last button in list (which should be the start start button and team-less button)
        for (int i = 0; i < teamIds.Length; i++)
        {
            teamIds[i] = buttonAnims.Length - 1;
        }

        buttonAnims[^1].SetTrigger("MoveError");
        buttonAnims[^1].SetBool("Selected", true);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += Disable;
    }

    /// <summary>
    /// disable input events
    /// </summary>
    private void Disable(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        UIMoveInput.Disable();
        UIMoveInput.performed -= (InputAction.CallbackContext ctx) => OnWASDInputForSwapTeams_ServerRPC(ClientManager.LocalClientGameId, ctx.ReadValue<Vector2>());
       
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Disable;
    }



    /// <summary>
    /// When a button is pressed, this function is called with that buttons id (same as teamId)
    /// </summary>
    private void SetNewTeam_ViaMouseButton(int newTeamId)
    {
        int clientGameId = ClientManager.LocalClientGameId;

        SetNewTeam_ViaMouseButton_ServerRPC(clientGameId, newTeamId);
    }

    #endregion




    #region Process player input on server and update teamId data accordingly

    /// <summary>
    /// Process WASD Input
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void OnWASDInputForSwapTeams_ServerRPC(int clientGameId, Vector2 moveInput)
    {
        //save previous teamId
        int oldTeamId = teamIds[clientGameId];

        //first move on X
        //keep track of swappedCount
        bool swapped = TrySwapTeam_OnServer(clientGameId, new Vector2(moveInput.x, 0));

        //after X, calculate move on Y
        swapped = TrySwapTeam_OnServer(clientGameId, new Vector2(0, moveInput.y)) || swapped;


        if (swapped)
        {
            int newTeamId = teamIds[clientGameId];

            UpdatePlayerIdDataArray_OnServer(clientGameId, newTeamId);

            SwapToNewTeamAnimation_ViaWASD_ClientRPC(clientGameId, oldTeamId, newTeamId, moveInput);
        }
        //if there is no team swap after X and Y movement, call Fail method
        else
        {
            FailToFollowInputAnimation_ClientRPC(clientGameId);
        }
    }


    /// <summary>
    /// Check if a new button can be selected through moveInput (WASD) from client on the server and update teamIds accordingly with "SetNewTeam_OnServer"
    /// </summary>
    private bool TrySwapTeam_OnServer(int clientGameId, Vector2 moveInput)
    {
        switch (teamIds[clientGameId])
        {
            case 4: // Center Down
                if (moveInput.y > 0) { SetNewTeam_OnServer(clientGameId, 0); return true; }
                if (moveInput.y < 0) { SetNewTeam_OnServer(clientGameId, 0); return true; }
                break;

            case 0: // Left
                if (moveInput.y != 0) { SetNewTeam_OnServer(clientGameId, 4); return true; }
                if (moveInput.x < 0) { SetNewTeam_OnServer(clientGameId, 3); return true; }
                if (moveInput.x > 0) { SetNewTeam_OnServer(clientGameId, 1); return true; }
                break;

            case 1: // Center Left
                if (moveInput.y != 0) { SetNewTeam_OnServer(clientGameId, 4); return true; }
                if (moveInput.x < 0) { SetNewTeam_OnServer(clientGameId, 0); return true; }
                if (moveInput.x > 0) { SetNewTeam_OnServer(clientGameId, 2); return true; }
                break;

            case 2: // Center Right
                if (moveInput.y != 0) { SetNewTeam_OnServer(clientGameId, 4); return true; }
                if (moveInput.x < 0) { SetNewTeam_OnServer(clientGameId, 1); return true; }
                if (moveInput.x > 0) { SetNewTeam_OnServer(clientGameId, 3); return true; }
                break;

            case 3: // Right
                if (moveInput.y != 0) { SetNewTeam_OnServer(clientGameId, 4); return true; }
                if (moveInput.x < 0) { SetNewTeam_OnServer(clientGameId, 2); return true; }
                if (moveInput.x > 0) { SetNewTeam_OnServer(clientGameId, 0); return true; }
                break;
        }

        return false;
    }


    /// <summary>
    /// Update teamIds from client on the server, possibly update "topRowLastSelectedTeamIds" to store what button to move up to when back at start button
    /// </summary>
    private void SetNewTeam_OnServer(int clientGameId, int newTeamId, bool WASDInput = true)
    {
        if (WASDInput)
        {
            //when going to top row from no team button, re select last selected top row button
            if (teamIds[clientGameId] == GameSettings.maxPlayers)
            {
                newTeamId = topRowLastSelectedTeamIds[clientGameId];
            }
            //save last top row selected button
            else if (newTeamId == GameSettings.maxPlayers)
            {
                topRowLastSelectedTeamIds[clientGameId] = teamIds[clientGameId];
            }
        }


        //check if clientGameId was on no team and swaps to a valid team
        if (teamIds[clientGameId] == GameSettings.maxPlayers && newTeamId != GameSettings.maxPlayers)
        {
            TeamCount += 1;
            teamCounts[clientGameId] += 1;
        }
        //otherwise check if was on a valid team and swaps to a valid team
        else if (teamIds[clientGameId] != GameSettings.maxPlayers && newTeamId == GameSettings.maxPlayers)
        {
            TeamCount -= 1;
            teamCounts[clientGameId] -= 1;
        }

        teamIds[clientGameId] = newTeamId;
    }


    /// <summary>
    /// Call click trhough server
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SetNewTeam_ViaMouseButton_ServerRPC(int clientGameId, int newTeamId)
    {
        int oldTeamId = teamIds[clientGameId];

        //only update teamData in playerIdDataArray if team actually changed
        if (oldTeamId != newTeamId)
        {
            SetNewTeam_OnServer(clientGameId, newTeamId, false);

            UpdatePlayerIdDataArray_OnServer(clientGameId, newTeamId);
        }

        SwapToNewTeamAnimation_ViaMouseButton_ClientRPC(clientGameId, oldTeamId, newTeamId);
    }

    #endregion




    /// <summary>
    /// Get and update playerIdDataArray with team changes and send it back to ClientManager
    /// </summary>
    private void UpdatePlayerIdDataArray_OnServer(int clientGameId, int newTeamId)
    {
        PlayerIdDataArray clientIdDataArrayCopy = ClientManager.GetPlayerIdDataArray();

        clientIdDataArrayCopy.MovePlayerToTeam(clientGameId, newTeamId);

        ClientManager.UpdatePlayerIdDataArray_OnServer(clientIdDataArrayCopy);
    }




    #region Button Animations

    /// <summary>
    /// When clicking on a button (including already selected one) select it and play undirectional animation, deselect old button.
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void SwapToNewTeamAnimation_ViaMouseButton_ClientRPC(int clientGameId, int oldTeamId, int newTeamId)
    {
        //only execute this function for the client "clientGameId"
        if (clientGameId != ClientManager.LocalClientGameId)
        {
            return;
        }

        //deselect old button
        buttonAnims[oldTeamId].SetBool("Selected", false);

        //select new button
        buttonAnims[newTeamId].SetBool("Selected", true);
        buttonAnims[newTeamId].SetTrigger("MoveError");
    }



    /// <summary>
    /// When inputing a direction where there is a button, play a directional based animation on new button and deselect old button
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void SwapToNewTeamAnimation_ViaWASD_ClientRPC(int clientGameId, int oldTeamId, int newTeamId, Vector2 moveInput)
    {
        //only execute this function for the client "clientGameId"
        if (clientGameId != ClientManager.LocalClientGameId)
        {
            return;
        }

        int animId = 0;
        if (moveInput.x > 0)
        {
            animId = 1;
        }
        if (moveInput.y > 0)
        {
            animId = 2;
        }
        if (moveInput.y < 0)
        {
            animId = 3;
        }

        //deselect old button
        buttonAnims[oldTeamId].SetBool("Selected", false);

        //select new button
        buttonAnims[newTeamId].SetBool("Selected", true);
        buttonAnims[newTeamId].SetInteger("WiggleId", animId);
    }



    /// <summary>
    /// When inputing a direction where there is no button, a failed animation will play
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void FailToFollowInputAnimation_ClientRPC(int clientGameId)
    {
        //only execute this function for the client "clientGameId"
        if (clientGameId != ClientManager.LocalClientGameId)
        {
            return;
        }

        //update button animation by shivering it
        buttonAnims[teamIds[clientGameId]].SetTrigger("MoveError");
    }

    #endregion





#if UNITY_EDITOR || DEVELOPMENT_BUILD

    public int debugTeamCount;
    private void Update()
    {
        debugTeamCount = TeamCount;
    }
#endif
}
