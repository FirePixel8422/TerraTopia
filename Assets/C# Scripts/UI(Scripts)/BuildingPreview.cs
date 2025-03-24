using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPreview : MonoBehaviour
{
    public BuildingHandler buildingHandler;
    public Image previewImage;
    public int tileObjectId;

    //The materials needed to buy the building
    public ObjectCosts buildingCosts;

    public void OnBuyButtonClicked()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (TurnManager.IsMyTurn == false) return;
#endif

        if (PlayerInput.Instance.CurrentBuildingTile.TryGetComponent(out TileBase tile))
        {
            //if TryBuild returns false, the player cannot afford the building
            if (ResourceManager.TrySpawnBuilding(buildingCosts, tileObjectId, tile.transform.position.ToRoundedVector2()) == false)
            {
                print("Player cannot afford this building");
            }
        }
        else
        {
            print("CurrentBuildingTile does not contain a TileBase script");
        }
    }
}
