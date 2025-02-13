using System;
using Unity.Netcode;
using UnityEngine;


public class TurnManager : NetworkBehaviour
{
    [Header("The id of the team that is on turn, synced between every player on the server")]
    [SerializeField] private NetworkVariable<int> teamOnTurnId = new NetworkVariable<int>();


    public static Action OnTurnGranted;




    public override void OnNetworkSpawn()
    {
        teamOnTurnId.OnValueChanged += (int oldValue, int newValue) =>
        {
            if (newValue == ClientManager.LocalClientGameId)
            {
                OnTurnGranted?.Invoke();
            }
        };
    }


    [ServerRpc(RequireOwnership = false)]
    public void NextTeam_ServerRPC()
    {
        int newTeamOnTurnId = teamOnTurnId.Value + 1;

        if (newTeamOnTurnId == CoalitionManager.Instance.teamCount)
        {
            newTeamOnTurnId = 0;
        }

        teamOnTurnId.Value = newTeamOnTurnId;
    }
}