using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class NetworkPrefabsHandler : MonoBehaviour
{
    [SerializeField] private NetworkPrefabsList[] networkPrefabsList;

    [SerializeField] private UnitTribeListSO[] tribePrefabsData;


    [SerializeField] private List<string> prefabsStrings;



    private void Start()
    {
        HashSet<GameObject> filterdPrefabSet = GetFilterdPrefabList();
        SetupNetworkPrefabs(filterdPrefabSet);
    }


    private HashSet<GameObject> GetFilterdPrefabList()
    {
        HashSet<GameObject> filterdPrefabSet = new HashSet<GameObject>();

        for (int i = 0; i < networkPrefabsList.Length; i++)
        {
            int prefabCount = networkPrefabsList[i].PrefabList.Count;

            for (int i2 = 0; i2 < prefabCount; i2++)
            {
                filterdPrefabSet.Add(networkPrefabsList[i].PrefabList[i2].Prefab);
            }
        }


        for (int i = 0; i < tribePrefabsData.Length; i++)
        {
            int prefabCount = tribePrefabsData[i].unitSpawnData.Length;

            for (int i2 = 0; i2 < prefabCount; i2++)
            {
                filterdPrefabSet.Add(tribePrefabsData[i].unitSpawnData[i2].body.gameObject);
                filterdPrefabSet.Add(tribePrefabsData[i].unitSpawnData[i2].head.gameObject);
            }
        }

        return filterdPrefabSet;
    }

    private void SetupNetworkPrefabs(HashSet<GameObject> filterdPrefabSet)
    {
        foreach (GameObject prefab in filterdPrefabSet)
        {
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
        }

        Destroy(this);
    }

#if UNITY_EDITOR
    private void Update()
    {
        prefabsStrings.Clear();

        for (int i = 0; i < NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists.Count; i++)
        {
            for (int i2 = 0; i2 < NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists.Count; i2++)
            {
                prefabsStrings.Add(NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists[i].PrefabList[i2].Prefab.name);
            }
        }
    }
#endif
}