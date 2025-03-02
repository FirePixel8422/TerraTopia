using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Building")]
public class Building : ScriptableObject
{
    public GameObject buildingGO;

    public Sprite buildingSprite;

    public BuildingCosts costs;

}