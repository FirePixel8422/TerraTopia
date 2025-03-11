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

            CityUpgradeHandler.Initialize();

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
    private void SelectTribe_ServerRPC(int tribeId, int clientGameId)
    {
        print("Tribe: " + tribeId + " Selected");

        UnitSpawnHandler.AddTribe_OnServer(tribeData[tribeId].unitSpawnData, clientGameId);
        PlayerColorHandler.AddPlayerColors(playerColors[playerCountThatSelectedTribe], clientGameId);

        CityUpgradeHandler.AddTribeCityData_OnServer(tribeData[tribeId].cityUpgrades, clientGameId);
    }
}