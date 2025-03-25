using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;


public class ClientLobbyMenu : NetworkBehaviour
{
    public static ClientLobbyMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        ClientManager.OnClientDisconnectedCallback += OnClientDisconnected_OnServer;
    }



    [SerializeField] private TextMeshProUGUI[] clientNameField;
    [SerializeField] private GameObject[] kickButtonObjs;

    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject invisibleScreenCover;


    private FixedString64Bytes[] _savedFixedClientNames;

#if UNITY_EDITOR
    public string[] debugNames;
#endif


    public override void OnNetworkSpawn()
    {
        _savedFixedClientNames = new FixedString64Bytes[4];

        Invoke(nameof(RecieveLocalClientGameId), 0.1f);
    }


    private void RecieveLocalClientGameId()
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
                "OutOfNames",
                "WhyIsThisHere",
                "TheFrenchLikeBaguette",
                "Moe-Lester"
            };

            int r = Random.Range(0, funnyNames.Length);

            string localClientId = NetworkManager.LocalClientId.ToString();
            userName = funnyNames[r] + localClientId + localClientId.Length + "DEV_DEV_DEV_DEV_DEV_DEV";
        }
#endif

        if (IsServer)
        {
            startGameButton.SetActive(true);
        }

        ///ClientManager.SetLocalUserName(userName);

        int localClientGameId = ClientManager.GetClientGameId(NetworkManager.LocalClientId);

        AddClient_ServerRPC(new FixedString64Bytes(userName), localClientGameId);
    }




    public async void KickClientOrLeaveAsync(int gameId)
    {
        if (IsServer)
        {
            if (gameId == 0)
            {
                ClientManager.Instance.DisconnectAllClients_ServerRPC();

                //terminate lobby and shutdown network.
                await LobbyManager.DeleteLobbyAsync();

                NetworkManager.Shutdown();
            }
            else
            {
                //disconect client
                ClientManager.Instance.DisconnectClient_ServerRPC(gameId);
            }
        }
        else
        {
            //diconnect self
            ClientManager.Instance.DisconnectClient_ServerRPC(gameId);
        }
    }

    public async void StartMatchAsync()
    {
        invisibleScreenCover.SetActive(true);

        await LobbyManager.SetLobbyLockStateAsync(true);

        SceneManager.LoadSceneOnNetwork("Patrick");
    }


    private void OnClientDisconnected_OnServer(ulong clientNetworkId, int clientGameId, int newClientCount)
    {
        print(clientGameId + " left, " + newClientCount + " Client left");

        for (int i = clientGameId; i < newClientCount; i++)
        {
            //move down all the networkIds in the array by 1.
            _savedFixedClientNames[i] = _savedFixedClientNames[i + 1];
        }
        _savedFixedClientNames[newClientCount] = "";

        SyncClientNames_ClientRPC(_savedFixedClientNames, newClientCount);
    }


    [ServerRpc(RequireOwnership = false)]
    public void AddClient_ServerRPC(FixedString64Bytes fixedClientName, int clientGameId)
    {
        clientNameField[clientGameId].text = fixedClientName.ToString();

        _savedFixedClientNames[clientGameId] = fixedClientName;


        int clientCount = NetworkManager.ConnectedClientsIds.Count;

        SyncClientNames_ClientRPC(_savedFixedClientNames, clientCount);
    }


    [ClientRpc(RequireOwnership = false)]
    public void SyncClientNames_ClientRPC(FixedString64Bytes[] fixedClientNames, int clientCount)
    {
        string targetUserName;

        for (int i = 0; i < clientCount; i++)
        {
            targetUserName = fixedClientNames[i].ToString();

            clientNameField[i].transform.parent.gameObject.SetActive(true);

            //add kick (disconnect/leave) button for you own client
            if (targetUserName == ClientManager.LocalUserName)
            {
                clientNameField[i].text += " (You)";

                kickButtonObjs[i].SetActive(true);
            }


            //if username is an auto generated name through a dev account
            if (targetUserName.EndsWith("DEV_DEV_DEV_DEV_DEV_DEV"))
            {
                if (int.TryParse(targetUserName[^24].ToString(), out int numberCount))
                {
                    //remove "DEV_DEV_DEV_DEV_DEV_DEV", the int before that storing how many numbers there are in this names corresponding clientNetworkId AND the clientNetworkId
                    targetUserName = targetUserName.Substring(0, targetUserName.Length - 24 - numberCount);
                }
            }

            clientNameField[i].text = targetUserName;


            //add kick button for every client if you are the server (host)
            if (IsServer)
            {
                kickButtonObjs[i].SetActive(true);
            }
        }

        for (int i = 3; i >= clientCount ; i--)
        {
            clientNameField[i].transform.parent.gameObject.SetActive(false);
            kickButtonObjs[i].SetActive(false);
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        ClientManager.OnClientDisconnectedCallback -= OnClientDisconnected_OnServer;
    }


#if UNITY_EDITOR
    private void Update()
    {
        debugNames = new string[_savedFixedClientNames.Length];

        for (int i = 0; i < debugNames.Length; i++)
        {
            debugNames[i] = _savedFixedClientNames[i].ToString();
        }
    }
#endif
}
