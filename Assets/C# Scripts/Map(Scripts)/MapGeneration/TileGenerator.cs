using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//This struct is used to generate a grid within its constructor.
public struct TileGenerator 
{
    public TileGenerator(NoiseData noiseData, int width, int length, int seed, out Dictionary<Vector2, GameObject> tilesToReturn)
    {
        tilesToReturn = new Dictionary<Vector2, GameObject>();

        //Generates data for each tile
        var tiles = noiseData.GetTiles(seed, width, length);

        //Instantiates every tile with before-achieved the tile data
        for (int i = 0; i < tiles.Length; i++)
        {
            tilesToReturn.Add(tiles[i].Item2, (Object.Instantiate(tiles[i].Item1, new Vector3(tiles[i].Item2.x, 0, tiles[i].Item2.y), Quaternion.identity)));

            if (tilesToReturn.Last().Value.TryGetComponent(out TileBase tile))
            {
                tile._perlinHeight = tiles[i].Item3;
            }
        }
    }
}
