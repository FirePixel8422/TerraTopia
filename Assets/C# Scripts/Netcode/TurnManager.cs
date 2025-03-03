using System;
using Unity.Netcode;
using UnityEngine;


public class TurnManager : NetworkBehaviour
{
    [Header("Team on turn Id, synced between every client on the server")]
    [SerializeField] private NetworkVariable<int> teamOnTurnId = new NetworkVariable<int>(-1);


    /// <summary>
    /// OnCycleStarted is called when all players did one turn (Before OnTurnEnded and OnTurnStarted are called)
    /// </summary>
    public static Action OnCycleStarted;

    /// <summary>
    /// OnTurnStarted is called on every client who his turn just started
    /// </summary>
    public static Action OnTurnStarted;

    /// <summary>
    /// OnTurnEnded is called on every client who his turn just ended
    /// </summary>
    public static Action OnTurnEnded;




    public override void OnNetworkSpawn()
    {
        //when the turn changes
        teamOnTurnId.OnValueChanged += (int oldTeamOnTurnId, int newTeamOnTurnId) =>
        {
            //if the last teams turn has just ended, a new cycle begins
            if (newTeamOnTurnId == 0 && oldTeamOnTurnId == (CoalitionManager.TeamCount - 1))
            {
                OnCycleStarted?.Invoke();
            }

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