using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct NoiseTileData
{
    //The GameObject assigned to this tile
    public GameObject tileGO;

    //Weight will decide when this tile is chosen, if the weight is > the randomly picked weight it will be skipped over.
    public float weight;

    public float constHeight;

}

[BurstCompile]
public abstract class NoiseData : ScriptableObject
{
    //The dimensions of the to-be created grid.
    //The dimensions will be assigned upon use
    protected int _width;
    protected int _length;

    [Header("Noise variables")]
    //The amount of layers of noise which will be layered
    [SerializeField] protected int _octaves;

    //The scale of the noise
    [Tooltip("Lower scale = more smoothness")]
    [SerializeField] protected float _scale;

    //number that determines how much detail is added or removed at each octave
    [SerializeField] protected float _lacunarity;

    //number that determines how much each octave contributes to the overall shape.
    [SerializeField] protected float _persistence;

    //Base resolution is 1, in which 1 tile represents the value of base tile(0, 0.5, 0.6) and such.
    //A higher resolution such as 2 will have 1 tile represent the combined values of x * x tiles around the selected position divided by the amount of tiles.
    [SerializeField] protected int _resolution;

    [Header("Tile variables")]
    
    //The multiplier for the height of the tile upon instantiation
    [SerializeField] public float tileHeightMultiplier = 1;

    //The possible tiles which will be picked within the SelectTile() method
    [Tooltip("MAKE SURE THAT: The array is sorted from low to high, otherwise No/The wrong tile is picked")]
    [SerializeField] protected NoiseTileData[] _possibleTiles;


    [Header("DebugValues")]
    //Determines whether debug methods are called or not
    [SerializeField] protected bool _shouldDebug;



    //Returns a full list of Tiles picked with the SelectTile() method to then be used by a generator class

    [BurstCompile] public abstract Tuple<NoiseTileData, Vector2, float>[] GetTiles(int seed, int width, int length);

    //Returns an Gameobject chosen on this class's derivative's choice.
    //such as in perlin in which the selected tile will be returned by the usage of weights and their corresponding perlin values
    //
    //Factor will be the deciding factor with which the tile will be selected with

    [BurstCompile] protected abstract NoiseTileData SelectTile(float factor);

    //Calculates noise (Type of which is determined within derivatives of this class)

    [BurstCompile] protected virtual float CalculateNoise(float worldPosX, float worldPosZ, int seed) { return 0f; }
}



