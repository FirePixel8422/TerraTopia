using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour, IBuildable
{
    [SerializeField] private List<Building> buildings = new List<Building>();
    public List<Building> AvailableBuildings()
    {
        return buildings;
    }
}
