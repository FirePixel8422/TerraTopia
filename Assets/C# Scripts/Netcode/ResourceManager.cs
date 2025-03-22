using Unity.Netcode;
using UnityEngine;



public class ResourceManager : NetworkBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    private static NetworkVariable<PlayerResourcesDataArray> playerResourcesDataArray = new NetworkVariable<PlayerResourcesDataArray>();

    /// <summary>
    /// Get PlayerResourcesData Copy (changes on copy wont sync back to ResourceManager and wont cause a networkSync)
    /// </summary>
    /// <returns>Copy Of PlayerResourcesData</returns>3
    public static PlayerResourcesDataArray GetResourceData()
    {
        return playerResourcesDataArray.Value;
    }

    /// <summary>
    /// Set Value Of PlayerResourcesData, Must be called from server (Will trigger networkSync)
    /// </summary>
    public static void UpdateResourceData(PlayerResourcesDataArray newValue)
    {
        playerResourcesDataArray.Value = newValue;
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerResourcesDataArray.Value = new PlayerResourcesDataArray(MatchManager.settings.maxPlayers);
        }
    }


    #region Modify Resources on Server

    /// <summary>
    /// Update Value of food on server (Will trigger networkSync)
    /// </summary>
    public static void ModifyFood_OnServer(int clientGameId, int addedFood)
    {
        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.food[clientGameId] += addedFood;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;
    }

    /// <summary>
    /// Update Value of food on server (Will trigger networkSync)
    /// </summary>
    public static void ModifyWood_OnServer(int clientGameId, int addedWood)
    {
        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.wood[clientGameId] += addedWood;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;
    }

    /// <summary>
    /// Update Value of food on server (Will trigger networkSync)
    /// </summary>
    public static void ModifyStone_OnServer(int clientGameId, int addedStone)
    {
        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.stone[clientGameId] += addedStone;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;
    }

    /// <summary>
    /// Update Value of food on server (Will trigger networkSync)
    /// </summary>
    public static void ModifyGems_OnServer(int clientGameId, int addedGems)
    {
        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.gems[clientGameId] += addedGems;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;
    }

    #endregion




    #region Building And Payment

    /// <summary>
    /// True if localClient has the updated resource values, set to false when a client modifies those resources, then set back to true once changes are processed through the server
    /// </summary>
    private static bool localClientHasUpdatedResources;


    /// <summary>
    /// Subtract the materialcost values from current materials
    /// </summary>
    public static bool TryBuild(BuildingCosts buildingCosts, int buildingToPlaceId, Vector2 tileToPlaceOnPos, bool isUnit = false)
    {
        //return false if client is not up to date with the latest server data OR cant afford the building
        if (localClientHasUpdatedResources == false || CanBuild(buildingCosts) == false) return false;

        //if the client is allowed to build AND can afford the building, build it and set "localClientHasUpdatedResources" to false until the server processes the resource payment update
        localClientHasUpdatedResources = false;

        //pay and build building on the server
        Instance.SpawnBuilding_ServerRPC(ClientManager.LocalClientGameId, buildingCosts, buildingToPlaceId, tileToPlaceOnPos, isUnit);

        return true;
    }

    /// <summary>
    /// Check if you can afford this building by comparing buildingCosts with current materials
    /// </summary>
    private static bool CanBuild(BuildingCosts buildingCosts)
    {
        int localGameId = ClientManager.LocalClientGameId;

        //if there are not enough resources to build this building, return false
        if (buildingCosts.food > playerResourcesDataArray.Value.food[localGameId]) return false;
        if (buildingCosts.wood > playerResourcesDataArray.Value.wood[localGameId]) return false;
        if (buildingCosts.food > playerResourcesDataArray.Value.stone[localGameId]) return false;
        if (buildingCosts.gems > playerResourcesDataArray.Value.gems[localGameId]) return false;

        return true;
    }


    /// <summary>
    /// Pay and build the building on the server
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuilding_ServerRPC(int clientGameId, BuildingCosts buildingCosts, int buildingToPlaceId, Vector2 tileToPlaceOnPos, bool isUnit = false)
    {
        #region Update Payed Resources

        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.food[clientGameId] -= buildingCosts.food;
        resourceArrayCopy.wood[clientGameId] -= buildingCosts.wood;
        resourceArrayCopy.stone[clientGameId] -= buildingCosts.stone;
        resourceArrayCopy.gems[clientGameId] -= buildingCosts.gems;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;

        OnResourcesUpdated_ClientRPC(clientGameId);

        #endregion


        GridManager.TryGetTileByPos(tileToPlaceOnPos, out TileBase tileToPlaceOn);

        if (isUnit)
        {
            tileToPlaceOn.SpawnAndAssignUnit(buildingToPlaceId);
        }
        else
        {
            tileToPlaceOn.AssignObject(buildingToPlaceId, true, ClientManager.UnAsignedPlayerId, true);
        }
    }


    /// <summary>
    /// Called on client when resources are updated through server
    /// </summary>
    [ClientRpc(RequireOwnership = false)]
    private void OnResourcesUpdated_ClientRPC(int clientGameId)
    {
        //only update the client who built something
        if (clientGameId != ClientManager.LocalClientGameId) return;

        localClientHasUpdatedResources = true;
    }

    #endregion




#if UNITY_EDITOR

    [SerializeField] private PlayerResourcesDataArray debugClientDataArray;
    private void Update()
    {
        if (playerResourcesDataArray != null)
        {
            debugClientDataArray = playerResourcesDataArray.Value;
        }
    }
#endif
}
