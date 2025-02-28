using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TribeSelecter : MonoBehaviour
{
    [Header("ALL Units all of their cosmetics and materialData")]
    [SerializeField] private UnitTribeListSO[] tribeData;


    private void Awake()
    {
        UnitSpawnHandler.Initialize();
        CityUpgradeHandler.Initialize();
    }



    public void SelectTribe(int tribeId)
    {
        UnitSpawnHandler.AddTribe_OnServer(tribeData[tribeId].unitSpawnData);
        CityUpgradeHandler.AddTribeCityData_OnServer(tribeData[tribeId].cityUpgrades);
    }
}