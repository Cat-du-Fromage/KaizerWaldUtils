using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static KWUtils.KWGrid;

public class DirectionManager : MonoBehaviour, IGridHandler<GridType,Vector3, GenericChunkedGrid<Vector3>>
{
    public bool DebugEnable;

    [SerializeField] private Transform Goal;
    private int goalIndex;
    
    private FlowField flowField;
    public IGridSystem<GridType> GridSystem { get; set; }
    public GenericChunkedGrid<Vector3> Grid { get; private set; }
    
    public void InitializeGrid(int2 terrainBounds)
    {
        Grid = new GenericChunkedGrid<Vector3>(terrainBounds, 16, 2);
    }

    private void Start()
    {
        goalIndex = GetIndexFromPosition(Goal.position.XZ(),GridSystem.MapBounds, 2);
        flowField = new FlowField(GridSystem.MapBounds/2, 16); //CARFEULL CELL SIZE
        flowField.GetFlowField(goalIndex, GridSystem.RequestGridArray<bool>(GridType.Obstacles));
        
        Grid.CopyFrom(flowField.directionField);
        GridSystem.SubscribeToGrid(GridType.Obstacles, OnNewObstacles);
    }

    private void OnNewObstacles()
    {
#if UNITY_EDITOR
        Stopwatch sw = new Stopwatch();
        sw.Start();
#endif
        flowField.GetFlowField(goalIndex, GridSystem.RequestGridArray<bool>(GridType.Obstacles));
        Grid.CopyFrom(flowField.directionField);
#if UNITY_EDITOR
        sw.Stop();
        UnityEngine.Debug.Log($"Path found: {sw.Elapsed} ms");          
#endif
    }

    private void OnDrawGizmos()
    {
        if (!DebugEnable || Grid.GridArray.IsNullOrEmpty()) return;

        for (int i = 0; i < Grid.GridArray.Length; i++)
        {
            Vector3 pos = Grid.GetCellCenter(i);
            KWUtils.Debug.DrawArrow.ForGizmo(pos, Grid.GridArray[i]);
            //Handles.Label(pos, flowField.BestCostField[i].ToString());
        }
    }
}
