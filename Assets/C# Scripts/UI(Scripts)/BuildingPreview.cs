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
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (TurnManager.IsMyTurn == false) return;
#endif

        if (PlayerInput.Instance.CurrentBuildingTile.TryGetComponent(out TileBase tile))
        {

            if (isUnit == true && tile.CurrentHeldUnit != null)
            {
                print("Tile already holds a unit");
                return;
            }

            //if TryBuild returns false, the player cannot afford the building
            if (ResourceManager.TryBuild(buildingCosts, tileObjectId, tile.transform.position.ToRoundedVector2(), isUnit) == false)
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
