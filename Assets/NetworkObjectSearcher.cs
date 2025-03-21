using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSearcher : MonoBehaviour
{
    public ulong toSearchHash;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var networkObjects = Resources.FindObjectsOfTypeAll<NetworkObject>();
            foreach (var networkObject in networkObjects)
            {
                if (networkObject.PrefabIdHash == toSearchHash)
                {
                    Debug.Log("Found object with hash: " + toSearchHash);
                    Debug.Log("Object name: " + networkObject.name);
                    return;
                }
            }
            Debug.Log("Object with hash: " + toSearchHash + " not found");
        }
    }
}
