
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public struct Matrix4x4List
{
    private NativeHashSet<Matrix4x4> matrixList;

    public int count;


    public Matrix4x4List(int capacity, Allocator allocator)
    {
        matrixList = new NativeHashSet<Matrix4x4>(capacity, allocator);

        count = 0;
    }

    [BurstCompile]
    public void Add(Matrix4x4 matrix)
    {
        //use count before incrementing it as id.
        matrixList.Add(matrix);

        count += 1;
    }

    [BurstCompile]
    public void RemoveAt(Matrix4x4 matrix)
    {
        //use count before incrementing it as id.
        matrixList.Remove(matrix);

        count -= 1;
    }
}
