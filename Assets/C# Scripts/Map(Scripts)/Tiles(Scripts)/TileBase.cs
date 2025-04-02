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
    public int ownedByPlayerTeamId;


    public float _perlinHeight;

    [Header("Enviromental Object Settings / Variables")]
    public List<EnviromentalItemData> _possibleEnviromentalObjects = new List<EnviromentalItemData>();
    [SerializeField] private TileObject _tileObject;
    private TileObject _currentHeldEnviromentalObject
    {
        get { return _tileObject; }
        set
        {
            // If there's a current object and it has a NetworkObject component
            if (_tileObject != null && _tileObject.NetworkObject != null && _tileObject.NetworkObject.IsSpawned)
            {
                // Despawn the current object properly
                //_tileObject.NetworkObject.Despawn();
            }

            _tileObject = value;

            if (_tileObject != null)
            {
                _tileObject.Initialize(true); // Replace with your actual initialization method
            }
        }
    }


    [SerializeField] private Transform _enviromentalObjectPosHolder;
    public UnitBase CurrentHeldUnit { private set; get; }
    public bool IsHoldingObject { private set; get; }

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }
    [SerializeField] private List<Building> buildings;

    [SerializeField] private float shakeStrength = 0.1f;
    public bool canShake = true;

    public bool CanBeBuiltOn { get; private set; } = true;

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
    public virtual void OnClick(int playerId)
    {
        //if (canShake)
        //{
        //    DoShake();
        //    if (_currentHeldEnviromentalObject) { _currentHeldEnviromentalObject.transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1); }
        //    if (CurrentHeldUnit) { CurrentHeldUnit.transform.DOPunchPosition(new Vector3(0, 0, shakeStrength), 1); }
        //}
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
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            if (netObj.TryGetComponent(out TileObject tile))
            {
                _currentHeldEnviromentalObject = tile;
                tile.Initialize(activateImmediately);
            }
            else
            {
                Debug.LogError("Spawned object does not have a TileObject component!");
            }
        }
        else
        {
            Debug.LogError($"No object found with network ID {networkObjectId}");
        }

        IsHoldingObject = _currentHeldEnviromentalObject != null;
    }



    public virtual void AssignObject(EnviromentalItemData enviromentalObject, bool activateImmediately, ulong ownerId, bool overwriteCurrent = false)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;
        var previousPreset = CanBeBuiltOn;
        CanBeBuiltOn = TileObjectPrefabManager.GetValue(enviromentalObject._possibleEnviromentalObjectId)
            .GetComponent<TileObject>().tileCanBeBuiltOn;

        // Overwrite current object if needed
        if (overwriteCurrent && _currentHeldEnviromentalObject != null)
        {
            _currentHeldEnviromentalObject.NetworkObject.Despawn();
            _currentHeldEnviromentalObject = null;
        }

        if (IsHoldingObject && !overwriteCurrent)
        {
            Debug.LogWarning("Tile already contains an environmental object. Cannot place another one.");
            CanBeBuiltOn = previousPreset;
            return;
        }

        if (!enviromentalObject._possibleEnviromentalPosHolder)
        {
            Debug.LogWarning("No position to spawn environmental object. Process ended.");
            CanBeBuiltOn = previousPreset;
            return;
        }


        // Call the spawn function on the server
        GridManager.Instance.SpawnObject_ServerRPC(
            enviromentalObject._possibleEnviromentalPosHolder.position,
            enviromentalObject._possibleEnviromentalPosHolder.rotation,
            enviromentalObject._possibleEnviromentalObjectId,
            activateImmediately,
            ownerId,
            enviromentalObject.randomRotation
        );

        // Mark as waiting for the new object
        IsHoldingObject = true;
    }


    public virtual void AssignObject_OnServer(int enviromentalObjectId, bool activateImmediately, ulong ownerId, bool overwriteCurrent = false)
    {
        _enviromentalObjectPosHolder = _enviromentalObjectPosHolder == null ? transform : _enviromentalObjectPosHolder;
        var previousPreset = CanBeBuiltOn;
        CanBeBuiltOn = TileObjectPrefabManager.GetValue(enviromentalObjectId).GetComponent<TileObject>().tileCanBeBuiltOn;

        if (overwriteCurrent && _currentHeldEnviromentalObject != null)
        {
            _currentHeldEnviromentalObject.NetworkObject.Despawn();
            _currentHeldEnviromentalObject = null;
        }

        if (IsHoldingObject && !overwriteCurrent)
        {
            Debug.LogWarning("Tile already contains an environmental object. Cannot place another one.");
            CanBeBuiltOn = previousPreset;
            return;
        }

        Debug.Log($"Spawning object ID {enviromentalObjectId} at {_enviromentalObjectPosHolder.position}");

        GridManager.Instance.SpawnObject_ServerRPC(_enviromentalObjectPosHolder.position, _enviromentalObjectPosHolder.rotation, enviromentalObjectId, activateImmediately, ownerId);

        // Mark as "waiting for object"
        IsHoldingObject = true;
    }


    public void AssignUnit(ulong UBNetworkObjectId)
    {
        UnitBase UB = NetworkManager.Singleton.SpawnManager.SpawnedObjects[UBNetworkObjectId].GetComponent<UnitBase>();
        CurrentHeldUnit = UB;

        UB.CurrentTile = this;
    }

    public void AssignUnit(ulong UBNetworkObjectId, bool syncPos)
    {
        UnitBase UB = NetworkManager.Singleton.SpawnManager.SpawnedObjects[UBNetworkObjectId].GetComponent<UnitBase>();
        UB.transform.position = transform.position;
        CurrentHeldUnit = UB;

        UB.CurrentTile = this;
    }




    public void DeAssignUnit()
    {
        CurrentHeldUnit = null;
    }
    #endregion
}

