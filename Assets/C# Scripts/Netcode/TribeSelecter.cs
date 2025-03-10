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
    [SerializeField] private UnitTribeListSO[] tribeData;

    [Header("Colors used for things like city borders")]
    [SerializeField] private Color[] playerColors;

    private int playerCountThatSelectedTribe;




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

    [ServerRpc(RequireOwnership = false)]
    public void SelectTribe_ServerRPC(int tribeId, int clientGameId)
    {
        print("Tribe: " + tribeId + " Selected");

        UnitSpawnHandler.AddTribe_OnServer(tribeData[tribeId].unitSpawnData, clientGameId);
        PlayerColorHandler.AddPlayerColors(playerColors[playerCountThatSelectedTribe], clientGameId);

        CityUpgradeHandler.AddTribeCityData_OnServer(tribeData[tribeId].cityUpgrades, clientGameId);
    }
}