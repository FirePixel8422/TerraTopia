using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPreview : MonoBehaviour
{
    public BuildingHandler buildingHandler;
    public Image previewImage;
    public int tileObjectId;

    public bool isUnit;
    //The materials needed to buy the building
    public BuildingCosts buildingCosts;

    public void OnBuyButtonClicked()
    {
        if (ResourceManager.CanAfford(buildingCosts))
        {
            if (PlayerInput.Instance.CurrentBuildingTile.TryGetComponent(out TileBase tile))
            {
                if (isUnit = true & !tile.CurrentHeldUnit)
                {
                    ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObjectId, tile, isUnit);
                    return; //Returns to prevent spawning it twice, once as a building and once as a unit
                }
                else
                {
                    print("Tile already holds a unit");
                }
                if (!tile.isHoldingObject)
                {
                    ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObjectId, tile, isUnit);
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
