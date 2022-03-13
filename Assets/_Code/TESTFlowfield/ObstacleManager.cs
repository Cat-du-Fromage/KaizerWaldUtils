using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.InputSystemExtension;

public class ObstacleManager : MonoBehaviour, IGridHandler<bool, GenericGrid<bool>>
{
    public IGridSystem GridSystem { get; set; }
    public GenericGrid<bool> Grid { get; private set; }

    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericGrid<bool>(terrainBounds, 2);
        Debug.Log($"ObstacleManager Init {Grid.GridArray.Length}");
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.wasReleasedThisFrame) return;
        Ray ray = Camera.main.ScreenPointToRay(GetMousePosition);
        ArrayPool<RaycastHit> arrayPool = ArrayPool<RaycastHit>.Shared;
        RaycastHit[] hits = arrayPool.Rent(1);
        if (Physics.RaycastNonAlloc(ray.origin, ray.direction, hits,math.INFINITY, 1<<8) != 0)
        {
            int currentGridIndex = hits[0].point.GetIndexFromPosition(GridSystem.MapBounds, 2);
            if(Grid.GetValue(currentGridIndex) == true) return;
            Grid.SetValue(currentGridIndex, true);
        }
    }

    private void OnDrawGizmos()
    {
        if(Grid == null) return;
        Vector3 cubeBounds = (Vector3.one * 2).SetAxis(Axis.Y, 0.5f);
        Gizmos.color = Color.red;
        for (int i = 0; i < Grid.GridArray.Length; i++)
        {
            if (Grid.GetValue(i) == false) continue;
            Gizmos.DrawCube(Grid.GetCellCenter(i), cubeBounds);
        }
    }
}
