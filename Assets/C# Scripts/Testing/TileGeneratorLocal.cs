using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

//This struct is used to generate a grid within its constructor.
[BurstCompile]

public struct TileGeneratorLocal
{

    public TileGeneratorLocal(NoiseData noiseData, int width, int length, int seed, Transform parent, out Dictionary<Vector2, TileBase> tilesToReturn)
    {
        tilesToReturn = new Dictionary<Vector2, TileBase>();

        var tileHeightMultiplier = noiseData.tileHeightMultiplier;
        var tileHeight = 0f;

        //Generates data for each tile
        var tiles = noiseData.GetTiles(seed, width, length);

        //Instantiates every tile with before-achieved the tile data
        for (int i = 0; i < tiles.Length; i++)
        {
            tileHeight = tiles[i].Item1.constHeight == 0 ?  tiles[i].Item3 * tileHeightMultiplier : tiles[i].Item1.constHeight;

            GameObject tileObj = Object.Instantiate(tiles[i].Item1.tileGO, new Vector3(tiles[i].Item2.x, tileHeight, tiles[i].Item2.y), Quaternion.identity, parent);
            tileObj.SetActive(true);

            TileBase tileBase = tileObj.GetComponent<TileBase>();
            tileBase.ownedByPlayerTeamId = -1;

            tilesToReturn.Add(tiles[i].Item2, tileBase);

            if (tilesToReturn.Last().Value.TryGetComponent(out TileBase tile))
            {
                tile._perlinHeight = tiles[i].Item3;
            }
        }
    }
}
