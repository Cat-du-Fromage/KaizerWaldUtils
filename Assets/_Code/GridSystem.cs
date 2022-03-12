using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;
/*
public class GridSystem : MonoBehaviour, IGridSystem
{
    public TerrainData mapDataForDebug;
    public TerrainData MapData { get; set; }
    public int2 MapBounds { get; set; }

    public T1[] RequestGrid<T1, T2>(T2 gridType) where T1 : struct where T2 : Enum
    {
        throw new NotImplementedException();
    }

    [SerializeField] private GridHandlerOne gridOne;
    [SerializeField] private ChunkedGridHandlerTwo chunkedGridTwo;
    
    private void Awake()
    {
        gridOne ??= FindObjectOfType<GridHandlerOne>();
        chunkedGridTwo ??= FindObjectOfType<ChunkedGridHandlerTwo>();
        
        MapData = mapDataForDebug = FindObjectOfType<Terrain>().terrainData;
        this.AsInterface<IGridSystem>()?.InitializeAllGrids();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
*/