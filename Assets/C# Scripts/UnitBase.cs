using Unity.Netcode;
using UnityEngine;



public class UnitBase : NetworkBehaviour
{
    private int ownerPlayerGameId;
    public int unitId;

    [SerializeField] private Renderer colorRenderer;


    
    public void OnSpawnUnit_OnServer(ulong clientNetworkId, int playerGameId)
    {
        NetworkObject.SpawnWithOwnership(clientNetworkId, true);
    }


    public override void OnNetworkSpawn()
    {
        ownerPlayerGameId = ClientManager.GetClientGameIdFromNetworkId(NetworkObject.OwnerClientId);
        if (IsServer)
        {
            OnSpawn_ClientRPC(ownerPlayerGameId);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void OnSpawn_ClientRPC(int ownerPlayerGameId)
    {
        colorRenderer.material = UnitSpawnHandler.GetTeamColorMaterial(ownerPlayerGameId, unitId);
    }
}
