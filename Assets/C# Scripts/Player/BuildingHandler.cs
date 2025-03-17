using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BuildingHandler : MonoBehaviour
{
    [SerializeField] private GameObject _buildingPanel;
    [SerializeField] private Transform _content;

    [SerializeField] private GameObject _buildingPreviewPrefab;

    private List<GameObject> _pooledObjects = new List<GameObject>();

    public void ShowBuildingPanel(IBuildable IB)
    {
        List<Building> buildings = IB.AvailableBuildings();
        if (buildings.Count == 0) return;
        _buildingPanel.gameObject.SetActive(true);

        foreach (GameObject building in _pooledObjects)
        {
            building.SetActive(false);
        }
        for (int i = 0; i < buildings.Count; i++)
        { 
            //Checks whether an object already exists. if so it will turn it on
            if(_pooledObjects.Count > i)
            {
                if (_pooledObjects[i].TryGetComponent(out BuildingPreview bp))
                {
                    bp.previewImage.sprite = buildings[i].buildingSprite;
                    bp.tileObjectId = buildings[i].buildingGOId;
                    bp.buildingCosts = buildings[i].costs;
                    bp.buildingHandler = this;
                    bp.isUnit = buildings[i].isUnit;
                    _pooledObjects[i].SetActive(true);
                }
            }

            else
            {
               var buildingButton = Instantiate(_buildingPreviewPrefab, _content);
                _pooledObjects.Add(buildingButton);
                if (buildingButton.TryGetComponent(out BuildingPreview bp))
                {
                    bp.previewImage.sprite = buildings[i].buildingSprite;
                    bp.tileObjectId = buildings[i].buildingGOId;
                    bp.buildingCosts = buildings[i].costs;
                    bp.isUnit = buildings[i].isUnit;
                    bp.buildingHandler = this;
                }
            }
        }
    }

    public void HideBuildingPanel()
    {
        _buildingPanel.gameObject.SetActive(false);
        foreach (var obj in _pooledObjects)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
