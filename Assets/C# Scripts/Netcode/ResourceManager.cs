using Unity.Netcode;



public class ResourceManager : NetworkBehaviour
{
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
    /// Set Value Of PlayerResourcesData (Will trigger networkSync)
    /// </summary>
    public static void UpdateResourceData(PlayerResourcesDataArray newValue)
    {
        playerResourcesDataArray.Value = newValue;
    }


    /// <summary>
    /// CHeck if you can afford this building by comparing buildingCosts with current materials
    /// </summary>
    /// <returns>wheather you can afford to build this building</returns>
    public static bool CanBuild(BuildingCosts buildingCosts)
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
    /// Subtract the materialcost values from current materials
    /// </summary>
    public static void BuildAndPayForBuilding(BuildingCosts buildingCosts)
    {
        int localGameId = ClientManager.LocalClientGameId;

        //copy resourceArray
        PlayerResourcesDataArray resourceArrayCopy = playerResourcesDataArray.Value;

        resourceArrayCopy.food[localGameId] -= buildingCosts.food;
        resourceArrayCopy.wood[localGameId] -= buildingCosts.wood;
        resourceArrayCopy.stone[localGameId] -= buildingCosts.stone;
        resourceArrayCopy.gems[localGameId] -= buildingCosts.gems;

        //update copy back to original resourceArray
        playerResourcesDataArray.Value = resourceArrayCopy;
    }
}
