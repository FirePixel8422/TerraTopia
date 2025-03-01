using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public class TileBase : MonoBehaviour, IOnClickable, IHoverable, IBuildable
{
    [Tooltip("Whether a castle can be placed on this tile or not")]
    public bool canHoldCastle;
     

    public float _perlinHeight;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    private GameObject _currentHeldEnviromentalObject;

    [SerializeField] private Transform _enviromentalObjectPosHolder;

    [SerializeField] bool isHoldingObject;

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }
    Building[] IBuildable.buildings { get => _possibleBuildings; }

    [SerializeField] private Building[] _possibleBuildings;

    public virtual void OnClick() 
    {
        StartCoroutine(transform.ShakeObject(0.25f, 0.1f));
    }
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
                enviromentalObject._possibleEnviromentalPosHolder.rotation, transform);

            isHoldingObject = true;
        }
        else
        {
            print("Object already contains an enviromental object");
        }
    }
    public virtual void AssignObject(GameObject enviromentalObject)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;

        if (!_enviromentalObjectPosHolder) { print("No position to spawn enviromental Object. process ended"); return; }
        if (!isHoldingObject)
        {
            //Cut into chunks to avoid getting out of the screen
            _currentHeldEnviromentalObject =
                  Instantiate(enviromentalObject,
                  _enviromentalObjectPosHolder.position,
                  _enviromentalObjectPosHolder.rotation, transform);
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

