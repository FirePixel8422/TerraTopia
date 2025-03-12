using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileObjectPrefabManager
{
    public static Dictionary<int, GameObject> tileObjectsDictionary = new Dictionary<int, GameObject>();


    public static void Add(GameObject obj)
    {
        tileObjectsDictionary.Add(tileObjectsDictionary.Count, obj);
    }

    public static GameObject GetValue(int id)
    {
        return tileObjectsDictionary[id];
    }
}