using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Rendering;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using System;




/// <summary>
/// 将整个世界分块，多线程地更新世界状态
/// </summary>
public class UpdateWorldSystem : JobComponentSystem
{
    public struct UpdateArea : IComponentData
    {
        public IntRect areaRect;
    }

    public struct UpdateAreaJob : IJobProcessComponentData<UpdateArea>
    {
        public void Execute(ref UpdateArea data)
        {
            var charMap = CellAutoWorld.instance.CurrentMap;
            for (int x = data.areaRect.left; x <= data.areaRect.right; x++)
            {
                for (int y = data.areaRect.down; y <= data.areaRect.top; y++)
                {
                    CellAutoWorld.instance.NextMap.map[x, y] = CellAutoWorld.instance.rule.GetNewState(x, y);
                }
            }

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new UpdateAreaJob();
        return job.Schedule(this,1,inputDeps);
        
    }
}

/// <summary>
/// 只做一件事，将World的Generation加1，以此来改变CurrentMap
/// </summary>
[UpdateAfter(typeof(UpdateWorldSystem))]
public class AddGenerationSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CellAutoWorld.instance.currentGeneration++;
        return base.OnUpdate(inputDeps);
    }
}


/// <summary>
/// 首先根据地图，记录每一种格子的实例的位置数组
/// 遍历位置数组，按格子种类（MeshInstanceRenderer）分组，每一组内根据位置数组设置其TransformMatrix
/// 如果该种格子的Entity不足，则增加Entity
/// </summary>
[UpdateAfter(typeof(AddGenerationSystem))]
public class UpdateMatrixSystem : JobComponentSystem
{
    public ComponentGroup gridgroup;
    ForEachComponentGroupFilter RendererFilter;
    List<MeshInstanceRenderer> renderers = new List<MeshInstanceRenderer>();

    public Dictionary<char, List<IntVec2D>> statePositionListDic;

    public struct charFlag : IComponentData
    {
        public char state;
        public int filterIndex;
    }

    protected override void OnCreateManager(int capacity)
    {
        base.OnCreateManager(capacity);
        gridgroup = GetComponentGroup(typeof(TransformMatrix), typeof(MeshInstanceRenderer));
        foreach (var cr in CellAutoWorld.instance.rule.charRenderer)
        {
            renderers.Add(cr.Value);
        }
        RendererFilter = gridgroup.CreateForEachFilter(renderers);
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {


        for(int i=0;i<RendererFilter.Length;i++)
        {
            ComponentDataArray<TransformMatrix> matrixArray= gridgroup.GetComponentDataArray<TransformMatrix>(RendererFilter, i);
        }

        return base.OnUpdate(inputDeps);
    }
}



public class CellAutoRenderer : MonoBehaviour {
    


    private static CellAutoRenderer _instance;
    public static CellAutoRenderer instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }
}
