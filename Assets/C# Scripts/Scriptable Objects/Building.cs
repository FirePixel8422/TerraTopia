using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Building")]
public class Building : ScriptableObject
{
    public int buildingGOId;

    public Sprite buildingSprite;

    public BuildingCosts costs;

    public bool isUnit;
}