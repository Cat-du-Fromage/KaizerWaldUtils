using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class GridHandlerOne : MonoBehaviour, IGridHandler<int, GenericGrid<int>>
{
    public GenericGrid<int> Grid { get; private set; }
    void Start()
    {
        
    }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericGrid<int>(terrainBounds, 1);
        Debug.Log($"Simple Recieved {terrainBounds}");
    }
}
