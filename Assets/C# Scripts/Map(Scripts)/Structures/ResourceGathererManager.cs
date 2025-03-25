using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGathererManager : MonoBehaviour
{

    public static int WoodPerRound = 0;
    public static int FoodPerRound = 0;
    public static int StonePerRound = 0;
    public static int GemsPerRound = 0;

    public void Awake()
    {
        TurnManager.OnMyTurnStarted += OnRoundChanged;
    }

    public static void UpdateResource(ResourceType resourceType, int resourceCount)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                WoodPerRound += resourceCount;
                break;
            case ResourceType.Food:
                FoodPerRound += resourceCount;
                break;
            case ResourceType.Stone:
                StonePerRound += resourceCount;
                break;
            case ResourceType.Gems:
                GemsPerRound += resourceCount;
                break;
        }
    }

    public void OnRoundChanged()
    {
        ResourceManager.ModifyFood_OnServer(ClientManager.LocalClientGameId, FoodPerRound);
        ResourceManager.ModifyWood_OnServer(ClientManager.LocalClientGameId, WoodPerRound);
        ResourceManager.ModifyStone_OnServer(ClientManager.LocalClientGameId, StonePerRound);
        ResourceManager.ModifyGems_OnServer(ClientManager.LocalClientGameId, GemsPerRound);
    }
}
