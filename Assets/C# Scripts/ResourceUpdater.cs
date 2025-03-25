using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RecourceUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] resourcesTextObjs;

    private void Start()
    {
        ResourceManager.OnResourcesUpdated += OnresourceUpdate;
    }


    private void OnresourceUpdate(PlayerResourcesDataArray newValue)
    {
        int localClientGameId = ClientManager.LocalClientGameId;

        int food = newValue.food[localClientGameId];
        int stone = newValue.stone[localClientGameId];
        int wood = newValue.wood[localClientGameId];
        int gems = newValue.gems[localClientGameId];

        resourcesTextObjs[0].text = food.ToString();
        resourcesTextObjs[1].text = stone.ToString();
        resourcesTextObjs[2].text = wood.ToString();
        resourcesTextObjs[3].text = gems.ToString();
    }

    private void OnDestroy()
    {
        ResourceManager.OnResourcesUpdated -= OnresourceUpdate;
    }
}
