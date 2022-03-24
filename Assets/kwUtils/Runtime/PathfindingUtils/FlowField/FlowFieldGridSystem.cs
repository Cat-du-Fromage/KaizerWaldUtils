using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    public enum GridType : int
    {
        Obstacles,
        FlowField,
    }
    
    [RequireComponent(typeof(FlowFieldGrid))]
    [RequireComponent(typeof(ObstaclesGrid))]
    public class FlowFieldGridSystem : MonoBehaviour, IGridSystem<GridType>
    {
        private const int CellSize = 1;
        
        [SerializeField] private Transform Goal;
        
        private int goalIndex;

        public TerrainData MapData { get; set; }
        public int2 MapBounds { get; set; }
        
        [SerializeField] private FlowFieldGrid  FlowFieldGrid;
        [SerializeField] private ObstaclesGrid  ObstaclesGrid;
        
        private void Awake()
        {
            this.AsInterface<IGridSystem<GridType>>().InitializeTerrain();
        
            if (Goal == null) return;
            goalIndex = Goal.position.XZ().GetIndexFromPosition(MapBounds, CellSize);
            
            //Implement GridType?
            ObstaclesGrid = GetComponent<ObstaclesGrid>();
            FlowFieldGrid = GetComponent<FlowFieldGrid>();

            this.AsInterface<IGridSystem<GridType>>().InitializeAllGrids();
        }
        
        private void OnDestroy()
        {
            ObstaclesGrid.Grid.ClearEvents();
            FlowFieldGrid.Grid.ClearEvents();
        }

        public void SubscribeToGrid(GridType gridType, Action action)
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

        public T2 RequestGrid<T1, T2>(GridType gridType) 
        where T1 : struct
        where T2 : GenericGrid<T1>
        {
            return gridType switch
            {
                GridType.Obstacles => ObstaclesGrid.Grid as T2,
                GridType.FlowField => FlowFieldGrid.Grid as T2,
                _ => throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null)
            };
        }

        public T1[] RequestGridArray<T1>(GridType gridType) 
            where T1 : struct
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
