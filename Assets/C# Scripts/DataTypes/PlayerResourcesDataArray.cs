using Unity.Burst;
using Unity.Netcode;
using UnityEngine;



[System.Serializable]
[Tooltip("Netcode friendly array to store resources")]
[BurstCompile]
public struct PlayerResourcesDataArray : INetworkSerializable
{
    public int[] wood;
    public int[] stone;
    public int[] gems;
    public int[] food;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref wood);
        serializer.SerializeValue(ref stone);
        serializer.SerializeValue(ref gems);
        serializer.SerializeValue(ref food);
    }
}