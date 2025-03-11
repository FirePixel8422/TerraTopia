using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileObject : NetworkBehaviour, IBuildable
{
   [SerializeField] private Renderer meshRenderer;


    [SerializeField] private List<Building> buildings = new List<Building>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        }
    }
    public List<Building> AvailableBuildings()
    {
        return buildings;
    }


    public void Initialize(bool activateImmediately)
    {
        if (GridManager.DoesCloudExist(transform.position.ToVector2()))
        {
            meshRenderer.enabled = false;
        }
        else
        {
            meshRenderer.enabled = true;
        }
    }

    public virtual void DiscoverObject()
    {
        if(meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
    }
}
