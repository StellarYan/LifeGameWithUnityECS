using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using toinfiniityandbeyond.Rendering2D;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using System;



public struct UpdateArea : IComponentData
{
    public IntRect areaRect;
}

public class UpdateWorldSystem : JobComponentSystem
{
    public struct UpdateAreaJob : IJobProcessComponentData<UpdateArea>
    {
        public void Execute(ref UpdateArea data)
        {
            var charMap = CellAutoWorld.instance.CurrentMap;
            for (int x = data.areaRect.left; x <= data.areaRect.right; x++)
            {
                for (int y = data.areaRect.down; y <= data.areaRect.top; y++)
                {
                    CellAutoWorld.instance.NextMap[x, y] = CellAutoWorld.instance.rule.GetNewState(x, y);
                }
            }

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new UpdateAreaJob();
        return job.Schedule(this,256,inputDeps);
        
    }
}

[UpdateAfter(typeof(UpdateWorldSystem))]
public class BuildGridTypeChunkSystem : JobComponentSystem
{
    public struct BuildGrid : IComponentData
    {

    }

    public struct UpdateAreaJob : IJobProcessComponentData<BuildGrid>
    {
        public void Execute(ref BuildGrid data)
        {
            
            
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new UpdateAreaJob();
        return job.Schedule(this, 256, inputDeps);
    }
}

[UpdateAfter(typeof(BuildGridTypeChunkSystem))]
public class UpdateMatrixSystem : JobComponentSystem
{

}




public class RenderGridSys : JobComponentSystem
{
    public ComponentGroup gridgroup;
    public Dictionary<char, SpriteInstanceRenderer> renderers;
    public Dictionary<char, List<IntVec2D>> GridTypeChunk;

    protected override void OnCreateManager(int capacity)
    {
        base.OnCreateManager(capacity);
        gridgroup = GetComponentGroup(typeof(TransformMatrix), typeof(SpriteInstanceRenderer));
        
    }



    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
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
