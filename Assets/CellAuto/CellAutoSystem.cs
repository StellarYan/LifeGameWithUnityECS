﻿using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using System;




/// <summary>
/// 将整个世界分块，多线程地更新世界状态
/// </summary>
public class UpdateWorldSystem : JobComponentSystem
{
    public struct UpdateAreaJob : IJobProcessComponentData<UpdateArea>
    {
        public void Execute(ref UpdateArea data)
        {
            var charMap = CellAutoWorld.instance.CurrentMap;
            for (int x = data.areaRect.left; x < data.areaRect.right; x++)
            {
                for (int y = data.areaRect.down; y < data.areaRect.top; y++)
                {
                    
                    CellAutoWorld.instance.NextMap[x, y] = CellAutoWorld.instance.rule.GetNewState(x, y);
                }
            }
            CellAutoWorld.instance.currentGeneration++;

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Debug.Log("UpdateWorldSystem Update");
        var s = CellAutoWorld.instance;
        var job = new UpdateAreaJob();
        return job.Schedule(this,1,inputDeps);

    }
}


/// <summary>
/// 首先根据地图，记录每一种格子的实例的位置数组
/// 遍历位置数组，按格子种类（MeshInstanceRenderer）分组，每一组内根据位置数组设置其TransformMatrix
/// 如果该种格子的Entity不足，则增加Entity
/// </summary>
//[UpdateAfter(typeof(UpdateWorldSystem))]
public class UpdateMatrixSystem : JobComponentSystem
{
    public struct UpdateMatrixJob : IJobProcessComponentData<GridINfo, TransformMatrix>
    {
        public void Execute(ref GridINfo data0, ref TransformMatrix data1)
        {

            if (CellAutoWorld.instance.CurrentMap[data0.x, data0.y] != data0.c)
            {

                data1.Value = Matrix4x4.identity;
            }
            else
            {

                data1.Value = data0.matrix;
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Debug.Log("UpdateWorldSystem Update");
        var job = new UpdateMatrixJob();
        return job.Schedule(this, 64, inputDeps);

    }
}








