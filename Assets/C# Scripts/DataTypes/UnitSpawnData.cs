using Unity.Netcode;
using UnityEngine;



/// <summary>
/// Container that holds the models for all the unit types for 1 cosmetic type
/// </summary>
[System.Serializable]
public class UnitSpawnData
{
    public UnitBase body;
    public NetworkObject head;

    public Material[] colorMaterials;
}