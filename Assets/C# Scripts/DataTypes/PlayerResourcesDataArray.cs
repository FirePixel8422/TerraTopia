using Unity.Burst;
using Unity.Netcode;
using UnityEngine;



[System.Serializable]
[Tooltip("Netcode friendly array to store resources")]
[BurstCompile]
public struct PlayerResourcesDataArray
{
    public int[] wood;
    public int[] stone;
    public int[] gems;
    public int[] food;
}