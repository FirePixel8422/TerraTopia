using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedMeshRenderer : MonoBehaviour
{
    public MeshInstancedRenderData meshData;


    private void Start()
    {
        InstancedMeshRenderManager.Instance.AddInstancedMesh(meshData);
    }


    private void OnDestroy()
    {
        InstancedMeshRenderManager.Instance.RemoveInstancedMesh(meshData);
    }
}
