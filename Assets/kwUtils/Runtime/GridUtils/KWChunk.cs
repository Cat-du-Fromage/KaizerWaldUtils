#define EnableBurst

using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
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
        public static int2 GetGridCellCoordFromChunkCellCoord(this in int2 cellInChunkCoord, int chunkCellWidth, in int2 chunkCoord)
        {
            return (chunkCoord * chunkCellWidth) + (cellInChunkCoord);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, in GridData gridData, int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(gridData.NumChunkXY.x);
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(gridData.NumCellInChunkX);
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(gridData.NumCellInChunkX, chunkCoord);
            return (cellGridCoord.y * (gridData.NumCellXY.x)) + cellGridCoord.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] //May be useful if we dont want to create a gridData
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, int mapSizeX,int cellSize ,int chunkSize ,int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(mapSizeX/chunkSize);
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(chunkSize);
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(chunkSize/cellSize, chunkCoord);
            return (cellGridCoord.y * mapSizeX) + cellGridCoord.x;
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
            JOrderArrayByChunkIndex<float3> job = new JOrderArrayByChunkIndex<float3>
            {
                CellSize = gridData.CellSize,
                ChunkSize = gridData.ChunkSize,
                NumCellX = gridData.NumCellXY.x,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unorderedIndices,
                SortedArray = orderedIndices
            };
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
            JOrderArrayByChunkIndex<float3> job = new JOrderArrayByChunkIndex<float3>
            {
                CellSize = gridData.CellSize,
                ChunkSize = gridData.ChunkSize,
                NumCellX = gridData.NumCellXY.x,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unOrderedIndices,
                SortedArray = orderedIndices
            };
            job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, default).Complete();
            
            Dictionary<int, Vector3[]> chunkCells = new Dictionary<int, Vector3[]>(gridData.TotalChunk);
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
            int totalChunk = cmul(gridData.NumChunkXY);
            using NativeArray<T> nativeUnOrderedIndices = unorderedIndices.ToNativeArray();
            using NativeArray<T> nativeOrderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            JGenericOrderArrayByChunkIndex2<T> job = new JGenericOrderArrayByChunkIndex2<T>
            {
                ChunkCellWidth = gridData.NumCellInChunkX,
                CellSize = gridData.CellSize,
                ChunkSize = gridData.ChunkSize,
                NumCellX = gridData.NumCellXY.x,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = nativeUnOrderedIndices,
                SortedArray = nativeOrderedIndices
            };
            JobHandle jobHandle = job.ScheduleParallel(unorderedIndices.Length, JobWorkerCount - 1, default);
            JobHandle.ScheduleBatchedJobs();
            
            Dictionary<int, T[]> chunkCells = new Dictionary<int, T[]>(totalChunk);
            jobHandle.Complete();
            for (int i = 0; i < totalChunk; i++)
            {
                chunkCells.Add(i, nativeOrderedIndices.Slice(i * gridData.TotalCellInChunk, gridData.TotalCellInChunk).ToArray());
            }
            return chunkCells;
        }
    }
    
    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // ✂ 1️⃣2️⃣3️⃣ ✂ 4️⃣5️⃣6️⃣ ✂ 7️⃣8️⃣9️⃣
#if EnableBurst
    [BurstCompile]
#endif
    public struct JOrderArrayByChunkIndex<T> : IJobFor
    where T : struct
    {
        //[ReadOnly] public int MapSizeX;
        [ReadOnly] public int CellSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int NumChunkX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkCellWidth = ChunkSize / CellSize;
            int2 cellCoord = index.GetXY2(NumCellX);
            
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);

            int2 coordInChunk = cellCoord - (int2)floor((float2)chunkCoord * chunkCellWidth);

            int indexCellInChunk = mad(coordInChunk.y, chunkCellWidth, coordInChunk.x);
            int chunkIndex = mad(chunkCoord.y, NumChunkX, chunkCoord.x);
            int totalCellInChunk = Sq(chunkCellWidth);
            
            SortedArray[mad(chunkIndex,totalCellInChunk,indexCellInChunk)] = UnsortedArray[index];
        }
    }
    
    //CAREFUL BURST MAKE UNITY CRASH HERE!
    //REPORT THIS TO UNITY'S BURST TEAM!
    //CAREFUL Burst does not support generic call
    //Only enable if the static function calling it is not a generic type!
    public struct JGenericOrderArrayByChunkIndex2<T> : IJobFor
    where T : struct
    {
        [ReadOnly] public int ChunkCellWidth;
        [ReadOnly] public int CellSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int NumChunkX;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int2 cellCoord = index.GetXY2(NumCellX);
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);

            int2 coordInChunk = cellCoord - (int2)floor((float2)chunkCoord * ChunkCellWidth);

            int indexCellInChunk = mad(coordInChunk.y, ChunkCellWidth, coordInChunk.x);
            int chunkIndex = mad(chunkCoord.y, NumChunkX, chunkCoord.x);
            int totalCellInChunk = Sq(ChunkCellWidth);
            
            SortedArray[mad(chunkIndex,totalCellInChunk,indexCellInChunk)] = UnsortedArray[index];
        }

    }
    
    //==================================================================================================================
    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // ✂ 1️⃣2️⃣3️⃣ ✂ 4️⃣5️⃣6️⃣ ✂ 7️⃣8️⃣9️⃣
    /*
#if EnableBurst
    [BurstCompile]
#endif
    public struct JOrderArrayByChunkIndex2<T> : IJobFor
    where T : struct
    {
        [ReadOnly] public int CellSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int NumChunkX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkCellWidth = ChunkSize / CellSize;
            
            int2 cellCoord = index.GetXY2(NumCellX);
            float ratioChunkCell = CellSize / (float)ChunkSize;
            
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratioChunkCell);
            int2 coordInChunk = cellCoord - (int2)floor((float2)chunkCoord * chunkCellWidth);

            int indexInChunk = mad(coordInChunk.y, chunkCellWidth, coordInChunk.x);
            int chunkIndex = mad(chunkCoord.y, NumChunkX, chunkCoord.x);
            int totalCellInChunk = Sq(chunkCellWidth);

            SortedArray[chunkIndex * totalCellInChunk + indexInChunk] = UnsortedArray[index];
        }
    }
    */
}