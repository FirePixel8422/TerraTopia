using Unity.Netcode;
using UnityEngine;



public class ResourceManager : NetworkBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    public static NetworkVariable<PlayerResourcesDataArray> playerResourcesDataArray = new NetworkVariable<PlayerResourcesDataArray>();

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
    public static void UpdateResourceData_OnServer(PlayerResourcesDataArray newValue)
    {
        playerResourcesDataArray.Value = newValue;
        playerResourcesDataArray.SetDirty(true);
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
    private static bool localClientHasUpdatedResources = true;



    public static bool TrySpawnBuilding(ObjectCosts buildingCosts, int buildingToPlaceId, Vector2 tileToPlaceOnPos)
    {
#if !Unity_Editor && !DEVELOPMENT_BUILD
        //return false if client is not up to date with the latest server data OR cant afford the building
        if (localClientHasUpdatedResources == false || CanAffordObject(buildingCosts) == false) return false;
#endif

        //if the client is allowed to build AND can afford the building, build it and set "localClientHasUpdatedResources" to false until the server processes the resource payment update
        localClientHasUpdatedResources = false;

        //pay and build building on the server
        Instance.SpawnBuilding_ServerRPC(ClientManager.LocalClientGameId, buildingCosts, buildingToPlaceId, tileToPlaceOnPos.ToRoundedVector2());

        return true;
    }

    public static bool TrySpawnUnit(ObjectCosts buildingCosts, int unitId, Vector2 tileToPlaceOnPos)
    {
#if !Unity_Editor && !DEVELOPMENT_BUILD
        //return false if client is not up to date with the latest server data OR cant afford the building
        if (localClientHasUpdatedResources == false || CanAffordObject(buildingCosts) == false) return false;
#endif

        //if the client is allowed to build AND can afford the building, build it and set "localClientHasUpdatedResources" to false until the server processes the resource payment update
        localClientHasUpdatedResources = false;

        //pay and build building on the server
        Instance.SpawnUnit_ServerRPC(ClientManager.LocalClientGameId, buildingCosts, unitId, tileToPlaceOnPos.ToRoundedVector2());

        return true;
    }


    /// <summary>
    /// Check if you can afford this building by comparing buildingCosts with current materials
    /// </summary>
    private static bool CanAffordObject(ObjectCosts buildingCosts)
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
    private void SpawnBuilding_ServerRPC(int clientGameId, ObjectCosts buildingCosts, int buildingToPlaceId, Vector2 tileToPlaceOnPos)
    {
        #region Update Paid Resources

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


        GridManager.TryGetTileByPos(tileToPlaceOnPos.ToRoundedVector2(), out TileBase tileToPlaceOn);

        tileToPlaceOn.AssignObject_OnServer(buildingToPlaceId, true, ClientManager.UnAsignedPlayerId, true);
    }


    /// <summary>
    /// Pay and build the unit on the server
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnit_ServerRPC(int clientGameId, ObjectCosts buildingCosts, int unitId, Vector2 tileToPlaceOnPos)
    {
        #region Update Paid Resources

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


        GridManager.TryGetTileByPos(tileToPlaceOnPos.ToRoundedVector2(), out TileBase tileToPlaceOn);
        UnitBase spawnedUnit = UnitSpawnHandler.InstantiateUnit_OnServer(clientGameId, unitId, tileToPlaceOnPos, Quaternion.identity);
        spawnedUnit.transform.position = tileToPlaceOn.transform.position;

        tileToPlaceOn.AssignUnit_ClientRPC(spawnedUnit, true);
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
