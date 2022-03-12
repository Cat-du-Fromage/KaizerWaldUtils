using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class TestFlowField : MonoBehaviour, IGridHandler<bool, GenericGrid<bool>>
{
    private FlowField flowField;
    public GenericGrid<bool> Grid { get; private set; }

    public IGridSystem GridSystem { get; set; }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericGrid<bool>(terrainBounds, 2);
    }
}
