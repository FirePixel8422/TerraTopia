using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmHouse : TileObject
{
    public WheatTile wheatTile;

    private int OwnerClientGameId;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TurnManager.OnTurnStarted += OnMyTurnStarted_OnServer;

            OwnerClientGameId = ClientManager.GetClientGameIdFromNetworkId(OwnerClientId);
        }
    }



    public void OnMyTurnStarted_OnServer()
    {
        if (wheatTile.FullyGrown)
        {
            ResourceManager.ModifyFood_OnServer(OwnerClientGameId, GameSettings.wheatFoodSize);

            wheatTile.HarvestCrop_ClientRPC();
        }
    }


    public override void OnDestroy()
    {
        if (IsServer)
        {
            TurnManager.OnTurnStarted -= OnMyTurnStarted_OnServer;
        }
    }
}
