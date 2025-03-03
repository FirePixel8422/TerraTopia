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
    [SerializeField] private TileObject _currentHeldEnviromentalObject;

    [SerializeField] private Transform _enviromentalObjectPosHolder;

    [SerializeField] bool isHoldingObject;

    public Transform hoverObjectHolder { get => transform; set => gameObject.AddComponent<Transform>(); }
    [SerializeField] private List<Building> buildings;




    private void OnEnable()
    {
        GridManager.DestroyCloud(transform.position.ToVector2());
    }


    public virtual void OnClick()
    {
        StartCoroutine(transform.ShakeObject(0.25f, 0.1f));
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
    private void SpawnObject(Transform spawnPos, GameObject objToSpawn)
    {
        if (Instantiate(objToSpawn, spawnPos.position, spawnPos.transform.rotation, transform).TryGetComponent(out TileObject tile))
        {
            _currentHeldEnviromentalObject = tile;
        }
        isHoldingObject = true;
    }
    private void SpawnObject(Transform spawnPos, GameObject objToSpawn, bool randomRot)
    {
        var spawnRot = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
        if (Instantiate(objToSpawn, spawnPos.position, quaternion.Euler(spawnRot), transform).TryGetComponent(out TileObject tile))
        {
            _currentHeldEnviromentalObject = tile;
        }
        isHoldingObject = true;
    }

     //Ugly code, will be improved (most likely)
    public virtual void AssignObject(EnviromentalItemData enviromentalObject)
    {
        if (!enviromentalObject._possibleEnviromentalPosHolder) { print("No position to spawn enviromental Object. process ended"); return; }
        if (!isHoldingObject)
        {
            if (enviromentalObject.randomRotation)
            {
                SpawnObject(enviromentalObject._possibleEnviromentalPosHolder, enviromentalObject._possibleEnviromentalObject, true);
                isHoldingObject = true;
            }
            else
            {
                SpawnObject(enviromentalObject._possibleEnviromentalPosHolder, enviromentalObject._possibleEnviromentalObject);
                isHoldingObject = true;
            }
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
            SpawnObject(_enviromentalObjectPosHolder, enviromentalObject);
        }
        else
        {
            print("Object already contains an enviromental object");
        }
    }
}

