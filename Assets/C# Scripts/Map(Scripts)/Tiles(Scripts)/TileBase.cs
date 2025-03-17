using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
using System;
[Flags]
public enum TileLayer
{
    none = 0,
    ground = 1,
    water = 2,
}
public class TileBase : MonoBehaviour, IOnClickable, IHoverable, IBuildable
{
    [Tooltip("The layer of the tile(Depends on what stuff can move on)")]
    //Standard is set to Ground to prevent searching why the hell it cant move during debugging
    public TileLayer tileLayer;

    [Tooltip("Whether a castle can be placed on this tile or not")]
    public bool canHoldCastle;

    [Tooltip("In whos city is this tile")]
    public int ownedByPlayerGameId;


    public float _perlinHeight;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    [SerializeField] private TileObject _currentHeldEnviromentalObject;
    [SerializeField] private Transform _enviromentalObjectPosHolder;
    public UnitBase CurrentHeldUnit { private set; get; }
    public bool isHoldingObject { private set; get; }

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }
    [SerializeField] private List<Building> buildings;

    [SerializeField] private float shakeStrength = 0.1f;
    public bool canShake = true;

    private void OnEnable()
    {
        if (_currentHeldEnviromentalObject)
        {
            _currentHeldEnviromentalObject.DiscoverObject();
        }

        GridManager.DestroyCloud(transform.position.ToVector2());

        shakeStrength = 0.1f;
    }
    public void DoShake()
    {
        canShake = false;
        transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1).OnComplete(() => { canShake = true; });
    }



    public virtual void OnClick()
    {
        if (canShake)
        {
            DoShake();
            if (_currentHeldEnviromentalObject) { _currentHeldEnviromentalObject.transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1); }
            if (CurrentHeldUnit) { CurrentHeldUnit.transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1); }
        }
    }
    public void OnDifferentClickableClicked(GameObject newlyClickedObject)
    {
       
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



    public virtual void AssignObject(EnviromentalItemData enviromentalObject, bool activateImmediately, ulong ownerId = GameSettings.UnAssignedPlayerId)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;
        if (!enviromentalObject._possibleEnviromentalPosHolder)
        {
            print("No position to spawn enviromental Object. process ended");
            return;
        }

        if (!isHoldingObject)
        {
            GridManager.Instance.SpawnObject_ServerRPC(enviromentalObject._possibleEnviromentalPosHolder.position, enviromentalObject._possibleEnviromentalPosHolder.rotation, enviromentalObject._possibleEnviromentalObjectId, activateImmediately, ownerId, enviromentalObject.randomRotation);
            isHoldingObject = true;
        }
    }

    public virtual void AssignObject(int enviromentalObjectId, bool activateImmediately, ulong ownerId = GameSettings.UnAssignedPlayerId)
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
            GridManager.Instance.SpawnObject_ServerRPC(_enviromentalObjectPosHolder.position, _enviromentalObjectPosHolder.rotation, enviromentalObjectId, activateImmediately, ownerId);
            isHoldingObject = true;
        }
    }
    public virtual void SpawnAndAssignUnit(int enviromentalObjectId)
    {
        UnitBase spawnedUnit = Instantiate(TileObjectPrefabManager.GetValue(enviromentalObjectId), transform.position, transform.rotation).GetComponent<UnitBase>();
        AssignUnit(spawnedUnit);
        spawnedUnit.CurrentTile = this;
    }

    public virtual void AssignUnit(UnitBase UB)
    {
       CurrentHeldUnit = UB; 
    }
    public virtual void DeAssignUnit(UnitBase UB)
    {
        CurrentHeldUnit = null;
    }
    #endregion
}

