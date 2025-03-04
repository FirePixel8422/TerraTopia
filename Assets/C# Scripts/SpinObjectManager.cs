//using System.Collections;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Netcode;
//using UnityEngine;

//public class SpinObjectManager : NetworkBehaviour, ICustomUpdater
//{
//    public static SpinObjectManager Instance;
//    private void Awake()
//    {
//        Instance = this;
//    }




//    private List<Transform> spinObjects = new List<Transform>();
//    private List<float> spinObjectsRot = new List<float>();

//    public float spinSpeed;



//    private void Start()
//    {
//        CustomUpdaterManager.AddUpdater(this);
//    }



//    [ServerRpc(RequireOwnership = false)]
//    public void Add_ServerRPC(ulong networkObjectId)
//    {
//        Add_ClientRPC(networkObjectId);
//    }

//    [ClientRpc(RequireOwnership = false)]
//    private void Add_ClientRPC(ulong networkObjectId)
//    {
//        Transform addedSpinObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId].transform;

//        spinObjects.Add(addedSpinObject);
//        spinObjectsRot.Add(addedSpinObject.rotation.y);
//    }



//    public bool RequireUpdate => spinObjectsRot.Count > 0;

//    [BurstCompile]
//    public void OnUpdate()
//    {
//        float addedRotation = spinSpeed * Time.deltaTime;

//        for (int i = 0; i < spinObjects.Count; i++)
//        {
//            spinObjectsRot[i] += addedRotation;

//            spinObjects[i].rotation = Quaternion.Euler(0, spinObjectsRot[i], 0);
//        }
//    }
//}
