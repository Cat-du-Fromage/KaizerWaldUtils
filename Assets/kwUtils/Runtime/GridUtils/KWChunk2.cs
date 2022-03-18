using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using Debug = UnityEngine.Debug;
using int2 = Unity.Mathematics.int2;
/*
namespace KWUtils.KWGenericGrid
{
    public enum ChunkEnterPoint
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    public static class KWChunk
    {
        /// <summary>
        /// Cell is at the constant size of 1!
        /// chunk can only be a square, meaning : width = height
        /// </summary>

        //CHUNK
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetGridCellCoordFromChunkCellCoord(this in int2 cellInChunkCoord, int chunkCellWidth, in int2 chunkCoord)
        {
            return (chunkCoord * chunkCellWidth) + cellInChunkCoord;
        }

        //CHUNK
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, in GridData gridData, int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(gridData.NumChunkXY.x);
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(gridData.NumCellInChunkX);
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(gridData.NumCellInChunkX, chunkCoord);
            return (cellGridCoord.y * (gridData.NumCellXY.x)) + cellGridCoord.x;
        }

        //CHUNK
        [MethodImpl(MethodImplOptions.AggressiveInlining)] //May be useful if we dont want to create a gridData
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, int mapSizeX, int cellSize, int chunkSize, int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(mapSizeX/chunkSize);
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(chunkSize);
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(chunkSize/cellSize, chunkCoord);
            return (cellGridCoord.y * mapSizeX) + cellGridCoord.x;
        }

        //CHUNK
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetChunkEnterPoint(ChunkEnterPoint point, in GridData gridData) =>
        point switch
        {
            ChunkEnterPoint.Left      => (int)floor((gridData.ChunkSize - 1) / 2f) * gridData.ChunkSize,
            ChunkEnterPoint.Right     => (int)floor((gridData.ChunkSize - 1) / 2f) * gridData.ChunkSize + (int)floor(gridData.ChunkSize - 1),
            ChunkEnterPoint.Top       => (int)floor(gridData.ChunkSize - 1) * gridData.ChunkSize + (int)floor((gridData.ChunkSize - 1) / 2f),
            ChunkEnterPoint.Bottom    => (int)floor((gridData.ChunkSize - 1) / 2f),
            _                         => -1,
        };
        //CHUNK
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndexFromChunkEnterPoint(this int chunkIndex, ChunkEnterPoint point, in GridData gridData)
        {
            return chunkIndex.GetGridCellIndexFromChunkCellIndex(gridData, GetChunkEnterPoint(point, gridData));
        }

        //==============================================================================================================
        // ORDER ARRAY
        //==============================================================================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle OrderNativeArrayByChunk(this NativeArray<float3> orderedIndices, NativeArray<float3> unorderedIndices, in GridData gridData, JobHandle dependency = default)
        {
            JOrderArrayByChunkIndex<float3> job = new (gridData, unorderedIndices, orderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(orderedIndices.Length, JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        //==============================================================================================================
        // ORDER AND PACK ARRAYS INTO DICTIONARY
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, Vector3[]> GetArrayOrderedByChunk(this Vector3[] unorderedIndices, in GridData gridData)
        {
            using NativeArray<float3> unOrderedIndices = unorderedIndices.ToNativeArray().Reinterpret<float3>(); 
            using NativeArray<float3> orderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JOrderArrayByChunkIndex<float3> job = new (gridData, unOrderedIndices, orderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, default);
            JobHandle.ScheduleBatchedJobs();
            
            Dictionary<int, Vector3[]> chunkCells = new Dictionary<int, Vector3[]>(gridData.TotalChunk);
            jobHandle.Complete();
            for (int i = 0; i < gridData.TotalChunk; i++)
            {
                chunkCells.Add(i, orderedIndices.Slice(i * gridData.TotalCellInChunk, gridData.TotalCellInChunk).SliceConvert<Vector3>().ToArray());
            }
            return chunkCells;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, T[]> GetGridValueOrderedByChunk<T>(this T[] unorderedIndices, in GridData gridData)
        where T : struct
        {
            using NativeArray<T> nativeUnOrderedIndices = unorderedIndices.ToNativeArray();
            using NativeArray<T> nativeOrderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JGenericOrderArrayByChunkIndex<T> job = new(gridData, nativeUnOrderedIndices, nativeOrderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, default);
            JobHandle.ScheduleBatchedJobs();
            
            Dictionary<int, T[]> chunkCells = new Dictionary<int, T[]>(gridData.TotalChunk);
            jobHandle.Complete();
            for (int i = 0; i < gridData.TotalChunk; i++)
            {
                chunkCells.Add(i, nativeOrderedIndices.Slice(i * gridData.TotalCellInChunk, gridData.TotalCellInChunk).ToArray());
            }
            return chunkCells;
        }
    }
}
*/