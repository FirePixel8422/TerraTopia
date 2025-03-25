using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Food,
    Stone,
    Gems
}
public class ResourceGatherer : TileObject
{
    [SerializeField] private int resourceCount;
    [SerializeField] private ResourceType resourceType; 

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        ResourceGathererManager.UpdateResource(resourceType, resourceCount);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsOwner) { return; }
        ResourceGathererManager.UpdateResource(resourceType, -resourceCount);
    }
}
