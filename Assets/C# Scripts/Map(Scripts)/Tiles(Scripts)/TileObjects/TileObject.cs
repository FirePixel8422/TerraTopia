using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileObject : NetworkBehaviour, IBuildable
{
    [SerializeField] private List<Building> buildings = new List<Building>();
    public List<Building> AvailableBuildings()
    {
        return buildings;
    }
}
