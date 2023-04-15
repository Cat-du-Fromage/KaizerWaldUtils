
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using KWUtils.KwTerrain;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using Debug = UnityEngine.Debug;
using int2 = Unity.Mathematics.int2;

namespace KWUtils
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetGridCellCoordFromChunkCellCoord(in int2 cellInChunkCoord, int chunkCellWidth, in int2 chunkCoord)
        {
            return (chunkCoord * chunkCellWidth) + cellInChunkCoord;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetGridCellIndexFromChunkCellIndex(int chunkIndex, in GridData gridData, int cellIndexInsideChunk)
        {
            int2 chunkCoord = GetXY2(chunkIndex,gridData.NumChunkXY.x);
            int2 cellCoordInChunk = GetXY2(cellIndexInsideChunk,gridData.NumCellInChunkX);
            int2 cellGridCoord = GetGridCellCoordFromChunkCellCoord(cellCoordInChunk, gridData.NumCellInChunkX, chunkCoord);
            return GetIndex(cellGridCoord, gridData.NumCellXY.x);//cellGridCoord.y * gridData.NumCellXY.x + cellGridCoord.x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)] //May be useful if we dont want to create a gridData
        public static int GetGridCellIndexFromChunkCellIndex(int chunkIndex, int mapSizeX, int chunkSize, int cellIndexInsideChunk, int cellSize = 1)
        {
            int2 chunkCoord = GetXY2(chunkIndex,mapSizeX/chunkSize);
            int2 cellCoordInChunk = GetXY2(cellIndexInsideChunk,chunkSize);
            int2 cellGridCoord = GetGridCellCoordFromChunkCellCoord(cellCoordInChunk,chunkSize/cellSize, chunkCoord);
            return GetIndex(cellGridCoord, mapSizeX); //(cellGridCoord.y * mapSizeX) + cellGridCoord.x;
        }
        
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
        public static int GetCellIndexFromChunkEnterPoint(int chunkIndex, ChunkEnterPoint point, in GridData gridData)
        {
            return GetGridCellIndexFromChunkCellIndex(chunkIndex, gridData, GetChunkEnterPoint(point, gridData));
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle OrderNativeArrayByChunk<T>(this NativeArray<T> orderedIndices, NativeArray<T> unorderedIndices, in GridData gridData, JobHandle dependency = default)
        where T : struct
        {
            JOrderArrayByChunkIndex<T> job = new (gridData, unorderedIndices, orderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(orderedIndices.Length, JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle OrderNativeArrayByChunk<T>(this NativeArray<T> orderedIndices, NativeArray<T> unorderedIndices, in KwTerrainData data, JobHandle dependency = default)
            where T : struct
        {
            JOrderArrayByChunkIndex<T> job = new (data, unorderedIndices, orderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(orderedIndices.Length, JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        //USE FOR IL2CPP
        private static void GenericGeneration()
        {
            using NativeArray<float2> f21 = new NativeArray<float2>(); 
            using NativeArray<float2> f22 = new NativeArray<float2>();
            f21.OrderNativeArrayByChunk<float2>(f22, new GridData()).Complete();
        }
        
        //==============================================================================================================
        // ORDER ARRAY WITH Shared Indices
        //==============================================================================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle SharedOrderNativeArrayByChunk<T>(this NativeArray<T> orderedIndices, NativeArray<T> unorderedIndices, in KwTerrainData data, JobHandle dependency = default)
        where T : struct
        {
            //get numchunk * chunkVertices
            int numChunk = cmul(data.TerrainSizeXZ / data.ChunkSize);
            int numSharedVertices = cmul(data.ChunkVerticesXZ) * numChunk;

            JSharedOrderArrayByChunkIndex<T> job = new (data, unorderedIndices, orderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        //==============================================================================================================
        // ORDER AND PACK ARRAYS INTO DICTIONARY
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, Vector3[]> GetArrayOrderedByChunk(Vector3[] unorderedIndices, in GridData gridData)
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
        public static Dictionary<int, T[]> GetGridValueOrderedByChunk<T>(T[] unorderedIndices, in GridData gridData)
        where T : struct
        {
            using NativeArray<T> nativeUnOrderedIndices = unorderedIndices.ToNativeArray();
            using NativeArray<T> nativeOrderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JOrderArrayByChunkIndex<T> job = new(gridData, nativeUnOrderedIndices, nativeOrderedIndices);
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PopulateChunkedGrid<T>(Dictionary<int, T[]> chunkedGrid, T[] unorderedIndices, in GridData gridData)
        where T : struct
        {
            using NativeArray<T> nativeUnOrderedIndices = unorderedIndices.ToNativeArray();
            using NativeArray<T> nativeOrderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JOrderArrayByChunkIndex<T> job = new(gridData, nativeUnOrderedIndices, nativeOrderedIndices);
            JobHandle jobHandle = job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, default);
            JobHandle.ScheduleBatchedJobs();
            
            jobHandle.Complete();
            for (int i = 0; i < gridData.TotalChunk; i++)
            {
                nativeOrderedIndices.Slice(i * gridData.TotalCellInChunk, gridData.TotalCellInChunk).CopyTo(chunkedGrid[i]);
            }
        }
        
        //==============================================================================================================
        // ADAPT GRID WITH BIGGER CELLS
        //=============================
        public static T[] AdaptToSmallerGrid<T>(this T[] bigGridToAdapt, in GridData smallGrid, in GridData bigGrid)
        where T : struct
        {
            using NativeArray<T> nativeGridToAdapt = new(bigGridToAdapt, Allocator.TempJob);
            using NativeArray<T> nativeAdaptedGrid = new(smallGrid.TotalCells, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JConvertGridBigToSmall<T> job = new(bigGrid, nativeGridToAdapt, nativeAdaptedGrid);
            job.ScheduleParallel(bigGridToAdapt.Length, JobWorkerCount-1, default).Complete();

            return nativeAdaptedGrid.ToArray();
        }
        
        public static NativeArray<T> NativeAdaptToSmallerGrid<T>(this T[] bigGridToAdapt, in GridData smallGrid, in GridData bigGrid)
        where T : struct
        {
            using NativeArray<T> nativeGridToAdapt = bigGridToAdapt.ToNativeArray();
            NativeArray<T> nativeAdaptedGrid = new(smallGrid.TotalCells, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            JConvertGridBigToSmall<T> job = new(bigGrid, nativeGridToAdapt, nativeAdaptedGrid);
            job.ScheduleParallel(bigGridToAdapt.Length, JobWorkerCount-1, default).Complete();

            return nativeAdaptedGrid;
        }
        //==============================================================================================================
    }

}
