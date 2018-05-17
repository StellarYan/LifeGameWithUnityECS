using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using toinfiniityandbeyond.Rendering2D;
using System;


public abstract class CellAutoRule : MonoBehaviour
{
    public Dictionary<byte, MeshInstanceRenderer> charRenderer;

    public abstract byte GetNewState(int x, int y);
}

public class BasicRule : CellAutoRule
{
    public BasicRule()
    {
        charRenderer = new Dictionary<byte, MeshInstanceRenderer>();
    }

    public override byte GetNewState(int x, int y)
    {
        int Count=0;
        byte v = CellAutoWorld.instance.CurrentMap[x, y];
        for (int i=x-1;i<=x+1 ;i++)
        {
            for(int j=y-1;j<=y+1;j++)
            {
                if(i!=0 &&j!=0 && 
                    i>=0 && i< CellAutoWorld.instance.CurrentMap.GetLength(0) &&
                    j>=0 && j< CellAutoWorld.instance.CurrentMap.GetLength(1))
                {
                    if (CellAutoWorld.instance.CurrentMap[i,  j] == 1) Count++;
                }
            }
        }
        if (Count < 2 || Count > 3) v = 0;
        else if (Count == 3) v = 1;
        return v;
        
    }
}



public struct IntRect
{
    public int left;
    public int right;
    public int top;
    public int down;

    public int length
    {
        get { return right - left; }
    }
    public int height
    {
        get { return top - down; }
    }
}


public struct UpdateArea : IComponentData
{
    public IntRect areaRect;
}

public struct GridINfo : IComponentData
{
    public int x;
    public int y;
    public float4x4 matrix;
    public byte c;

}


public class CellAutoWorld  {




    public IntRect WorldRect;
    public int currentGeneration;
    public CellAutoRule rule;


    private byte[,] oddGenerationMap;
    private byte[,] evenGenerationMap;
    public byte[,] CurrentMap
    {
        get { return (currentGeneration % 2 == 0) ? evenGenerationMap : oddGenerationMap; }
    }

    public byte[,] NextMap
    {
        get { return (currentGeneration % 2 == 0) ? oddGenerationMap : evenGenerationMap; }
    }

    


    private static CellAutoWorld _instance;
    public static CellAutoWorld instance
    {
        get
        {
            if (_instance == null) _instance = new CellAutoWorld();
            return _instance;
        }
    }

    private void InitArea()
    {
        var area = World.Active.GetOrCreateManager<EntityManager>().CreateEntity(typeof(UpdateArea));
        World.Active.GetOrCreateManager<EntityManager>().SetComponentData(area, new UpdateArea()
        {
            areaRect = new IntRect()
            {
                left = 0,
                down = 0,
                top = 100,
                right = 100
            }
        }
        );
    }

    private void InitGrid()
    {
        var gridtype = World.Active.GetOrCreateManager<EntityManager>().
            CreateArchetype(typeof(GridINfo), typeof(TransformMatrix),typeof(MeshInstanceRenderer));
        foreach(var cr in rule.charRenderer)
        {
            for (int x = WorldRect.left; x < WorldRect.right; x++)
            {
                for (int y = WorldRect.down; y < WorldRect.top; y++)
                {
                    var grid = World.Active.GetOrCreateManager<EntityManager>().CreateEntity(gridtype);
                    float4x4 matrix = Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.identity, Vector3.one);
                    World.Active.GetOrCreateManager<EntityManager>().SetComponentData(grid, new GridINfo() { x = x, y = y, c = cr.Key, matrix = matrix });
                    World.Active.GetOrCreateManager<EntityManager>().SetComponentData(
                        grid, new TransformMatrix()
                        {
                            Value = matrix
                        }
                    );
                    World.Active.GetOrCreateManager<EntityManager>().SetSharedComponentData(grid, cr.Value);
                }
            }
        }

    }

    private void InitRenderer()
    {
        var tree = Resources.Load<GameObject>("Tree");
        Material mat = tree.GetComponent<MeshRenderer>().sharedMaterial;
        Mesh mesh = tree.GetComponent<MeshFilter>().sharedMesh;
        var render = new MeshInstanceRenderer() { castShadows = 0, material = mat, mesh = mesh };
        rule.charRenderer.Add(0, render);

        var tree1 = Resources.Load<GameObject>("Tree 1");
        mat = tree1.GetComponent<MeshRenderer>().sharedMaterial;
        mesh = tree1.GetComponent<MeshFilter>().sharedMesh;
        render = new MeshInstanceRenderer() { castShadows = 0, material = mat, mesh = mesh };
        rule.charRenderer.Add(1, render);
        
    }

    private void InitMap()
    {
        for(int x=0;x<CurrentMap.GetLength(0);x++)
        {
            for(int y=0;y<CurrentMap.GetLength(1);y++)
            {
                CurrentMap[x, y] = (byte)UnityEngine.Random.Range(0, 2);
                Debug.Log(CurrentMap[x, y]);
            }
        }
    }


    public CellAutoWorld()
    {
        WorldRect = new IntRect() { left = 0, down = 0, top = 100, right = 100 };
        oddGenerationMap = new byte[WorldRect.length, WorldRect.height];
        evenGenerationMap = new byte[WorldRect.length, WorldRect.height];

        BasicRule r = new BasicRule();
        rule = r;
        InitRenderer();
        InitArea();
        InitGrid();
        InitMap();
        Debug.Log("World Init");
    }



    


}
