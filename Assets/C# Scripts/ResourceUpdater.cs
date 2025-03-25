using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RecourceUpdater : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI[] naam;

    public override void OnNetworkSpawn()
    {
        ResourceManager.playerResourcesDataArray.OnValueChanged += (PlayerResourcesDataArray oldValue, PlayerResourcesDataArray newValue) => OnresourceUpdate(newValue);
    }


    private void OnresourceUpdate(PlayerResourcesDataArray newValue)
    {
        int food = newValue.food[ClientManager.LocalClientGameId];
        int stone = newValue.stone[ClientManager.LocalClientGameId];
        int wood = newValue.wood[ClientManager.LocalClientGameId];
        int gems = newValue.gems[ClientManager.LocalClientGameId];

        Debug.Log($"Food: {food}, Stone: {stone}, Wood: {wood}, Gems: {gems}");

        var iterator = 0;
        var isTrue = true;
        while (isTrue)
        {
            isTrue = false;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    iterator++;
                    naam[iterator].text = food.ToString();
                }
                else if (i == 1)
                {
                    iterator++;
                    naam[iterator].text = stone.ToString();
                }
                else if (i == 2)
                {
                    iterator++;
                    naam[iterator].text = wood.ToString();
                }
                else 
                {
                    iterator++;
                    naam[iterator].text = gems.ToString();
                }
            }
        }
    }
}
