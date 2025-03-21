using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public interface IBuildable 
{
    public List<Building> AvailableBuildings();

    public static List<Building> CombineLists(List<Building> l1, List<Building> l2)
    {
        //The list to return
        List<Building> rl = new List<Building>();

        foreach (Building b in l1) { rl.Add(b); }
        //Combine the two lists
        for (int i = 0; i < l2.Count; i++) 
        {
            if (!l1.Contains(l2[i])) { rl.Add(l2[i]); }
        }

        return rl;
    }
}
