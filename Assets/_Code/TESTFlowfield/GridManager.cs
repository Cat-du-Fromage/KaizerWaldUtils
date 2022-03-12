using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

public enum GridType
{
    Obstacles,
    FlowField,
}

public class GridManager : MonoBehaviour, IGridSystem
{
    public TerrainData MapData { get; set; }
    public int2 MapBounds { get; set; }

    private int chunkSize = 16;
    private int cellSize = 2;

    [SerializeField] private Transform Goal;
    public int goalIndex;
    public Vector3 goalPosition;
    
    private FlowField flowField;
    [SerializeField] private ObstacleManager ObstacleGrid; //cell = 2
    [SerializeField] private DirectionManager directionGrid; //cell = 1
    //How to manage the conversion?
    
    //EventManager?

    private void Awake()
    {
        this.AsInterface<IGridSystem>().InitializeTerrain();
        
        if (Goal == null) return;
        InitGoal();
        
        ObstacleGrid  ??= FindObjectOfType<ObstacleManager>();
        directionGrid ??= FindObjectOfType<DirectionManager>();
        
        MapData = FindObjectOfType<Terrain>().terrainData;
        this.AsInterface<IGridSystem>().InitializeAllGrids();
        
        flowField = new FlowField(MapBounds/cellSize, chunkSize); //CARFEULL CELL SIZE
        flowField.GetFlowField(goalIndex, ObstacleGrid.Grid.GridArray);
    }

    private void InitGoal()
    {
        goalPosition = Goal.position;
        goalIndex = goalPosition.XZ().GetIndexFromPosition(MapBounds, 2);
    }

    public T1[] RequestGrid<T1, T2>(T2 gridType) where T1 : struct where T2 : Enum
    {
        switch (gridType)
        {
            case GridType.Obstacles:
                return ObstacleGrid.Grid.GridArray as T1[];
            case GridType.FlowField:
                return flowField.directionField as T1[];
            default:
                throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null);
        }
        return null;
    }
}
