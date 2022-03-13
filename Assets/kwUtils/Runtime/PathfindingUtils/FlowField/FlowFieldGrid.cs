using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using KWUtils.KWGenericGrid;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;

namespace KWUtils.KWGenericGrid
{
    [RequireComponent(typeof(FlowFieldGridSystem))]
    public class FlowFieldGrid : MonoBehaviour, IGridHandler<Vector3, GenericChunkedGrid<Vector3>>
    {
        public bool DebugEnable;
        
        [SerializeField] private Transform Goal;
        private int goalCellIndex;
        
        [SerializeField] private int Chunksize = 16;
        [SerializeField] private int CellSize = 2;
        
        private int totalNumCells;
        private int2 gridSize;
    
        //CostField
        private NativeArray<bool> nativeWalkableCells;
        private NativeArray<byte> nativeCostField;
        //IntegrationField
        private NativeArray<int> nativeBestCostField;
        //FlowField
        private NativeArray<float3> nativeBestDirection;
        private NativeArray<float3> nativeOrderedBestDirection;
    
        private byte[] CostField;
        private int[] BestCostField;
        private Vector3[] directionField;

        private bool jobSchedule;
        private JobHandle lastJobScheduled;
        /// <summary>
        /// Grid Interface
        /// </summary>
        public IGridSystem GridSystem { get; set; }
        public GenericChunkedGrid<Vector3> Grid { get; private set; }
        
        /// <summary>
        /// Find the position of the target cell then get the corresponding index on the grid
        /// </summary>
        /// <param name="terrainBounds">terrain Bounds</param>
        public void InitializeGrid(int2 terrainBounds)
        {
            goalCellIndex = Goal == null ? 0 : Goal.position.XZ().GetIndexFromPosition(terrainBounds, 2);
            Grid = new GenericChunkedGrid<Vector3>(terrainBounds, Chunksize, CellSize);
            gridSize = terrainBounds / CellSize;
            totalNumCells = gridSize.x * gridSize.y;
            UnityEngine.Debug.Log($"Grid.ChunkDictionary COUNT {Grid.ChunkDictionary[0].Length}");
        }

        private void Start()
        {
            CostField      = new byte[totalNumCells];
            BestCostField  = new int[totalNumCells];
            directionField = new Vector3[totalNumCells];
            GridSystem.SubscribeToGrid(GridType.Obstacles, OnNewObstacles);
            CalculateFlowField(GridSystem.RequestGrid<bool, GridType>(GridType.Obstacles), goalCellIndex);
        }

        private void OnDestroy()
        {
            DisposeAll();
        }

        private void Update()
        {
            if (!jobSchedule) return;
            CompleteJob();
        }
        
        private void OnNewObstacles()
        {
#if UNITY_EDITOR
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            CalculateFlowField(GridSystem.RequestGrid<bool, GridType>(GridType.Obstacles), goalCellIndex);
#if UNITY_EDITOR
            sw.Stop();
            UnityEngine.Debug.Log($"Path found: {sw.Elapsed} ms");          
#endif
        }

        private void CompleteJob()
        {
            lastJobScheduled.Complete();
            nativeBestDirection.Reinterpret<Vector3>().CopyTo(directionField);
            
            //Grid.GridArray = directionField;
            int totalChunkCell = KWmath.Sq(Chunksize/CellSize);
            //UnityEngine.Debug.Log($"totalChunkCell {totalChunkCell}");
            for (int i = 0; i < Grid.ChunkDictionary.Count; i++)
            {
                Vector3[] test = nativeOrderedBestDirection
                    .Slice(i * totalChunkCell, totalChunkCell)
                    .SliceConvert<Vector3>()
                    .ToArray();

                if (i == 0)
                {
                    for (int j = 0; j < test.Length; j++)
                    {
                        //UnityEngine.Debug.Log($"test at {j} {test[i]}");
                    }
                }

                Grid.SetValues(i, test);
                /*
                Grid.SetValues(i, nativeOrderedBestDirection
                    .Slice(i * totalChunkCell, totalChunkCell)
                    .SliceConvert<Vector3>()
                    .ToArray());
                    */
            }
            
#if UNITY_EDITOR
            nativeCostField.CopyTo(CostField);
            nativeBestCostField.CopyTo(BestCostField);
#endif
            
            UnityEngine.Debug.Log($"Test array length {Grid.GridArray.Length}");
            DisposeAll();
            jobSchedule = false;
        }

