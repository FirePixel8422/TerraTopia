using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TileBase : MonoBehaviour, IOnClickable
{
    public float _perlinHeight;
    //The outline spawned around the 
    [SerializeField] private GameObject _outLine;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    [SerializeField] private GameObject _currentHeldEnviromentalObject;

    [SerializeField] private Transform enviromentalObjectPosHolder;

    [SerializeField] bool isHoldingObject;

    protected virtual void Start()
    {
        if (!_outLine.activeInHierarchy) { _outLine = Instantiate(_outLine, transform.position, transform.rotation, transform); }
        _outLine.SetActive(false);
    }
    protected virtual void OnMouseOver() { _outLine.SetActive(true); }
    protected virtual void OnMouseExit() { _outLine.SetActive(false); }

    public virtual void OnClick() { }

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

