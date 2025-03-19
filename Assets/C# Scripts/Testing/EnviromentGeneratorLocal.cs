using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public struct EnviromentGeneratorLocal
{
    public EnviromentGeneratorLocal(Dictionary<Vector2, TileBase> tiles, int seed)
    {
        System.Random prng = new System.Random(seed);
        foreach (var tile in tiles)
        {
            TileBase selectedTile = tile.Value;

            if (CanChooseEnviromentalObject(selectedTile, out EnviromentalItemData enviromentalObject))
            {
                selectedTile.AssignObjectLocal(enviromentalObject, true);
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
