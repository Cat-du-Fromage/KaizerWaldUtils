using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

using static KWUtils.KWGrid;

public enum GridType
{
    Obstacles,
    Directions,
    FlowField,
}


public class GridManager : MonoBehaviour, IGridSystem<GridType>
{
    public TerrainData MapData { get; set; }
    public int2 MapBounds { get; set; }
    

    private int chunkSize = 16;
    private int cellSize = 2;

    [SerializeField] private Transform Goal;
    public int goalIndex;
    public Vector3 goalPosition;

    //private FlowField flowField;
    [SerializeField] private FlowFieldGrid    FlowFieldGrid;
    [SerializeField] private ObstacleManager  ObstacleGrid; //cell = 2
    [SerializeField] private DirectionManager DirectionGrid; //cell = 1
    //How to manage the conversion?
    
    //EventManager?

    private void Awake()
    {
        this.AsInterface<IGridSystem<GridType>>().InitializeTerrain();
        
        if (Goal == null) return;
        InitGoal();
        
        FlowFieldGrid ??= FindObjectOfType<FlowFieldGrid>();
        ObstacleGrid  ??= FindObjectOfType<ObstacleManager>();
        DirectionGrid ??= FindObjectOfType<DirectionManager>();
        
        this.AsInterface<IGridSystem<GridType>>().InitializeAllGrids();
        
        //flowField = new FlowField(MapBounds/cellSize, chunkSize, goalIndex); //CARFEULL CELL SIZE
        //flowField.GetFlowField(goalIndex, ObstacleGrid.Grid.GridArray);
    }

    private void OnDestroy()
    {
        ObstacleGrid.Grid.ClearEvents();
        DirectionGrid.Grid.ClearEvents();
        FlowFieldGrid.Grid.ClearEvents();
    }

    private void InitGoal()
    {
        goalPosition = Goal.position;
        goalIndex = GetIndexFromPosition(goalPosition.XZ(), MapBounds, 2);
    }
    
    public void SubscribeToGrid(GridType gridType, Action action)
    {
        switch (gridType)
        {
            case GridType.Obstacles:
                ObstacleGrid.Grid.OnGridChange += action;
                return;
            case GridType.Directions:
                DirectionGrid.Grid.OnGridChange += action;
                return;
            case GridType.FlowField:
                return;
        }
    }

    public T2 RequestGrid<T1, T2>(GridType gridType) 
    where T1 : struct 
    where T2 : GenericGrid<T1>
    {
        return gridType switch
        {
            GridType.Obstacles => ObstacleGrid.Grid as T2,
            GridType.FlowField => FlowFieldGrid.Grid as T2,
            _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
        };
    }

    public T1[] RequestGridArray<T1>(GridType gridType) where T1 : struct
    {
        return gridType switch
        {
            GridType.Obstacles => ObstacleGrid.Grid.GridArray as T1[],
            GridType.FlowField => FlowFieldGrid.Grid.GridArray as T1[],
            _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
        };
    }
    
}
