using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WheatTile : TileObject
{
    [Tooltip("An int between 1 and 100 that is added to growth. soilFertility represents how much this crop grows after a cycle (all player turns is 1 cycle, crop grows every 100 fertility)")]
    public int soilFertility;

    private int maxGrowth;
    private int growth;

    public bool FullyGrown => growth >= maxGrowth;




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TurnManager.OnCycleStarted += (_) => CycleStarted();
        }
    }

    private void CycleStarted()
    {
        //grow if crop isnt fully grown already
        if (growth < maxGrowth)
        {
            growth += soilFertility;
        }
    }


    [ClientRpc(RequireOwnership = false)]
    public void HarvestCrop_ClientRPC()
    {
        if (IsServer)
        {
            //set growth to -100 to delay the growth of the next crop
            growth = -100;
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        if (IsServer)
        {
            TurnManager.OnCycleStarted -= (_) => CycleStarted();
        }
    }
}
