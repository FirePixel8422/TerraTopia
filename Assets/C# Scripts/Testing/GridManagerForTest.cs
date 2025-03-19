using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;



[BurstCompile]
public class GridManagerForTest : MonoBehaviour
{
    public static GridManagerForTest Instance;

    private static Dictionary<Vector2, TileBase> _tiles = new Dictionary<Vector2, TileBase>();

    public int players;
    [Header("Preset MapGeneration Values")]
    private int _seed;
    [SerializeField] private NoiseData _noiseData;
    [SerializeField] private int _castlePrefabId;
    [SerializeField] private GameObject cloudPrefab;


    //The dimensions of the to-be created grid.
    [Header("Dimensions")]

    [Tooltip("The X-axis")]
    public int _width;
    [Tooltip("The Z-axis")]
    public int _length;

    public TileObjectLibrarySO tileObjectsData;

    private void Awake()
    {
        Instance = this;
    }


    [ContextMenu("Generate the map")]
    [BurstCompile]
    public void GenerateGrid()
    {
        foreach(var tiles in _tiles)
        {
            Destroy(tiles.Value.gameObject);
        }
        _tiles.Clear();
        _seed = MatchManager.settings.seed;
        if (_seed == 0)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
        }

        //Generates the grid in order:

        //1 The tiles
        //2 The castles (before the enviromentalobjects to avoid not being able to place any due to enviromental objects occupying tiles
        //3 Finally the enviromental objects which CAN be placed specifically around the castles

        //Generates the tiles, without any non-grid logic
        new TileGeneratorLocal(_noiseData, _width, _length, _seed, transform, out _tiles);

        //Generates a castle based on the grid
        new CastlePosGenerator(_tiles, _seed, players, _width, _length, _castlePrefabId);

        //Adds supported enviromentel assets assigned within the unity inspector onto the previously created grid of tiles
        new EnviromentGenerator(_tiles, _seed);
    }

    private void SpawnMapAssets_OnServer()
    {
        //Generates a castle based on the grid
        new CastlePosGenerator(_tiles, _seed, players, _width, _length, _castlePrefabId);

        //Adds supported enviromentel assets assigned within the unity inspector onto the previously created grid of tiles
        new EnviromentGeneratorLocal(_tiles, _seed);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnObject_ServerRPC(Vector3 spawnPos, Quaternion spawnRot, int objToSetId, bool activateImmediately, ulong ownerId, bool randomRot = false)
    {
        if (randomRot)
        {
            spawnRot = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        }

        NetworkObject spawnedObj = Instantiate(TileObjectPrefabManager.GetValue(objToSetId), spawnPos, spawnRot).GetComponent<NetworkObject>();

        spawnedObj.SpawnWithOwnership(ownerId, true);

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
        tilePos.y = (int)tilePos.y;
        tilePos.x = (int)tilePos.x;
        if (_tiles.TryGetValue(tilePos, out tile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool TryGetTileByPos(Vector2 tilePos, out GameObject tile)
    {
        tilePos.y = (int)tilePos.y;
        tilePos.x = (int)tilePos.x;
        if (_tiles.TryGetValue(tilePos, out TileBase tileBase))
        {
            tile = tileBase.gameObject;
            return true;
        }
        else
        {
            tile = null; 
            return false;
        }
    }



    public static void Discover3X3(Vector2 tilePos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 offsetPos = tilePos + new Vector2(x, y);

                if (_tiles.ContainsKey(offsetPos))
                {
                    _tiles[offsetPos].gameObject.SetActive(true);
                }
            }
        }
    }



#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private bool cloudsToggled;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && cloudsToggled == false)
    //    {
    //        cloudsToggled = true;

    //        foreach (KeyValuePair<Vector2, TileBase> tile in _tiles)
    //        {
    //            tile.Value.gameObject.SetActive(true);
    //        }
    //    }
    //}
#endif
}