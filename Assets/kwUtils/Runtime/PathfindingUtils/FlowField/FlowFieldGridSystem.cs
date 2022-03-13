using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    public enum GridType
    {
        Obstacles,
        FlowField,
    }
    
    public class FlowFieldGridSystem : MonoBehaviour, IGridSystem
    {
        [SerializeField] private Transform Goal;
        public int goalIndex;
        public Vector3 goalPosition;
        
        public TerrainData MapData { get; set; }
        public int2 MapBounds { get; set; }
        
        [SerializeField] private FlowFieldGrid  FlowFieldGrid;
        [SerializeField] private ObstaclesGrid  ObstaclesGrid;
        
        private void Awake()
        {
            this.AsInterface<IGridSystem>().InitializeTerrain();
        
            if (Goal == null) return;
            goalPosition = Goal.position;
            goalIndex = goalPosition.XZ().GetIndexFromPosition(MapBounds, 2);
            
            ObstaclesGrid = GetComponent<ObstaclesGrid>();
            FlowFieldGrid = GetComponent<FlowFieldGrid>();

            this.AsInterface<IGridSystem>().InitializeAllGrids();
        }
        
        private void OnDestroy()
        {
            ObstaclesGrid.Grid.ClearEvents();
            FlowFieldGrid.Grid.ClearEvents();
        }

        public void SubscribeToGrid<T>(T gridType, Action action) 
        where T : Enum
        {
            switch (gridType)
            {
                case GridType.FlowField:
                    FlowFieldGrid.Grid.OnGridChange += action;
                    return;
                case GridType.Obstacles:
                    ObstaclesGrid.Grid.OnGridChange += action;
                    return;
            }
        }

        public T1[] RequestGrid<T1, T2>(T2 gridType) 
        where T1 : struct 
        where T2 : Enum
        {
            return gridType switch
            {
                GridType.Obstacles => ObstaclesGrid.Grid.GridArray as T1[],
                GridType.FlowField => FlowFieldGrid.Grid.GridArray as T1[],
                _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
            };
        }
    }
}
