using System.Collections.Generic;
using Unity.Burst;
using Unity.Netcode;
using UnityEngine;



[BurstCompile]
public class GridManager : NetworkBehaviour
{
    private static Dictionary<Vector2, GameObject> _tiles;
    private static Dictionary<Vector2, GameObject> _clouds;


    [Header("Preset MapGeneration Values")]
    [Tooltip("If left at 0 it will generate a random seed")]
    [SerializeField] private int _seed;
    [SerializeField] private NoiseData _noiseData;
    [SerializeField] private GameObject _castlePrefab;
    [SerializeField] private GameObject cloudPrefab;


    //The dimensions of the to-be created grid.
    [Header("Dimensions")]
    [Tooltip("The X-axis")]
    [SerializeField] private int _width;
    [Tooltip("The Z-axis")]
    [SerializeField] private int _length;

    [Header("Player Values")]
    [SerializeField] private int playerCount;



    //public override void OnNetworkSpawn()
    //{
    //    GenerateGrid(playerCount);
    //}

    private void Start()
    {
        GenerateGrid(playerCount);
    }

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

        //Generates a castle based on the grid
        new CastlePosGenerator(_tiles, _seed, playerCount, _width, _length, _castlePrefab);

        //Adds supported enviromentel assets assigned within the unity inspector onto the previously created grid of tiles
        new EnviromentGenerator(_tiles, _seed);
    }


    public static bool TryGetTileByPos(Vector2 tilePos, out GameObject tile)
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

            foreach (KeyValuePair<Vector2, GameObject> cloud in _tiles)
            {
                cloud.Value.SetActive(true);
            }
        }
    }
#endif
}