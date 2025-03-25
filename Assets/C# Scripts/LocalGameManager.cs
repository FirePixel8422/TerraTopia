using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalGameManager
{
    public static List<City> ownedCities = new List<City>();

    public static bool TileIsWithinBorder(Vector3 tilePos)
    {
        for(int i = 0; i < ownedCities.Count; i++)
        {
            if (ownedCities[i].BorderTilePositions.Contains(tilePos))
            {
                return true;
            }
        }
        return false;
    }
}