        private void CalculateFlowField(bool[] walkableCells = null, int targetCell = -1)
        {
            goalCellIndex = targetCell == -1 ? goalCellIndex : targetCell;
            
            //Cost Field
            nativeWalkableCells = walkableCells.ToNativeArray();
            nativeCostField = AllocNtvAry<byte>(totalNumCells);
            JobHandle jHCostField = GetCostField();

            //Integration Field
            nativeBestCostField = AllocFillNtvAry<int>(totalNumCells, ushort.MaxValue);
            JobHandle jHIntegrationField = GetIntegrationField(targetCell, jHCostField);
        
            //Direction Field
            nativeBestDirection = AllocNtvAry<float3>(totalNumCells);
            JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
            
            nativeOrderedBestDirection = AllocNtvAry<float3>(totalNumCells);
            lastJobScheduled = nativeOrderedBestDirection.OrderNativeArrayByChunk(nativeBestDirection, Grid.gridData, jHDirectionField);
            JobHandle.ScheduleBatchedJobs();
            jobSchedule = true;
        }
        
        private JobHandle GetCostField(in JobHandle dependency = default)
        {
            JCostField job = new JCostField
            {
                Obstacles = nativeWalkableCells,
                CostField = nativeCostField,
            };
            return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
        }

        private JobHandle GetIntegrationField(int targetCellIndex, in JobHandle dependency = default)
        {
            JIntegrationField job = new JIntegrationField
            {
                DestinationCellIndex = targetCellIndex,
                MapSizeX = gridSize.x,
                CostField = nativeCostField,
                BestCostField = nativeBestCostField
            };
            return job.Schedule(dependency);
        }

        private JobHandle GetDirectionField(in JobHandle dependency = default)
        {
            JBestDirection job = new JBestDirection
            {
                MapSizeX = gridSize.x,
                BestCostField = nativeBestCostField,
                CellBestDirection = nativeBestDirection
            };
            return job.ScheduleParallel(totalNumCells, JobWorkerCount - 1, dependency);
        }

        private void DisposeAll()
        {
            if (nativeWalkableCells.IsCreated)        nativeWalkableCells.Dispose();
            if (nativeCostField.IsCreated)            nativeCostField.Dispose();
            if (nativeBestCostField.IsCreated)        nativeBestCostField.Dispose();
            if (nativeBestDirection.IsCreated)        nativeBestDirection.Dispose();
            if (nativeOrderedBestDirection.IsCreated) nativeOrderedBestDirection.Dispose();
        }
        
        private void OnDrawGizmos()
        {
            if (!DebugEnable || Grid.GridArray.IsNullOrEmpty()) return;
/*
            for (int i = 0; i < Grid.GridArray.Length; i++)
            {
                Vector3 pos = Grid.GetCellCenter(i);
                Debug.DrawArrow.ForGizmo(pos, Grid.GridArray[i]);
                //Handles.Label(pos, flowField.BestCostField[i].ToString());
            }
            */

            Gizmos.color = Color.red;
            foreach ((int id, Vector3[] values)in Grid.ChunkDictionary)
            {
                if (id == 0)
                {
                    Gizmos.DrawWireCube(Grid.GetChunkCenter(0), (Vector3.one * Chunksize).Flat());
                    Gizmos.color = Color.green;
                    for (int i = 0; i < values.Length; i++)
                    {
                        int cellindex = id.GetGridCellIndexFromChunkCellIndex(Grid.gridData, i);
                        UnityEngine.Debug.Log(cellindex);
                        float2 test = (float2)cellindex.GetXY2(Grid.gridData.ChunkCellWidth) * CellSize;
                        Vector3 cellPos = new Vector3(test.x + 1, 0, test.y + 1);
                        Debug.DrawArrow.ForGizmo(cellPos, Grid.GridArray[i]);
                    }
                }
            }
            
        }
    }
}
