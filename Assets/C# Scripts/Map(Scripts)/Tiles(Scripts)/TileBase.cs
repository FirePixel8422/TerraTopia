using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
public class TileBase : MonoBehaviour, IOnClickable, IHoverable, IBuildable
{
    [Tooltip("Whether a castle can be placed on this tile or not")]
    public bool canHoldCastle;


    public float _perlinHeight;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    [SerializeField] private TileObject _currentHeldEnviromentalObject;

    [SerializeField] private Transform _enviromentalObjectPosHolder;

    public bool isHoldingObject { private set; get; }

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }
    [SerializeField] private List<Building> buildings;

    [SerializeField] private float shakeStrength = 0.1f;


    private void OnEnable()
    {
        GridManager.DestroyCloud(transform.position.ToVector2());

        if(shakeStrength == 0) { shakeStrength = 0.1f; }
    }


    public virtual void OnClick()
    {
        transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1);
    }

    public virtual void OnHover(Transform _hoverObject)
    {
        _hoverObject.transform.position = hoverObjectHolder.transform.position;
    }
    public List<Building> AvailableBuildings()
    {
        if (_currentHeldEnviromentalObject == null) { return buildings; }
        else
        {
            return IBuildable.CombineLists(buildings, _currentHeldEnviromentalObject.AvailableBuildings());
        }
    }
    #region Assigning/Spawning new objects
    public void SetObject(GameObject objToSet)
    {
        if (objToSet.TryGetComponent(out TileObject tile))
        {
            _currentHeldEnviromentalObject = tile;
        }

        isHoldingObject = true;
    }



    public virtual void AssignObject(EnviromentalItemData enviromentalObject)
    {
        if (!enviromentalObject._possibleEnviromentalPosHolder)
        {
            print("No position to spawn enviromental Object. process ended");
            return;
        }

        if (isHoldingObject)
        {
            print("Object already contains an enviromental object");
        }
        else
        {
            GridManager.Instance.SpawnObject_ServerRPC(enviromentalObject._possibleEnviromentalPosHolder.position, enviromentalObject._possibleEnviromentalPosHolder.rotation, enviromentalObject._possibleEnviromentalObjectId, enviromentalObject.randomRotation);
            isHoldingObject = true;
        }
    }

    public virtual void AssignObject(int enviromentalObjectId)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;

        if (!_enviromentalObjectPosHolder)
        {
            print("No position to spawn enviromental Object. process ended");
            return;
        }

        if (isHoldingObject)
        {
            print("Object already contains an enviromental object");
        }
        else
        {
            GridManager.Instance.SpawnObject_ServerRPC(_enviromentalObjectPosHolder.position, _enviromentalObjectPosHolder.rotation, enviromentalObjectId);
        }
    }
    #endregion
}

