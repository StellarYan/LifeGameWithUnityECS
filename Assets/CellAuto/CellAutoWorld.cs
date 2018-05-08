using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;
using Unity.Entities;
using toinfiniityandbeyond.Rendering2D;

public abstract class CellAutoRule : MonoBehaviour
{
    public Dictionary<char, MeshInstanceRenderer> charRenderer;

    public abstract char GetNewState(int x, int y);
}

public struct IntVec2D
{
    public int x;
    public int y;
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





public class CellAutoWorld : MonoBehaviour {

    public struct CEllAutoMap
    {
        public char[,] map;
        public Dictionary<char, int> charCountDic;
    }


    public IntRect WorldRect;
    public int currentGeneration;
    public CellAutoRule rule;


    public Dictionary<char, SpriteInstanceRenderer> renderers;
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
        get { return _instance; }
    }


    void InitWorld()
    {
        
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

    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }


}
