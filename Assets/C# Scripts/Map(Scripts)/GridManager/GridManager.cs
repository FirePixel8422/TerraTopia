using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Burst;
using UnityEngine;
[BurstCompile]
public class GridManager : MonoBehaviour
{
    private static Dictionary<Vector2,GameObject> _tiles;

     
    [Header("Preset MapGeneration Values")]
    [Tooltip("If left at 0 it will generate a random seed")]
    [SerializeField] private int _seed;
    [SerializeField] private NoiseData _noiseData;
    [SerializeField] private GameObject _castlePrefab;
    //The dimensions of the to-be created grid.
    [Header("Dimensions")]
    [Tooltip("The X-axis")]
    [SerializeField] private int _width;
    [Tooltip("The Z-axis")]
    [SerializeField] private int _length;

    [Header("Player Values")]
    [SerializeField] private int playerCount;



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
        new TileGenerator(_noiseData, _width, _length, _seed, transform, out _tiles);

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

}









