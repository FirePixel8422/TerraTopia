using System;
using Unity.Netcode;
using UnityEngine;


public class TurnManager : NetworkBehaviour
{
    [Header("Team on turn Id, synced between every client on the server")]
    [SerializeField] private NetworkVariable<int> teamOnTurnId = new NetworkVariable<int>();


    public static Action OnTurnStarted;
    public static Action OnTurnEnded;




    public override void OnNetworkSpawn()
    {
        //when the turn changes
        teamOnTurnId.OnValueChanged += (int oldTeamOnTurnId, int newTeamOnTurnId) =>
        {
            //if this clients teamId is the same as "newTeamOnTurnId", start its turn.
            if (newTeamOnTurnId == ClientManager.LocalClientTeamId)
            {
                OnTurnStarted?.Invoke();
            }
            //if this clients teamId is the same as "oldTeamOnTurnId", end its turn.
            if (oldTeamOnTurnId == ClientManager.LocalClientTeamId)
            {
                OnTurnEnded?.Invoke();
            }
        };
    }



    /// <summary>
    /// called from a client ending its turn.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void NextTeam_ServerRPC()
    {
        //increment teamOnTurnId by 1 and subtract by TeamCount if its value becomes equal to TeamCount. (subtracting it will set the value to 0)
        teamOnTurnId.Value = (teamOnTurnId.Value + 1) % CoalitionManager.TeamCount;
    }
}