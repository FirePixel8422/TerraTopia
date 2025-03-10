using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileObject : NetworkBehaviour, IBuildable
{
    private Renderer meshRenderer;


    [SerializeField] private List<Building> buildings = new List<Building>();
    public List<Building> AvailableBuildings()
    {
        return buildings;
    }


    public void Initialize(bool activateImmediately)
    {
        if (!GridManager.DoesCloudExist(transform.position.ToVector2()))
        {
            gameObject.SetActive(activateImmediately);


            //TEMP FIX

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>(true);
            }
            meshRenderer.enabled = false;
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
