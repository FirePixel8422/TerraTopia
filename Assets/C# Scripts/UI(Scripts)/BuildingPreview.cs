using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPreview : MonoBehaviour
{
    public BuildingHandler buildingHandler;
    public Image previewImage;
    public GameObject tileObject;

    //The materials needed to buy the building
    public BuildingCosts buildingCosts;

    public void OnBuyButtonClicked()
    {
        if (ResourceManager.CanBuild(buildingCosts))
        {
            if (PlayerInput.Instance.currentBuildingTile.TryGetComponent(out TileBase tile))
            {
                ResourceManager.BuildAndPayForBuilding(buildingCosts, tileObject, tile);
            }
            else
            {
                print("CurrentBuildingTile does  not contain a TileBase script");
            }
        }
    }
}
