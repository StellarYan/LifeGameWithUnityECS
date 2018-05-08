﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Rendering;

public abstract class CellAutoRule : MonoBehaviour
{
    public Dictionary<char, MeshInstanceRenderer> charRenderer;

    public abstract char GetNewState(int x, int y);
}




public class CellAutoWorld : MonoBehaviour {
    public int length;
    public int height;
    public int currentGeneration;
    public CellAutoRule rule;

    private char[,] oddGenerationMap;
    private char[,] evenGenerationMap;
    public char[,] CurrentMap
    {
        get { return (currentGeneration % 2 == 0) ? evenGenerationMap : oddGenerationMap; }
    }

    public char[,] NextMap
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
        oddGenerationMap = new char[length, height];
        evenGenerationMap = new char[length, height];
        
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }


}