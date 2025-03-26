using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.EventSystems;


public class ClientManager : NetworkBehaviour
{
    public static ClientManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    private NetworkVariable<PlayerIdDataArray> playerIdDataArray = new NetworkVariable<PlayerIdDataArray>();

    /// <summary>
    /// Get PlayerIdDataArray Copy (changes on copy wont sync back to clientManager and wont cause a networkSync)
    /// </summary>
    /// <returns>Copy Of PlayerIdDataArray</returns>
    public static PlayerIdDataArray GetPlayerIdDataArray()
    {
        return Instance.playerIdDataArray.Value;
    }

    /// <summary>
    /// Set Value Of PlayerIdDataArray, Must be called from server (Will trigger networkSync)
    /// </summary>
    public static void UpdatePlayerIdDataArray_OnServer(PlayerIdDataArray newValue)
    {
        Instance.playerIdDataArray.Value = newValue;
        Instance.playerIdDataArray.SetDirty(true);
    }



    [Tooltip("Turn GameId into NetworkId")]
    public static ulong GetClientNetworkId(int gameId) => Instance.playerIdDataArray.Value.GetPlayerNetworkId(gameId);

    [Tooltip("Turn NetworkId into GameId")]
    public static int GetClientGameId(ulong networkId) => Instance.playerIdDataArray.Value.GetPlayerGameId(networkId);

    [Tooltip("Turn GameId into TeamId")]
    public static int GetClientTeamId(int gameId) => Instance.playerIdDataArray.Value.GetPlayerTeamId(gameId);



    #region OnConnect and OnDisconnect Callbacks

