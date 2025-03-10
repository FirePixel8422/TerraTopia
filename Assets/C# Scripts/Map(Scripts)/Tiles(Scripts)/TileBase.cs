using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
public class TileBase : MonoBehaviour, IOnClickable, IHoverable, IBuildable
{
    [Tooltip("Whether a castle can be placed on this tile or not")]
    public bool canHoldCastle;

    [Tooltip("In whos city is this tile")]
    public int ownedByPlayerGameId;


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
        if (_currentHeldEnviromentalObject) { _currentHeldEnviromentalObject.gameObject.SetActive(true); }

        GridManager.DestroyCloud(transform.position.ToVector2());

        if (shakeStrength == 0) { shakeStrength = 0.1f; }
    }


    public virtual void OnClick()
    {
        transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1);
        if (_currentHeldEnviromentalObject) { _currentHeldEnviromentalObject.transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1); }
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

    public void SetObject(ulong networkObjectId, bool activateImmediately)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].TryGetComponent(out TileObject tile))
        {
            _currentHeldEnviromentalObject = tile;
            tile.Initialize(activateImmediately);
        }

        isHoldingObject = true;
    }



    public virtual void AssignObject(EnviromentalItemData enviromentalObject, bool activateImmediately)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;
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
            GridManager.Instance.SpawnObject_ServerRPC(enviromentalObject._possibleEnviromentalPosHolder.position, enviromentalObject._possibleEnviromentalPosHolder.rotation, enviromentalObject._possibleEnviromentalObjectId, activateImmediately, enviromentalObject.randomRotation);
            isHoldingObject = true;
        }
    }

    public virtual void AssignObject(int enviromentalObjectId, bool activateImmediately)
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
            GridManager.Instance.SpawnObject_ServerRPC(_enviromentalObjectPosHolder.position, _enviromentalObjectPosHolder.rotation, enviromentalObjectId, activateImmediately);
        }
    }
    #endregion
}

