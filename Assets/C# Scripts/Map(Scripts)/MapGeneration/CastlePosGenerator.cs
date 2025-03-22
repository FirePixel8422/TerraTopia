using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct CastlePosGenerator
{
    public CastlePosGenerator(Dictionary<Vector2, TileBase> tiles, int seed, int playerCount, int width, int length, int castlePrefabId)
    {
        System.Random rng = new System.Random(seed);
        Dictionary<int, Vector2Int> playerCastlePositions = new Dictionary<int, Vector2Int>();
        Dictionary<Vector2Int, GameObject> tileMap = new Dictionary<Vector2Int, GameObject>();


        foreach (TileBase tile in tiles.Values)
        {
            Vector2Int position = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z));

            TileBase tileBase = tile;
            if (tileBase != null && tileBase.canHoldCastle)
            {
                tileMap[position] = tile.gameObject;
            }
        }

        List<Vector2Int> validTiles = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1,  0),                   new Vector2Int(1,  0),
            new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1)
        };

        foreach (var tile in tileMap.Keys)
        {
            int surroundingTileCount = 0;
            foreach (var dir in directions)
            {
                if (tileMap.ContainsKey(tile + dir))
                    surroundingTileCount++;
            }
            if (surroundingTileCount >= 8)
                validTiles.Add(tile);
        }

        List<Vector2Int> chosenPositions = new List<Vector2Int>(playerCount);


        if (validTiles.Count > 0)
            chosenPositions.Add(validTiles[rng.Next(validTiles.Count)]);

        while (chosenPositions.Count < playerCount && validTiles.Count > 0)
        {
            float bestDistance = -1f;
            Vector2Int bestCandidate = validTiles[0];

            foreach (Vector2Int candidate in validTiles)
            {
                float minDistance = float.MaxValue;
                foreach (Vector2Int chosen in chosenPositions)
                {
                    float distance = Vector2.Distance(candidate, chosen);
                    if (distance < minDistance)
                        minDistance = distance;
                }

                if (minDistance > bestDistance)
                {
                    bestDistance = minDistance;
                    bestCandidate = candidate;
                }
            }

            chosenPositions.Add(bestCandidate);
            validTiles.RemoveAll(t => Vector2.Distance(t, bestCandidate) < 2f);
        }

        ulong castleId = 0;
        int castleCount = chosenPositions.Count;

        for (int i = 0; i < castleCount; i++)
        {
            int r = rng.Next(0, chosenPositions.Count);

            Vector2Int pos = chosenPositions[r];
            chosenPositions.RemoveAt(r);

            var castleTile = tiles.FirstOrDefault(a => a.Value.transform.position == new Vector3(pos.x, a.Value.transform.position.y, pos.y));
            if (castleTile.Value.TryGetComponent(out TileBase tb))
            {
                tb.AssignObject(castlePrefabId, true, castleId);
            }

            castleId += 1;
        }
    }
}
