using System;
using Unity.Netcode;
using UnityEngine;



public class ClientManager : NetworkBehaviour
{
    public static ClientManager Instance;
    private void Awake()
    {
        Instance = this;
    }




    private static NetworkVariable<PlayerIdDataArray> playerIdDataArray = new NetworkVariable<PlayerIdDataArray>(new PlayerIdDataArray(GameSettings.maxPlayers));

    /// <summary>
    /// Get PlayerIdDataArray Copy (changes on copy wont sync back to clientManager and wont cause a networkSync)
    /// </summary>
    /// <returns>Copy Of PlayerIdDataArray</returns>3
    public static PlayerIdDataArray GetPlayerIdDataArray()
    {
        return playerIdDataArray.Value;
    }

    /// <summary>
    /// Set Value Of PlayerIdDataArray, Must be called from server (Will trigger networkSync)
    /// </summary>
    public static void UpdatePlayerIdDataArray_OnServer(PlayerIdDataArray newValue)
    {
        playerIdDataArray.Value = newValue;
    }



    [Tooltip("Turn GameId into NetworkId")]
    public static ulong GetClientNetworkIdFromGameId(int gameId) => playerIdDataArray.Value.GetPlayerNetworkId(gameId);

    [Tooltip("Turn NetworkId into GameId")]
    public static int GetClientGameIdFromNetworkId(ulong networkId) => playerIdDataArray.Value.GetPlayerGameId(networkId);

    [Tooltip("Turn GameId into TeamId")]
    public static int GetClientTeamId(int gameId) => playerIdDataArray.Value.GetPlayerTeamId(gameId);



    [Tooltip("Invoked after NetworkManager.OnClientConnected, before updating ClientManager gameId logic. \nreturns: ulong clientId, int clientGamId, int clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientConnectedCallback;

    [Tooltip("Invoked after NetworkManager.OnClientDisconnected, before updating ClientManager gameId logic. \nreturns: ulong clientId, int clientGamId, int clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientDisconnectedCallback;



    [Tooltip("Local Client gameId, the number equal to the clientCount when this client joined the lobby")]
    public static int LocalClientGameId { get; private set; }

    [Tooltip("Turn NetworkId into GameId")]
    public static int LocalClientTeamId { get; private set; }


    [Tooltip("Amount of Players in server that have been setup by ClientManager (game/team ID System")]
    public static int PlayerCount => playerIdDataArray.Value.PlayerCount;


    [Tooltip("Local Client userName, value is set after ClientDisplayManager's OnNetworkSpawn")]
    public static string LocalUserName { get; private set; }


    public static void SetLocalUserName(string newname)
    {
        LocalUserName = newname;
    }



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
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
        };
    }



    #region Join and Leave Callbacks

    /// <summary>
    /// when a clients joins the lobby, called on the server only
    /// </summary>
    private void OnClientConnected_OnServer(ulong clientId)
    {
        OnClientConnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);

        PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;

        updatedDataArray.AddPlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;
    }


    /// <summary>
    /// when a client leaves the lobby, called on the server only
    /// </summary>
    private void OnClientDisconnected_OnServer(ulong clientId)
    {
        //if the diconnecting client is the server dont update data, the server is shut down anyways.
        if (clientId == 0)
        {
            return;
        }

        OnClientDisconnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClientsIds.Count);


        PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;

        updatedDataArray.RemovePlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;
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
        Destroy(LobbyMaker.Instance.gameObject);


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
        ulong clientNetworkId = GetClientNetworkIdFromGameId(clientGameId);

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



#if UNITY_EDITOR || DEVELOPMENT_BUILD

    public PlayerIdDataArray debugClientDataArray;
    private void Update()
    {
        debugClientDataArray = playerIdDataArray.Value;
    }
#endif
}