using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
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


    

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UnitSpawnHandler.Initialize();
            CityUpgradeHandler.Initialize();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectTribe_ServerRPC(int tribeId)
    {
        print("Tribe: " + tribeId + " Selected");

        UnitSpawnHandler.AddTribe_OnServer(tribeData[tribeId].unitSpawnData);
        CityUpgradeHandler.AddTribeCityData_OnServer(tribeData[tribeId].cityUpgrades);
    }
}