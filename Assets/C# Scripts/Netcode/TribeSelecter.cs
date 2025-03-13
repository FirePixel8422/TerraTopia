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


    private int playerCountThatSelectedTribe;

    public static int selectedTribeId;




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UnitSpawnHandler.Initialize();
            PlayerColorHandler.Initialize();

            Cityhandler.Initialize();

            FindObjectOfType<NavButtonManager>(true).OnConfirm.AddListener((int selectedButtonId) => SelectTribe_ServerRPC(selectedButtonId, ClientManager.LocalClientGameId));
        }
    }


    public void SelectTribe()
    {
        if (tribeData.Length <= selectedTribeId)
        {
            Debug.LogError("Tribe " + selectedTribeId + " does not exist yet");
            return;
        }

        SelectTribe_ServerRPC(selectedTribeId, ClientManager.LocalClientGameId);
    }

    public UnitSpawnData[] GetSelectedTribeData()
    {
        return tribeData[selectedTribeId].unitSpawnData;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SelectTribe_ServerRPC(int tribeId, int playerGameId)
    {
        print("Tribe: " + tribeId + " Selected");

        UnitSpawnHandler.AddTribe_OnServer(tribeData[tribeId].unitSpawnData, playerGameId);
        PlayerColorHandler.AddPlayerColors_OnServer(playerColors[playerCountThatSelectedTribe], playerGameId);

        Cityhandler.AddCityData_OnServer(tribeData[tribeId].cityUpgrades, cityColorMaterials[playerGameId], playerGameId);
    }
}