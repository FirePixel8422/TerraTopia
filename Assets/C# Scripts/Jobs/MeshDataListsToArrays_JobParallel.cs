using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct MeshDataListsToArrays_JobParallel : IJobParallelFor
{
    [NoAlias][ReadOnly] public NativeList<float3> verticesList;
    [NoAlias][ReadOnly] public NativeList<int> trianglesList;


    [NativeDisableParallelForRestriction]
    [NoAlias][WriteOnly] public NativeArray<float3> verticesArray;

    [NativeDisableParallelForRestriction]
    [NoAlias][WriteOnly] public NativeArray<int> trianglesArray;


    [BurstCompile]
    public void Execute(int index)
    {
        int vOffset = index * 8;
        int tOffset = index * 12;

        // Copy 8 vertices per iteration
        for (int i = 0; i < 8; i++)
        {
            if (vOffset + i < verticesList.Length)
            {
                verticesArray[vOffset + i] = verticesList[vOffset + i];
            }
        }


        // Copy 12 triangles per iteration
        for (int i = 0; i < 12; i++)
        {
            if (tOffset + i < trianglesList.Length)
            {
                trianglesArray[tOffset + i] = trianglesList[tOffset + i];
            }
        }
    }
}