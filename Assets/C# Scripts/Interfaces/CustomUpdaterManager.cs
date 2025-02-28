using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public static class CustomUpdaterManager
{
    public static void Initialize()
    {
        updateStack = new List<ICustomUpdater>(GameSettings.updateManagerListPreSizeCapacity);

        NetworkManager.Singleton.StartCoroutine(UpdateLoop());
    }





    public static List<ICustomUpdater> updateStack;

    public static void AddUpdater(ICustomUpdater newEntry)
    {
        updateStack.Add(newEntry);
    }

    
    private static IEnumerator UpdateLoop()
    {
        while (true)
        {
            yield return null;

            Update_UpdateStack();
        }
    }


    private static void Update_UpdateStack()
    {
        for (int i = 0; i < updateStack.Count; i++)
        {
            if (updateStack[i].RequireUpdate)
            {
                updateStack[i].OnUpdate();
            }
        }
    }
}
