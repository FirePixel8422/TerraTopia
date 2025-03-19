


public class FarmHouse : TileObject
{
    public WheatTile wheatTile;

    private int OwnerClientGameId;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TurnManager.OnMyTurnStarted += OnMyTurnStarted_OnServer;

            OwnerClientGameId = ClientManager.GetClientGameId(OwnerClientId);
        }
    }



    public void OnMyTurnStarted_OnServer()
    {
        if (wheatTile.FullyGrown)
        {
            ResourceManager.ModifyFood_OnServer(OwnerClientGameId, GlobalGameSettings.wheatFoodSize);

            wheatTile.HarvestCrop_ClientRPC();
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        if (IsServer)
        {
            TurnManager.OnMyTurnStarted -= OnMyTurnStarted_OnServer;
        }
    }
}
