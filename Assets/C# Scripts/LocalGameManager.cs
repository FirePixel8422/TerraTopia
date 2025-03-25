using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalGameManager
{
    public static List<City> ownedCities = new List<City>();

    public static bool TileIsWithinBorder(Vector3 tilePos)
    {
        for (int i = 0; i < ownedCities.Count; i++)
        {
            foreach (var tile in ownedCities[i].BorderTilePositions)
            {
                if (tile == tilePos)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
