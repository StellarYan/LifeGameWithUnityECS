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
    public int left;
    public int right;
    public int up;
    public int down;
}


//更新矩形区域的格子
public struct UpdateAreaJob : IJobProcessComponentData<UpdateArea>
{
    public void Execute(ref UpdateArea data)
    {
        var charMap = CellAutoWorld.instance.CurrentMap;
        for(int x=data.left;x<=data.right;x++)
        {
            for(int y=data.down;y<=data.up;y++)
            {
                CellAutoWorld.instance.NextMap[x,y] =CellAutoWorld.instance.rule.GetNewState(x, y);
            }
        }
        
    }
}

public class UpdateWorldSystem : JobComponentSystem
{
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new UpdateAreaJob();
        return job.Schedule(this,inputDeps);
        
    }
}




public class RenderGridSys : JobComponentSystem
{
    public ComponentGroup gridgroup;
    public Dictionary<char, SpriteInstanceRenderer> renderers;

    





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
