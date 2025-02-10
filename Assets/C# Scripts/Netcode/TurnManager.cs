using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class TurnManager : NetworkBehaviour
{
    [Header("The id of the team that is on turn, synced between every player on the server")]
    [SerializeField] private NetworkVariable<int> teamOnTurnId = new NetworkVariable<int>();



    [ServerRpc(RequireOwnership = false)]
    public void NextTeam_ServerRPC()
    {
        int newClientOnTurnNetworkId = teamOnTurnId.Value + 1;

        if (newClientOnTurnNetworkId == NetworkManager.ConnectedClientsIds.Count)
        {
            newClientOnTurnNetworkId = 0;
        }

        teamOnTurnId.Value = newClientOnTurnNetworkId;
    }
}