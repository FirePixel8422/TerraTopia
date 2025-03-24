using System;
using Unity.Netcode;

[Serializable]
public struct ObjectCosts : INetworkSerializable
{
    public int food;
    public int stone;
    public int wood;
    public int gems;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref food);
        serializer.SerializeValue(ref stone);
        serializer.SerializeValue(ref wood);
        serializer.SerializeValue(ref gems);
    }
}