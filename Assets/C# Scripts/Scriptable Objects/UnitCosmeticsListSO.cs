using UnityEngine;



/// <summary>
/// Scriptable object container class to display lists in lists
/// </summary>
[CreateAssetMenu(fileName = "New Unit", menuName = "ScriptableObjects/Unit")]
public class UnitCosmeticsListSO : ScriptableObject
{
    public UnitSpawnData[] unitSpawnData;



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