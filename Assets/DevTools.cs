using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DevTools : MonoBehaviour
{
    [ContextMenu("AddMaterials")]
    public void AddMaterials()
    {
        PlayerResourcesDataArray resourcesCopy = ResourceManager.GetResourceData();
        resourcesCopy.gems[0] = 10000000;
        resourcesCopy.wood[0] = 10000000;
        resourcesCopy.food[0] = 10000000;
        resourcesCopy.stone[0] = 10000000;


        ResourceManager.UpdateResourceData(resourcesCopy);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMaterials();
        }
    }
}
