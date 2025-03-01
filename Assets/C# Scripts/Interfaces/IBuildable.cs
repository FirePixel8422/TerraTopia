using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public struct Building
{
    public GameObject buildingGO;

    public Sprite buildingSprite;
}
public interface IBuildable 
{
    public Building[] buildings { get;  }
}
