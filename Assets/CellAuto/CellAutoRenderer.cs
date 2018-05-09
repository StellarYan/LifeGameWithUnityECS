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
        Debug.Log("UpdateWorldSystem");
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
        Debug.Log("AddGenerationSystem ");
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
    List<MeshInstanceRenderer> renderers = new List<MeshInstanceRenderer>();

    ForEachComponentGroupFilter RendererFilter;
    public ComponentGroup gridgroup;
    public Dictionary<MeshInstanceRenderer, List<IntVec2D>> renderPositionListDic;
    EntityArchetype GridRenderEntity;


    public void Init()
    {
        Debug.Log("UpdateMatrixSystem init");
        renderPositionListDic = new Dictionary<MeshInstanceRenderer, List<IntVec2D>>();
        foreach (var cr in CellAutoWorld.instance.rule.charRenderer)
        {
            renderers.Add(cr.Value);
            renderPositionListDic.Add(cr.Value, new List<IntVec2D>());
        }
        BuildGroup();
        GridRenderEntity = EntityManager.CreateArchetype(typeof(TransformMatrix), typeof(MeshInstanceRenderer));
        
    }

    protected void AddEntityRender(MeshInstanceRenderer render,int count)
    {
        for(int i=0;i<count;i++)
        {
            var entity = EntityManager.CreateEntity(GridRenderEntity);
            EntityManager.SetSharedComponentData(entity, render);
        }
        
    }

    protected void BuildGroup()
    {
        gridgroup = GetComponentGroup(typeof(TransformMatrix), typeof(MeshInstanceRenderer));
        RendererFilter = gridgroup.CreateForEachFilter(renderers);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Debug.Log("UpdateMatrixSystem");
        for (int x= CellAutoWorld.instance.WorldRect.left;x< CellAutoWorld.instance.WorldRect.right;x++)
        {
            for (int y = CellAutoWorld.instance.WorldRect.down; y < CellAutoWorld.instance.WorldRect.top; y++)
            {
                var renderer = CellAutoWorld.instance.rule.charRenderer[CellAutoWorld.instance.CurrentMap.map[x, y]];
                renderPositionListDic[renderer].Add(
                    new IntVec2D { x =x, y = y });
            }
        }


        for(int i=0;i<RendererFilter.Length;i++)
        {
            var renderer = renderers[i];
            ComponentDataArray<TransformMatrix> matrixArray= gridgroup.GetComponentDataArray<TransformMatrix>(RendererFilter, i);
            List<IntVec2D> positionList = renderPositionListDic[renderer];
            if(matrixArray.Length< positionList.Count)
            {
                AddEntityRender(renderer, positionList.Count - matrixArray.Length + positionList.Count);
            }
            matrixArray = gridgroup.GetComponentDataArray<TransformMatrix>(RendererFilter, i);

            Vector3 standard = new Vector3(1, 1, 1);
            for (int m=0;m<matrixArray.Length;m++)
            {                
                IntVec2D? pos;
                if (m>=positionList.Count) pos = null;
                else pos = positionList[m];

                if (pos != null)
                {
                    matrixArray[m] = new TransformMatrix()
                    {
                        Value =
                        Matrix4x4.TRS(new Vector3(pos.Value.x, pos.Value.y, 0), new Quaternion(), standard)
                    };

                }
                
            }


        }
        

        return base.OnUpdate(inputDeps);
    }
}




