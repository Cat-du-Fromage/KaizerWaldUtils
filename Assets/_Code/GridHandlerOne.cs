using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;
/*
public class GridHandlerOne : MonoBehaviour, IGridHandler<int, GenericGrid<int>>
{
    public bool EnableDebug;
    public GenericGrid<int> Grid { get; private set; }

    public IGridSystem GridSystem { get; set; }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericGrid<int>(terrainBounds, 1);
        Debug.Log($"Simple Recieved {terrainBounds}");
    }

    private void OnDrawGizmos()
    {
        if (!EnableDebug || Grid.GridArray.IsNullOrEmpty()) return;
        DebugCell();
    }

    private void DebugCell()
    {
        Gizmos.color = Color.green;
        Vector3 bounds = new Vector3(1, 0, 1);
        for (int i = 0; i < Grid.GridArray.Length; i++)
        {
            Vector3 centerCell = Grid.GetCellCenter(i);
            Gizmos.DrawWireCube(centerCell, bounds);
        }
    }
}
*/