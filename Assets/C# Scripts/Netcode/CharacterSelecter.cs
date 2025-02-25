using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelecter : MonoBehaviour
{
    [Header("ALL Units all of their cosmetics and materialData")]
    [SerializeField] private UnitCosmeticsListSO[] unitCosmeticsData;


    private void Start()
    {
        UnitSpawnHandler.Initialize();
    }



    public void SelectUnitCosmetics(int cosmeticId)
    {
        UnitSpawnHandler.AddUnitCosmetics_OnServer(unitCosmeticsData[cosmeticId].unitSpawnData);
    }
}