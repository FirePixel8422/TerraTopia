using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileObject : NetworkBehaviour, IBuildable, IOnClickable
{
    public bool tileCanBeBuiltOn = true;
    private Renderer meshRenderer;


    [SerializeField] private List<Building> buildings = new List<Building>();

    public List<Building> AvailableBuildings()
    {
        return buildings;
    }
     

    public void Initialize(bool activateImmediately)
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        }


        //if a cloud exists
        if (GridManager.DoesCloudExist(transform.position.ToVector2()))
        {
            meshRenderer.enabled = false;

        }
        else if (activateImmediately)
        {
            meshRenderer.enabled = true;
        }
    }

    public virtual void DiscoverObject()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
    }

    public virtual void OnClick(int playerId)
    {
       
    }

    public virtual void OnDifferentClickableClicked(GameObject newlyClickedObject)
    {

    }
}
