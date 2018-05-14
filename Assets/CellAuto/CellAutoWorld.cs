using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;
using Unity.Entities;
using Unity.Transforms;
using toinfiniityandbeyond.Rendering2D;
using System;


public abstract class CellAutoRule : MonoBehaviour
{
    public Dictionary<char, MeshInstanceRenderer> charRenderer;

    public abstract char GetNewState(int x, int y);
}

public class BasicRule : CellAutoRule
{
    public BasicRule()
    {
        charRenderer = new Dictionary<char, MeshInstanceRenderer>();
    }

    public override char GetNewState(int x, int y)
    {
        return '0';
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
}


public class CellAutoWorld  {

    public struct CEllAutoMap
    {
        public char[,] map;
        public Dictionary<char, int> charCountDic;
    }


    public IntRect WorldRect;
    public int currentGeneration;
    public CellAutoRule rule;


    private CEllAutoMap oddGenerationMap;
    private CEllAutoMap evenGenerationMap;
    public CEllAutoMap CurrentMap
    {
        get { return (currentGeneration % 2 == 0) ? evenGenerationMap : oddGenerationMap; }
    }

    public CEllAutoMap NextMap
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
            CreateArchetype(typeof(GridINfo), typeof(TransformMatrix), typeof(MeshInstanceRenderer));
        for(int x= WorldRect.left; x<WorldRect.right;x++)
        {
            for(int y=WorldRect.down;y<WorldRect.top;y++)
            {
                var grid = World.Active.GetOrCreateManager<EntityManager>().CreateEntity(gridtype);
                World.Active.GetOrCreateManager<EntityManager>().SetComponentData(grid, new GridINfo() { x = x, y = y });
                World.Active.GetOrCreateManager<EntityManager>().SetComponentData(
                    grid, new TransformMatrix()
                    {
                        Value = Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.identity, Vector3.one)
                    }
                );
                World.Active.GetOrCreateManager<EntityManager>().SetSharedComponentData(grid, rule.charRenderer['0']);

            }
        }

    }


    public CellAutoWorld()
    {
        WorldRect = new IntRect() { left = 0, down = 0, top = 100, right = 100 };
        oddGenerationMap = new CEllAutoMap
        {
            map = new char[WorldRect.length, WorldRect.height],
            charCountDic =new Dictionary<char, int>()
        };
        evenGenerationMap = new CEllAutoMap
        {
            map = new char[WorldRect.length, WorldRect.height],
            charCountDic = new Dictionary<char, int>()
        };
        //renderers = new Dictionary<char, MeshInstanceRenderer>();
        BasicRule r = new BasicRule();
        rule = r;

        var tree = Resources.Load<GameObject>("Tree");
        Material mat= tree.GetComponent<MeshRenderer>().sharedMaterial;
        Mesh mesh = tree.GetComponent<MeshFilter>().sharedMesh;

        var render = new MeshInstanceRenderer() { castShadows = 0,material =mat,mesh=mesh  };
        rule.charRenderer.Add('0', render);

        InitArea();
        InitGrid();
        Debug.Log("World Init");





    }



    


}
