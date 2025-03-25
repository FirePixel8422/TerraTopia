using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[BurstCompile]
public class InstancedMeshRenderManager : MonoBehaviour, ICustomUpdater
{
    public static InstancedMeshRenderManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    private NativeHashSet<Matrix4x4>[] meshMatrixData;
    private NativeArray<int> cMeshInstancesCounts;

    [SerializeField] private Mesh[] instancedMeshes;
    [SerializeField] private Material[] instancedMaterials;






    [BurstCompile]
    private void Start()
    {
        //add the customUpdate interface on this script to the update list in CustomUpdaterManager
        CustomUpdaterManager.AddUpdater(this);

    }

    [BurstCompile]
    private void SetupMatrixSets()
    {
        int meshInstancesCount = instancedMeshes.Length;
        int mapSizeSquared = MatchManager.settings.mapSize * MatchManager.settings.mapSize;

        meshMatrixData = new NativeHashSet<Matrix4x4>[meshInstancesCount];

        for (int i = 0; i < meshInstancesCount; i++)
        {
            meshMatrixData[i] = new NativeHashSet<Matrix4x4>(mapSizeSquared, Allocator.Persistent);
        }

        cMeshInstancesCounts = new NativeArray<int>(meshInstancesCount, Allocator.Persistent);
    }



    /// <summary>
    /// Add Mesh Matrix from InstancedMeshRenderSystem
    /// </summary>
    [BurstCompile]
    public void AddInstancedMesh(MeshInstancedRenderData meshData)
    {
        //add new matrix to set
        meshMatrixData[meshData.instanceId].Add(meshData.matrix);

        cMeshInstancesCounts[meshData.instanceId] += 1;
    }


    /// <summary>
    /// Update Mesh Matrix from InstancedMeshRenderSystem
    /// </summary>
    [BurstCompile]
    public void UpdateInstancedMesh(MeshInstancedRenderData meshData, Matrix4x4 oldMatrix)
    {
        //remove old matrix from set
        meshMatrixData[meshData.instanceId].Remove(oldMatrix);

        //add new one back
        meshMatrixData[meshData.instanceId].Add(meshData.matrix);
    }


    /// <summary>
    /// Remove Mesh Matrix from InstancedMeshRenderSystem
    /// </summary>
    [BurstCompile]
    public void RemoveInstancedMesh(MeshInstancedRenderData meshData)
    {
        //remove matrix from set
        meshMatrixData[meshData.instanceId].Remove(meshData.matrix);

        cMeshInstancesCounts[meshData.instanceId] -= 1;
    }


    public bool RequireUpdate => true;

    [BurstCompile]
    public void OnUpdate()
    {
        int instanceCount = meshMatrixData.Length;


        /*
        //MaterialPropertyBlock d = new MaterialPropertyBlock();
        //d.SetTexture("_MainTex", new Texture2DArray(1,1,1, TextureFormat.ARGB4444, false));

        JobHandle jobHandle = new JobHandle();

        for (int instanceId = 0; instanceId < instanceCount; instanceId++)
        {
            int meshCount = cMeshInstancesCounts[instanceId];

            ConvertHashSetToArrayJob convertHashSetToArrayJob = new ConvertHashSetToArrayJob
            {
                matricesSet = meshMatrixData[instanceId],
                matricesArray = new NativeArray<Matrix4x4>(meshCount, Allocator.TempJob),
                itemCount = meshCount
            };

            //schedule and combine all jobs
            jobHandle = JobHandle.CombineDependencies(convertHashSetToArrayJob.Schedule(), jobHandle);
        }

        //complete all jobs
        jobHandle.Complete();
        */


        for (int instanceId = 0; instanceId < instanceCount; instanceId++)
        {
            int meshCount = cMeshInstancesCounts[instanceId];

            Graphics.DrawMeshInstanced(instancedMeshes[instanceId], 0, instancedMaterials[instanceId], meshMatrixData[instanceId].ToNativeArray(Allocator.Temp).ToArray(), meshCount);
        }
    }
}


[BurstCompile]
public struct ConvertHashSetToArrayJob : IJob
{
    [NoAlias][ReadOnly] public NativeHashSet<Matrix4x4> matricesSet;
    [NoAlias][WriteOnly] public NativeArray<Matrix4x4> matricesArray;

    public int itemCount;


    [BurstCompile]
    public void Execute()
    {
        int index = 0;
        foreach (var item in matricesSet)
        {
            matricesArray[index] = item;
            index++;
        }
    }
}