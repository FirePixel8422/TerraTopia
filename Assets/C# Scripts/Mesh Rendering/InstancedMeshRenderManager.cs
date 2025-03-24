using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


[BurstCompile]
public class InstancedMeshRenderManager : MonoBehaviour, ICustomUpdater
{
    private NativeList<NativeArray<Matrix4x4>> meshData;
    private NativeList<int> meshInstanceIds;

    [SerializeField] private Mesh[] instancedMeshes;
    [SerializeField] private Material[] instancedMaterials;






    private void Start()
    {
        CustomUpdaterManager.AddUpdater(this);
    }


    public bool RequireUpdate => true;

    [BurstCompile]
    public void OnUpdate()
    {
        RenderParams renderParams = new RenderParams()
        {
            layer = 0,
            camera = Camera.main,
            shadowCastingMode = ShadowCastingMode.On,
            receiveShadows = true,
            matProps = new MaterialPropertyBlock()
            {
                
            }
        };



        //for (int i = 0; i < instancedMeshRenderData.Length; i++)
        //{
        //    Graphics.RenderMeshInstanced(in renderParams, meshRenderData[i].meshId, 0, meshRenderData[i].materialId, meshRenderData[i].matrix);
        //}
    }
}
