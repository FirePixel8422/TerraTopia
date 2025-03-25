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
    public ObjectCosts buildingCosts;

    public void OnBuyButtonClicked()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (TurnManager.IsMyTurn == false) return;
#endif
        if (isUnit)
        {
            if (PlayerInput.Instance.CurrentBuildingTile.TryGetComponent(out TileBase tile))
            {
                //if TryBuild returns false, the player cannot afford the building
                if (ResourceManager.TrySpawnUnit(buildingCosts, tileObjectId, tile.transform.position.ToRoundedVector2()))
                {
                    buildingHandler.HideBuildingPanel();
                    print("Panel is hidden");   
                }
                else
                {
                    print("Player cannot afford this building");
                }
            }
            else
            {
                print("CurrentBuildingTile does not contain a TileBase script");
            }
        }
        else
        {
            if (PlayerInput.Instance.CurrentBuildingTile.TryGetComponent(out TileBase tile))
            {
                //if TryBuild returns false, the player cannot afford the building
                if (ResourceManager.TrySpawnBuilding(buildingCosts, tileObjectId, tile.transform.position.ToRoundedVector2()))
                {
                    buildingHandler.HideBuildingPanel();
                    print("Panel is hidden");
                }
                else
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
}
