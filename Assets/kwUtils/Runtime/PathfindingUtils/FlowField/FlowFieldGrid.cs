using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using KWUtils.KWGenericGrid;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
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

        //CostField
        private NativeArray<bool> nativeObstacles;
        private NativeArray<byte> nativeCostField;
        //IntegrationField
        private NativeArray<int> nativeBestCostField;
        //FlowField
        private NativeArray<float3> nativeBestDirection;
        private NativeArray<float3> nativeOrderedBestDirection;
        
#if UNITY_EDITOR
        private byte[] CostField;
        private int[] BestCostField;
        private Vector3[] directionField;
#endif

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
            UnityEngine.Debug.Log($"Grid.ChunkDictionary COUNT {Grid.ChunkDictionary[0].Length}");
        }

        private void Start()
        {
#if UNITY_EDITOR
            //CostField      = new byte[Grid.GridData.TotalCells];
            //BestCostField  = new int[Grid.GridData.TotalCells];
            //directionField = new Vector3[Grid.GridData.TotalCells];
#endif
            GridSystem.SubscribeToGrid(GridType.Obstacles, OnNewObstacles);
            CalculateFlowField(GridSystem.RequestGrid<bool, GridType>(GridType.Obstacles), goalCellIndex);
        }

        private void OnDestroy()
        {
            lastJobScheduled.Complete();
            DisposeAll();
        }

        private void Update()
        {
            if (!jobSchedule) return;
            if (!lastJobScheduled.IsCompleted) return;
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
#if UNITY_EDITOR
            //nativeBestDirection.Reinterpret<Vector3>().CopyTo(directionField);
#endif
            int totalChunkCell = Grid.GridData.TotalCellInChunk;
            for (int i = 0; i < Grid.ChunkDictionary.Count; i++)
            {
                Grid.SetValues(i, nativeOrderedBestDirection
                    .Slice(i * totalChunkCell, totalChunkCell)
                    .SliceConvert<Vector3>()
                    .ToArray());
            }
            
#if UNITY_EDITOR
            //nativeCostField.CopyTo(CostField);
            //nativeBestCostField.CopyTo(BestCostField);
#endif
            DisposeAll();
            jobSchedule = false;
        }

        private void CalculateFlowField(bool[] obstacles = null, int targetCell = -1)
        {
            goalCellIndex = targetCell == -1 ? goalCellIndex : targetCell;
            
            //Cost Field
            nativeObstacles = obstacles.ToNativeArray();
            int totalCells = Grid.GridData.TotalCells;
            nativeCostField = AllocNtvAry<byte>(totalCells);
            JobHandle jHCostField = GetCostField();

            //Integration Field
            nativeBestCostField = AllocFillNtvAry<int>(totalCells, ushort.MaxValue);
            JobHandle jHIntegrationField = GetIntegrationField(targetCell, jHCostField);
        
            //Direction Field
            nativeBestDirection = AllocNtvAry<float3>(totalCells);
            JobHandle jHDirectionField = GetDirectionField(jHIntegrationField);
            
            nativeOrderedBestDirection = AllocNtvAry<float3>(totalCells);
            lastJobScheduled = nativeOrderedBestDirection.OrderNativeArrayByChunk(nativeBestDirection, Grid.GridData, jHDirectionField);
            
            JobHandle.ScheduleBatchedJobs();
            jobSchedule = true;
        }
        
        private JobHandle GetCostField(in JobHandle dependency = default)
        {
            JCostField job = new JCostField
            {
                Obstacles = nativeObstacles,
                CostField = nativeCostField,
            };
            return job.ScheduleParallel(nativeCostField.Length, JobWorkerCount - 1, dependency);
        }

        private JobHandle GetIntegrationField(int targetCellIndex, in JobHandle dependency = default)
        {
            JIntegrationField job = new JIntegrationField
            {
                DestinationCellIndex = targetCellIndex,
                NumCellX = Grid.GridData.NumCellXY.x,
                CostField = nativeCostField,
                BestCostField = nativeBestCostField
            };
            return job.Schedule(dependency);
        }

        private JobHandle GetDirectionField(in JobHandle dependency = default)
        {
            JBestDirection job = new JBestDirection
            {
                NumCellX = Grid.GridData.NumCellXY.x,
                BestCostField = nativeBestCostField,
                CellBestDirection = nativeBestDirection
            };
            return job.ScheduleParallel(nativeBestCostField.Length, JobWorkerCount - 1, dependency);
        }

        private void DisposeAll()
        {
            if (nativeObstacles.IsCreated)            nativeObstacles.Dispose();
            if (nativeCostField.IsCreated)            nativeCostField.Dispose();
            if (nativeBestCostField.IsCreated)        nativeBestCostField.Dispose();
            if (nativeBestDirection.IsCreated)        nativeBestDirection.Dispose();
            if (nativeOrderedBestDirection.IsCreated) nativeOrderedBestDirection.Dispose();
        }
        
        private void OnDrawGizmos()
        {
            if (!DebugEnable || Grid.GridArray.IsNullOrEmpty() || Grid.ChunkDictionary is null) return;
            /*
            Gizmos.color = Color.green;
            for (int i = 0; i < directionField.Length; i++)
            {
                Vector3 cellPos = Grid.GetCellCenter(i);
                Debug.DrawArrow.ForGizmo(cellPos, directionField[i]);
                //Handles.Label(cellPos, directionField[i].ToString());
            }
            */
            
            foreach ((int id, Vector3[] values)in Grid.ChunkDictionary)
            {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(Grid.GetChunkCenter(id), new Vector3(Chunksize,0.5f,Chunksize));
                
                    for (int i = 0; i < values.Length; i++)
                    {
                        Gizmos.color = Color.green;
                        Vector3 cellPos = Grid.GetChunkCellCenter(id, i);
                        Gizmos.DrawWireCube(cellPos, (Vector3.one * Grid.GridData.CellSize).Flat());
                        Debug.DrawArrow.ForGizmo(cellPos, values[i]);
                    }
            }
            
            /*
            //PROBLEM GETTING INDEX!
            Gizmos.color = Color.green;
            for (int i = 0; i < Grid.GridArray.Length; i++)
            {
                Vector3 cellPos = Grid.GetCellCenter(i);
                Debug.DrawArrow.ForGizmo(cellPos, Grid.GridArray[i]);
            }
            */
        }
    }
}
