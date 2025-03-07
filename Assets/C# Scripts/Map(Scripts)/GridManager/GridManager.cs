using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;



[BurstCompile]
public class GridManager : NetworkBehaviour
{
    public static GridManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    private static Dictionary<Vector2, TileBase> _tiles;
    private static Dictionary<Vector2, GameObject> _clouds;


    [Header("Preset MapGeneration Values")]
    [Tooltip("If left at 0 it will generate a random seed")]
    [SerializeField] private int _seed;
    [SerializeField] private NoiseData _noiseData;
    [SerializeField] private int _castlePrefabId;
    [SerializeField] private GameObject cloudPrefab;


    //The dimensions of the to-be created grid.
    [Header("Dimensions")]
    [Tooltip("The X-axis")]
    public int _width;
    [Tooltip("The Z-axis")]
    public int _length;

    [Header("Player Values")]
    [SerializeField] private int playerCount;

    public TileObjectLibrarySO tileObjectsData;



    public override void OnNetworkSpawn()
    {
        GenerateGrid(playerCount);

        if (IsServer)
        {
            for (int i = 0; i < tileObjectsData.tileObjects.Length; i++)
            {
                TileObjectPrefabManager.Add(tileObjectsData.tileObjects[i]);
            }

            SpawnMapAssets_OnServer();
        }
    }

    //private void Start()
    //{
    //    GenerateGrid(playerCount);
    //}

    [BurstCompile]
    private void GenerateGrid(int playerCount)
    {
        //Checks whether the seed is already pre-determined or not with use of the Ternary Operator
        _seed = _seed == 0 ? _seed = Random.Range(int.MinValue, int.MaxValue) : _seed;

        //Generates the grid in order:

        //1 The tiles
        //2 The castles (before the enviromentalobjects to avoid not being able to place any due to enviromental objects occupying tiles
        //3 Finally the enviromental objects which CAN be placed specifically around the castles

        //Generates the tiles, without any non-grid logic
        new TileGenerator(_noiseData, _width, _length, _seed, transform, cloudPrefab, out _tiles, out _clouds);

    }

    private void SpawnMapAssets_OnServer()
    {
        //Generates a castle based on the grid
        new CastlePosGenerator(_tiles, _seed, playerCount, _width, _length, _castlePrefabId);

        //Adds supported enviromentel assets assigned within the unity inspector onto the previously created grid of tiles
        new EnviromentGenerator(_tiles, _seed);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnObject_ServerRPC(Vector3 spawnPos, Quaternion spawnRot, int objToSetId, bool activateImmediately, bool randomRot = false)
    {
        if (randomRot)
        {
            spawnRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        }

        NetworkObject spawnedObj = Instantiate(TileObjectPrefabManager.GetValue(objToSetId), spawnPos, spawnRot).GetComponent<NetworkObject>();

        spawnedObj.Spawn(true);

        SetupTileObject_ClientRPC(spawnPos.ToVector2(), spawnedObj.NetworkObjectId, activateImmediately);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetupTileObject_ClientRPC(Vector2 tilePos, ulong networkObjectId, bool activateImmediately)
    {
        if (TryGetTileByPos(tilePos, out TileBase tile))
        {
            tile.GetComponent<TileBase>().SetObject(networkObjectId, activateImmediately);
        }
    }


    public static bool TryGetTileByPos(Vector2 tilePos, out TileBase tile)
    {
        if (_tiles.TryGetValue(tilePos, out tile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool DoesCloudExist(Vector2 tilePos)
    {
        if (_clouds.TryGetValue(tilePos, out _))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public static void DestroyCloud(Vector2 tilePos)
    {
        Destroy(_clouds[tilePos]);

        _clouds.Remove(tilePos);
    }



#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private bool cloudsToggled;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cloudsToggled == false)
        {
            cloudsToggled = true;

            foreach (KeyValuePair<Vector2, TileBase> tile in _tiles)
            {
                tile.Value.gameObject.SetActive(true);
            }
            //StartCoroutine(FancyClouds());
        }
    }

    public float cloudSpeed = 0.05f;

    private IEnumerator FancyClouds()
    {
        foreach (KeyValuePair<Vector2, TileBase> tile in _tiles)
        {
            tile.Value.gameObject.SetActive(true);

            yield return new WaitForSeconds(cloudSpeed);
        }
    }
#endif
}