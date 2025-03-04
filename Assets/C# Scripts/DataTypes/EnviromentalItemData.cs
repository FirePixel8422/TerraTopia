using UnityEngine;

[System.Serializable]
public struct EnviromentalItemData
{
    //The enviromental prefab which will be spawned on the _possibleEnviromentalPosHolder
    public int _possibleEnviromentalObjectId;

    //The position wher the _possibleEnviromentalObjects will be spawned.
    public Transform _possibleEnviromentalPosHolder;

    //This means how likely it is to be selected
    [Range(0, 100)]
    public int weight;


    public bool randomRotation;
}