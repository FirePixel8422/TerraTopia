using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DevTools : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    private void AddMaterials_ServerRPC()
    {
        PlayerResourcesDataArray resourcesCopy = ResourceManager.GetResourceData();

        for (int i = 0; i < ClientManager.PlayerCount; i++)
        {
            resourcesCopy.gems[i] = 10000000;
            resourcesCopy.wood[i] = 10000000;
            resourcesCopy.food[i] = 10000000;
            resourcesCopy.stone[i] = 10000000;
        }


        ResourceManager.UpdateResourceData_OnServer(resourcesCopy);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMaterials_ServerRPC();
        }
    }
}
