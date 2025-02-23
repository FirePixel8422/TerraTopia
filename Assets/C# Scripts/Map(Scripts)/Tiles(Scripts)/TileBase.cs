using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TileBase : MonoBehaviour, IOnClickable, IHoverable
{
    [Tooltip("Whether a castle can be placed on this tile or not")]
    public bool canHoldCastle;
     

    public float _perlinHeight;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    [SerializeField] private GameObject _currentHeldEnviromentalObject;

    [SerializeField] private Transform enviromentalObjectPosHolder;

    [SerializeField] bool isHoldingObject;

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }

    public virtual void OnClick() { }
    public virtual void OnHover(Transform _hoverObject)
    {
        _hoverObject.transform.position = hoverObjectHolder.transform.position;
    }

    public virtual void AssignObject(EnviromentalItemData enviromentalObject)
    {
        if (!enviromentalObject._possibleEnviromentalPosHolder) { print("No position to spawn enviromental Object. process ended"); return; }
        if (!isHoldingObject)
        {
            //Cut into chunks to avoid getting out of the screen
          _currentHeldEnviromentalObject = 
                Instantiate(enviromentalObject._possibleEnviromentalObject, 
                enviromentalObject._possibleEnviromentalPosHolder.position,
                enviromentalObject._possibleEnviromentalPosHolder.rotation);

            isHoldingObject = true;
        }
        else
        {
            print("Object already contains an enviromental object");
        }
    }
    public virtual void AssignObject(GameObject enviromentalObject)
    {
        if (!enviromentalObjectPosHolder) { print("No position to spawn enviromental Object. process ended"); return; }
        if (!isHoldingObject)
        {
            //Cut into chunks to avoid getting out of the screen
            _currentHeldEnviromentalObject =
                  Instantiate(enviromentalObject,
                  enviromentalObjectPosHolder.position,
                  enviromentalObjectPosHolder.rotation);
            isHoldingObject = true;
        }
        else
        {
            print("Object already contains an enviromental object");
        }
    }

}



[System.Serializable]
public struct EnviromentalItemData
{
    //The enviromental prefab which will be spawned on the _possibleEnviromentalPosHolder
    public GameObject _possibleEnviromentalObject;

    //The position wher the _possibleEnviromentalObjects will be spawned.
    public Transform _possibleEnviromentalPosHolder;

    //This means how likely it is to be selected
    [Range(0, 100)]
    public int weight;
}

