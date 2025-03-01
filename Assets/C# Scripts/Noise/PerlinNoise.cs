using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Noise/Perlin")]

[BurstCompile]
public class PerlinNoise : NoiseData
{
    [SerializeField] private List<float> debugPerlinValues;

    [BurstCompile]
    public override Tuple<NoiseTileData, Vector2, float>[] GetTiles(int seed, int width, int length)
    {
        debugPerlinValues = new List<float>();
        _width = width;
        _length = length;
        var tiles = new Tuple<NoiseTileData, Vector2, float>[_width * _length];
        var iterator = 0;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _length; y++)
            {
                var pv = CalculateNoise(x, y, seed);
                if (_shouldDebug)
                {
                    debugPerlinValues.Add(pv);
                }

                //Tile, Position, Perlin
                tiles[iterator] = new Tuple<NoiseTileData, Vector2, float>(SelectTile(pv), new Vector2(x, y), pv);
                iterator++;
            }
        }

        return tiles;
    }


    [BurstCompile]
    protected override NoiseTileData SelectTile(float factor)
    {
        for (int i = 0; i < _possibleTiles.Length; i++)
        {
            if (factor < _possibleTiles[i].weight)
            {
                return _possibleTiles[i];
            }
        }
        return new NoiseTileData();
    }


    [BurstCompile]
    protected override float CalculateNoise(float posX, float posY, int seed)
    {
        float amplitude = 1.0f;
        float frequency = _scale;
        float noiseValue = 0.0f;
        float maxValue = 0.0f;
        var prng = new System.Random(seed);
        float offsetX = (float)prng.NextDouble() * 100000;
        float offsetY = (float)prng.NextDouble() * 100000;

        for (int i = 0; i < _octaves; i++)
        {
            float xCoord = (posX + offsetX) * frequency;
            float yCoord = (posY + offsetY) * frequency;

            noiseValue += Mathf.PerlinNoise(xCoord, yCoord) * amplitude;

            maxValue += amplitude;

            amplitude *= _persistence;
            frequency *= _lacunarity;
        }
        return noiseValue / maxValue;
    }
}
