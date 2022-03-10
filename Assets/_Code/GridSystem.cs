using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class GridSystem : MonoBehaviour, IGridSystem
{
    public TerrainData mapDataForDebug;

    public TerrainData MapData { get; private set; }

    [SerializeField] private GridHandlerOne gridOne;
    [SerializeField] private GridHandlerTwo gridTwo;
    
    private void Awake()
    {
        gridOne ??= FindObjectOfType<GridHandlerOne>();
        gridTwo ??= FindObjectOfType<GridHandlerTwo>();
        
        MapData = mapDataForDebug = FindObjectOfType<Terrain>().terrainData;
        this.AsInterface<IGridSystem>()?.InitializeAllGrids();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
