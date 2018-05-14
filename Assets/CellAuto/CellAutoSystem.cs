using System.Collections.Generic;
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
            return;
            var charMap = CellAutoWorld.instance.CurrentMap;
            for (int x = data.areaRect.left; x < data.areaRect.right; x++)
            {
                for (int y = data.areaRect.down; y < data.areaRect.top; y++)
                {
                    CellAutoWorld.instance.NextMap.map[x, y] = CellAutoWorld.instance.rule.GetNewState(x, y);
                }
            }
            

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







