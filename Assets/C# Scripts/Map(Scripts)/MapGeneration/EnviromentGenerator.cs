using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EnviromentGenerator 
{
    public EnviromentGenerator(List<GameObject> tiles, int seed)
    {
        System.Random prng = new System.Random(seed);
        foreach (var tile in tiles)
        {
            if (tile.TryGetComponent(out TileBase selectedTile))
            {
                if (CanChooseEnviromentalObject(selectedTile, out EnviromentalItemData enviromentalObject))
                {
                    selectedTile.AssignObject(enviromentalObject);
                }
            }
        }

        bool CanChooseEnviromentalObject(TileBase tile, out EnviromentalItemData enviromentalObject)
        {
            var selectedWeight = prng.Next(0, 101);
            for (int i = 0; i < tile._possibleEnviromentalObjects.Count; i++)
            {
                if(selectedWeight <= tile._possibleEnviromentalObjects[i].weight)
                {
                    enviromentalObject = tile._possibleEnviromentalObjects[i];
                    return true;
                }
            }
            enviromentalObject = new EnviromentalItemData();
            return false;
        }
    }


}
