using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public class TribeSelecter : NetworkBehaviour
{
    public static TribeSelecter Instance;
    private void Awake()
    {
        Instance = this;
    }


    [Header("ALL Units all of their cosmetics and materialData")]
    public UnitTribeListSO[] tribeData;

    [Header("The colorMaterials of the city, SHOULD be equal to playerColors and unitColors")]
    public Material[] cityColorMaterials;

    [Header("Colors used for things like city borders")]
    [SerializeField] private Color[] playerColors;

    public static int selectedTribeId;




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerColorHandler.Initialize_OnServer();

            Cityhandler.Initialize_OnServer();
        }
        //for every NON-host client
        else
        {
            Cityhandler.Initialize_OnClients();
        }

        UnitSpawnHandler.Initialize();
        Cityhandler.AddCityMaterials(cityColorMaterials);
    }

    public UnitSpawnData[] GetSelectedTribeData()
    {
        return tribeData[selectedTribeId].unitSpawnData;
    }



    public void SelectTribe()
    {
        SelectTribe_ServerRPC(selectedTribeId, ClientManager.LocalClientGameId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectTribe_ServerRPC(int tribeId, int playerGameId)
    {
        print("Tribe: " + tribeId + " Selected");

        Cityhandler.AddCityData_OnServer(tribeData[tribeId].cityUpgrades, playerGameId);

        PlayerColorHandler.AddPlayerColors_OnServer(playerColors[playerGameId], playerGameId);

        SelectTribe_ClientRPC(tribeId, playerGameId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SelectTribe_ClientRPC(int tribeId, int playerGameId)
    {
        UnitSpawnHandler.AddTribe(tribeData[tribeId].unitSpawnData, playerGameId);
    }
}