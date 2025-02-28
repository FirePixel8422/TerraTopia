using UnityEngine;



/// <summary>
/// Scriptable object container class to display lists in lists
/// </summary>
[CreateAssetMenu(fileName = "New Tribe Data", menuName = "ScriptableObjects/Tribe Data")]
public class UnitTribeListSO : ScriptableObject
{
    public UnitSpawnData[] unitSpawnData;
    public CityUpgradeData[] cityUpgrades;



    /// <summary>
    /// update unitIds
    /// </summary>
    private void OnValidate()
    {
        for (int i = 0; i < unitSpawnData.Length; i++)
        {
            if (unitSpawnData[i].prefab != null)
            {
                unitSpawnData[i].prefab.unitId = i;
            }
        }
    }
}