using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public class DirectionManager : MonoBehaviour, IGridHandler<Vector3, GenericChunkedGrid<Vector3>>
{
    public bool DebugEnable;
    
    public IGridSystem GridSystem { get; set; }
    public GenericChunkedGrid<Vector3> Grid { get; private set; }
    
    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericChunkedGrid<Vector3>(terrainBounds, 16, 2);
    }

    private void Start()
    {
        Grid.CopyFrom(GridSystem.RequestGrid<Vector3,GridType>(GridType.FlowField));
/*
        for (int i = 0; i < Grid.ChunkDictionary[0].Length; i++)
        {
            Debug.Log(Grid.ChunkDictionary[0][i]);
        }
        */
    }

    private void OnDrawGizmos()
    {
        if (!DebugEnable || Grid.GridArray.IsNullOrEmpty()) return;

        for (int i = 0; i < Grid.GridArray.Length; i++)
        {
            Vector3 pos = Grid.GetCellCenter(i);
            KWUtils.Debug.DrawArrow.ForGizmo(pos, Grid.GridArray[i]);
        }
    }
}