    [Tooltip("Invoked after NetworkManager.OnClientConnected, before updating ClientManager gameId logic. \nreturns: ulong clientId, int clientGamId, int clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientConnectedCallback;

    [Tooltip("Invoked after NetworkManager.OnClientDisconnected, before updating ClientManager gameId logic. \nreturns: ulong clientId, int clientGamId, int clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientDisconnectedCallback;

    #endregion



    #region Initialisation

    [Tooltip("Invoked after this scripts OnNetworkSpawn is fully executed")]
    private static Action OnInitialized;
    public static bool Initialized { get; private set; }

    /// <summary>
    /// Subscribe to OnInitialized event, if already initialized instead invoke the action (and dont subscribe to OnInitialized) 
    /// </summary>
    public static void SubscribeToOnInitialized(Action toExecuteAction, bool invokeIfAlreadyInitialized = true)
    {
        //if this script is already initialized
        if (Initialized)
        {
            //invoke the action if "invokeIfAlreadyInitialized" is true
            if (invokeIfAlreadyInitialized)
            {
                toExecuteAction.Invoke();
            }

            return;
        }

        OnInitialized += toExecuteAction;
    }

    #endregion



    #region Usefull LocalClient and Global Lobby Data

    [Tooltip("Local Client gameId, the number equal to the clientCount when this client joined the lobby")]
    public static int LocalClientGameId { get; private set; }

    [Tooltip("Turn NetworkId into GameId")]
    public static int LocalClientTeamId { get; private set; }

    [Tooltip("How Many Teams are there")]
    public static int TeamCount { get; private set; }



    [Tooltip("Amount of Players in server that have been setup by ClientManager (game/team ID System")]
    public static int PlayerCount => Instance.playerIdDataArray.Value.PlayerCount;

    [Tooltip("Amount of Players in server that have been setup is 1 higher then the highestPlayerId")]
    public static ulong UnAsignedPlayerId => (ulong)Instance.playerIdDataArray.Value.PlayerCount;


    [Tooltip("Local Client userName, value is set after ClientDisplayManager's OnNetworkSpawn")]
    public static string LocalUserName { get; private set; }

    private void CreateLocalUsername()
    {
        string userName = AuthenticationService.Instance.PlayerInfo.Username;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (string.IsNullOrEmpty(userName))
        {
            string[] funnyNames = new string[]
            {
                "JohnDoe",
                "WillowWilson",
                "BijnaMichael",
                "Yi-Long-Ma",
                "Loading4Ever",
                "DickSniffer",
                "CraniumSnuiver",
                "Moe-Lester",
                "69PussySlayer69",
                "HonkiePlonkie",
            };

            int r = UnityEngine.Random.Range(0, funnyNames.Length);

            userName = funnyNames[r];
        }
#endif

        LocalUserName = userName;
    }

    #endregion




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerIdDataArray.Value = new PlayerIdDataArray(MatchManager.settings.maxPlayers);

            //call manually for the server, since its only triggers for each joining client.
            OnClientConnected_OnServer(0);

            //setup server only events
            NetworkManager.OnClientConnectedCallback += OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnServer;
        }

        //setup server and client event
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnClient;

        //on value changed event of playerIdDataArray
        playerIdDataArray.OnValueChanged += (PlayerIdDataArray oldValue, PlayerIdDataArray newValue) =>
        {
            LocalClientGameId = newValue.GetPlayerGameId(NetworkManager.LocalClientId);
            LocalClientTeamId = newValue.GetPlayerTeamId(LocalClientGameId);

            TeamCount = newValue.TeamCount;
        };

        DontDestroyOnLoad(gameObject);
        CreateLocalUsername();

        //Invoke OnInitialized event after OnNetworkSpawn is fully executed, also Set initialized to true and clear OnInitialized Action
        OnInitialized?.Invoke();
        OnInitialized = null;
        Initialized = true;
    }




    #region Join and Leave Callbacks

    /// <summary>
    /// when a clients joins the lobby, called on the server only
    /// </summary>
    private void OnClientConnected_OnServer(ulong clientId)
    {
        PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;

        updatedDataArray.AddPlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;

        OnClientConnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);
    }


    /// <summary>
    /// when a client leaves the lobby, called on the server only
    /// </summary>
    private void OnClientDisconnected_OnServer(ulong clientId)
    {
        //if the diconnecting client is the server dont update data, the server is shut down anyways.
        if (clientId == 0) return;

        PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;

        updatedDataArray.RemovePlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;

        OnClientDisconnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), PlayerCount);
    }


    /// <summary>
    /// when a client leaves the lobby, called only on disconnecting client
    /// </summary>
    private void OnClientDisconnected_OnClient(ulong clientId)
    {
        //call function only on client who disconnected
        if (clientId != NetworkManager.LocalClientId)
        {
            return;
        }


        Destroy(gameObject);
        Destroy(MatchManager.Instance.gameObject);


        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnServer;
        }

        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnClient;

        //when kicked from the server, load this scene
        SceneManager.LoadScene("Setup Network");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion




    #region Kick Client and kill Server code

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(ulong clientNetworkId)
    {
        GetKicked_ClientRPC(clientNetworkId);

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(int clientGameId)
    {
        ulong clientNetworkId = GetClientNetworkId(clientGameId);

        GetKicked_ClientRPC(clientNetworkId);

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectAllClients_ServerRPC()
    {
        KickAllClients_ClientRPC();
    }


    [ClientRpc(RequireOwnership = false)]
    private void GetKicked_ClientRPC(ulong clientNetworkId)
    {
        //destroy the rejoin reference on the kicked client
        if (clientNetworkId == NetworkManager.LocalClientId)
        {
            FileManager.DeleteFile("RejoinData.json");
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void KickAllClients_ClientRPC()
    {
        //destroy the rejoin reference on the kicked client
        FileManager.DeleteFile("RejoinData.json");
    }

    #endregion



#if UNITY_EDITOR

    [SerializeField] private PlayerIdDataArray debugClientDataArray;
    private void Update()
    {
        if (playerIdDataArray != null)
        {
            debugClientDataArray = playerIdDataArray.Value;
        }
    }
#endif
}