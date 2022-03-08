using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.NativeCollectionExt;

namespace KWUtils
{
    public static class SpatialPartitionUtils
    {
        /// <summary>
        /// Given a mapSize/gridSize + a desired number of cell per axis
        /// Get a cell Size
        /// </summary>
        /// <param name="mapSize">Size of the grid to divide</param>
        /// <param name="numCellPerAxis">desired number of cell per axis</param>
        /// <returns></returns>
        public static float GetCellSize(int mapSize, int numCellPerAxis) => (float)mapSize / numCellPerAxis;

        
        /// <summary>
        /// Get spatial partition's cells bound
        /// </summary>
        /// <param name="mapSize"></param>
        /// <param name="numCellPerAxis"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public static NativeArray<Bounds> GetCellsBounds(int mapSize, int numCellPerAxis, in JobHandle dependency = default)
        {
            float cellSize = GetCellSize(mapSize, numCellPerAxis);
            int totalCells = numCellPerAxis * numCellPerAxis;
            
            NativeArray<Bounds> bounds = AllocNtvAry<Bounds>(totalCells);

            CellBound(mapSize, numCellPerAxis, ref bounds).Complete();
            //Bounds[] boundsArray = bounds.ToArray();

            return bounds;
        }

        public static int[] GetObjectInCells(int mapSize, int numCellPerAxis, NativeArray<float3> positions, in JobHandle dependency = default)
        {
            float cellSize = GetCellSize(mapSize, numCellPerAxis);
            int totalCells = numCellPerAxis * numCellPerAxis;
            
            NativeArray<Bounds> bounds = AllocNtvAry<Bounds>(totalCells);
            NativeArray<int> objIds = AllocNtvAry<int>(positions.Length);

            ObjectIdsInCell(in mapSize, in numCellPerAxis, ref bounds, ref positions, ref objIds).Complete();
            int[] objectIds = objIds.ToArray();
            
            bounds.Dispose();
            objIds.Dispose();
            return objectIds;
        }
        
        //JOB SYSTEM
        //==============================================================================================================

        private static JobHandle CellBound(int mapSize, int numCellPerAxis, ref NativeArray<Bounds> bounds, in JobHandle dependency = default)
        {
            int totalCells = numCellPerAxis * numCellPerAxis;
            CellBounds2DJob job = new CellBounds2DJob(numCellPerAxis, GetCellSize(mapSize,numCellPerAxis), bounds);
            return job.ScheduleParallel(totalCells, JobsUtility.JobWorkerCount - 1, dependency);
        }
        
        private static JobHandle ObjectIdsInCell(in int mapSize,
                                                 in int numCellPerAxis, 
                                                 ref NativeArray<Bounds> bounds,
                                                 ref NativeArray<float3> positions,
                                                 ref NativeArray<int> objectIds,
                                                 in JobHandle dependency = default)
        {
            JobHandle cellJobHandle = CellBound(mapSize, numCellPerAxis, ref bounds);
            ObjectIdInCellsJob objectIdJob = new ObjectIdInCellsJob(bounds, positions, objectIds);
            return objectIdJob.ScheduleParallel(positions.Length, JobsUtility.JobWorkerCount - 1, cellJobHandle);
        }
        
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct CellBounds2DJob : IJobFor
    {
        [ReadOnly]private int jNumCellPerAxis;
        [ReadOnly]private float jCellSize;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<Bounds> jBounds;

        public CellBounds2DJob(int numCell, float cellSize, NativeArray<Bounds> bounds)
        {
            jNumCellPerAxis = numCell;
            jCellSize = cellSize;
            jBounds = bounds;
        }
        public void Execute(int index)
        {
            int z = (int)floor((float)index / jNumCellPerAxis);
            int x = index - (z * jNumCellPerAxis);
            
            float xCenterCell = GetMinMaxAdd(in x) * 0.5f;
            float zCenterCell = GetMinMaxAdd(in z) * 0.5f;

            float3 center = new float3(xCenterCell, 0, zCenterCell);
            float3 boundSize = new float3(jCellSize, 0, jCellSize);
            
            jBounds[index] = new Bounds(center,boundSize);
        }

        private float GetMinMaxAdd(in int axis)
        {
            float min = axis * jCellSize;
            float max = (axis + 1) * jCellSize;
            return min+max;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct ObjectIdInCellsJob: IJobFor
    {
        [ReadOnly] private NativeArray<Bounds> jCellBounds;
        [ReadOnly] private NativeArray<float3> jPositions;
        [NativeDisableParallelForRestriction]
        [WriteOnly]private NativeArray<int> jObjectsId;

        public ObjectIdInCellsJob(NativeArray<Bounds> bounds, NativeArray<float3> positions, NativeArray<int> objIds)
        {
            jCellBounds = bounds;
            jPositions = positions;
            jObjectsId = objIds;
        }
        
        public void Execute(int index)
        {
            for (int i = 0; i < jCellBounds.Length; i++)
            {
                if (!jCellBounds[i].Contains(jPositions[index])) continue;
                jObjectsId[index] = i;
            }
        }
    }
}