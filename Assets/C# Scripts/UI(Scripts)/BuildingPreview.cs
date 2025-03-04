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
    public BuildingCosts buildingCosts;

    public void OnBuyButtonClicked()
    {
        print("ButtonHasBeenClicked");
        if (ResourceManager.CanAfford(buildingCosts))
        {
            if (PlayerInput.Instance.currentBuildingTile.TryGetComponent(out TileBase tile))
            {
                if (!tile.isHoldingObject)
                {
                    ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObjectId, tile);
                }
                else
                {
                    print("Tile already holds an object");
                }
            }
            else
            {
                print("CurrentBuildingTile does  not contain a TileBase script");
            }
        }
        else
        {
            print("Player cannot afford this building");
        }
    }
}
