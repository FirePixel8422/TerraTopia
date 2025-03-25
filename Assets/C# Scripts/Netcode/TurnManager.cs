using System;
using Unity.Netcode;
using UnityEngine;


public class TurnManager : NetworkBehaviour
{
    [Header("Team on turn Id, synced between every client on the server")]
    [SerializeField] private NetworkVariable<int> teamOnTurnId = new NetworkVariable<int>(-1);


    [Tooltip("OnCycleStarted<int cycleId> is called when all players did one turn (Before OnTurnEnded and OnTurnStarted are called)")]
    public static Action<int> OnCycleStarted;

    private static int currentCycle;


    [Tooltip("OnTurnStarted is called on every client who his turn just started")]
    public static Action OnMyTurnStarted;

    [Tooltip("OnTurnEnded is called on every client who his turn just ended")]
    public static Action OnMyTurnEnded;


    [Tooltip("is it this clients turn")]
    public static bool IsMyTurn { get; private set; }



    public void OnAllClientsLoaded()
    {
        transform.GetChild(0).gameObject.SetActive(true);

        if (IsServer)
        {
            SetRandomStartTeam_ServerRPC();
        }

        //when the turn changes
        teamOnTurnId.OnValueChanged += (int oldTeamOnTurnId, int newTeamOnTurnId) =>
        {
            //if the last teams turn has just ended, a new cycle begins
            if (newTeamOnTurnId == 0 && oldTeamOnTurnId == (ClientManager.TeamCount - 1))
            {
                currentCycle += 1;
                OnCycleStarted?.Invoke(currentCycle);
            }

            //if this clients teamId is the same as "newTeamOnTurnId", start its turn.
            if (newTeamOnTurnId == ClientManager.LocalClientTeamId)
            {
                IsMyTurn = true;
                OnMyTurnStarted?.Invoke();
            }
            //if this clients teamId is the same as "oldTeamOnTurnId", end its turn.
            if (oldTeamOnTurnId == ClientManager.LocalClientTeamId)
            {
                IsMyTurn = false;
                OnMyTurnEnded?.Invoke();
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
        teamOnTurnId.Value = (teamOnTurnId.Value + 1) % ClientManager.TeamCount;
    }

    /// <summary>
    /// Give random team the first turn
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetRandomStartTeam_ServerRPC()
    {
        teamOnTurnId.Value = UnityEngine.Random.Range(0, ClientManager.TeamCount);
    }
}