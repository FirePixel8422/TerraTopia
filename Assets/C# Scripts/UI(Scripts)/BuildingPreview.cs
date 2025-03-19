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
                if (isUnit == true)
                {
                    if (tile.CurrentHeldUnit) { print("Tile already holds an unit"); return; }
                    ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObjectId, tile, isUnit);
                    return;
                }
                else if (isUnit == false)
                {
                    ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObjectId, tile, isUnit);
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
