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
        public static int2 GetGridCellCoordFromChunkCellCoord(this in int2 cellInChunkCoord, int cellSize, int chunkSize, in int2 chunkCoord)
        {
            return (chunkCoord * (chunkSize/cellSize)) + (cellInChunkCoord);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, in GridData gridData, int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(gridData.NumChunkXY.x);
            
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(gridData.ChunkCellWidth);
            
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(gridData.CellSize, gridData.ChunkSize, chunkCoord);
            //if(chunkCoord.Equals(int2.zero))
                //UnityEngine.Debug.Log($"chunkCoord : {chunkCoord} at {cellCoordInChunk} id :{(cellGridCoord.y * (gridData.MapSize.x/gridData.CellSize)) + cellGridCoord.x}");
            return (cellGridCoord.y * (gridData.MapSize.x/gridData.CellSize)) + cellGridCoord.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] //May be useful if we dont want to create a gridData
        public static int GetGridCellIndexFromChunkCellIndex(this int chunkIndex, int mapSizeX,int cellSize ,int chunkSize ,int cellIndexInsideChunk)
        {
            int2 chunkCoord = chunkIndex.GetXY2(mapSizeX/chunkSize);
            int2 cellCoordInChunk = cellIndexInsideChunk.GetXY2(chunkSize);
            int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(cellSize, chunkSize, chunkCoord);
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
        public static JobHandle OrderNativeArrayByChunk(this NativeArray<float3> orderedIndices, NativeArray<float3> unorderedIndices, in GridData gridData, in JobHandle dependency = default)
        {
            int totalChunk = cmul(gridData.NumChunkXY);

            JOrderArrayByChunkIndex2<float3> job = new JOrderArrayByChunkIndex2<float3>
            {
                MapSizeX = gridData.MapSize.x,
                CellSize = gridData.CellSize,
                ChunkSize = gridData.ChunkSize,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unorderedIndices,
                SortedArray = orderedIndices
            };
            JobHandle jobHandle = job.ScheduleParallel(totalChunk, JobWorkerCount - 1, dependency);
            return jobHandle;
        }
        
        //==============================================================================================================
        // ORDER AND PACK ARRAYS INTO DICTIONARY
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, int[]> GetArrayOrderedByChunk(int[] unorderedIndices, in GridData gridData)
        {
            int totalChunk = Sq(gridData.ChunkCellWidth);
            using NativeArray<int> unOrderedIndices = unorderedIndices.ToNativeArray(); 
            using NativeArray<int> orderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JOrderArrayByChunkIndex<int> job = new JOrderArrayByChunkIndex<int>
            {
                MapSizeX = gridData.MapSize.x,
                ChunkSize = gridData.ChunkSize,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unOrderedIndices,
                SortedArray = orderedIndices
            };
            job.ScheduleParallel(totalChunk, JobWorkerCount - 1, default).Complete();
            
            Dictionary<int, int[]> chunkCells = new Dictionary<int, int[]>(totalChunk);
            int totalChunkCell = (gridData.ChunkSize * gridData.ChunkSize);
            //int offsetChunk = startOffset - 1;
            for (int i = 0; i < totalChunk; i++)
            {
                int start = i * totalChunkCell;
                chunkCells.Add(i, orderedIndices.GetSubArray(start, totalChunkCell).ToArray());
            }
            return chunkCells;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, Vector3[]> GetArrayOrderedByChunk(this Vector3[] unorderedIndices, in GridData gridData)
        {
            int totalChunk = Sq(gridData.ChunkCellWidth);
            using NativeArray<float3> unOrderedIndices = unorderedIndices.ToNativeArray().Reinterpret<float3>(); 
            using NativeArray<float3> orderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JOrderArrayByChunkIndex<float3> job = new JOrderArrayByChunkIndex<float3>
            {
                MapSizeX = gridData.MapSize.x,
                ChunkSize = gridData.ChunkSize,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unOrderedIndices,
                SortedArray = orderedIndices
            };
            job.ScheduleParallel(totalChunk, JobWorkerCount - 1, default).Complete();
            
            Dictionary<int, Vector3[]> chunkCells = new Dictionary<int, Vector3[]>(totalChunk);
            int totalChunkCell = (gridData.ChunkSize * gridData.ChunkSize);
            for (int i = 0; i < totalChunk; i++)
            {
                int start = i * totalChunkCell;
                chunkCells.Add(i, orderedIndices.GetSubArray(start, totalChunkCell).Reinterpret<Vector3>().ToArray());
            }
            return chunkCells;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, byte[]> GetArrayOrderedByChunk(byte[] unorderedIndices, in GridData gridData)
        {
            int totalChunk = Sq(gridData.ChunkCellWidth);
            using NativeArray<byte> unOrderedIndices = unorderedIndices.ToNativeArray(); 
            using NativeArray<byte> orderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JOrderArrayByChunkIndex<byte> job = new JOrderArrayByChunkIndex<byte>
            {
                MapSizeX = gridData.MapSize.x,
                ChunkSize = gridData.ChunkSize,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = unOrderedIndices,
                SortedArray = orderedIndices
            };
            job.ScheduleParallel(totalChunk, JobWorkerCount - 1, default).Complete();
            
            Dictionary<int, byte[]> chunkCells = new Dictionary<int, byte[]>(totalChunk);
            int totalChunkCell = (gridData.ChunkSize * gridData.ChunkSize);
            for (int i = 0; i < totalChunk; i++)
            {
                chunkCells.Add(i, orderedIndices.GetSubArray(i * totalChunkCell, totalChunkCell).ToArray());
            }
            return chunkCells;
        }
        
        //This One is not eligible for Burst Compile!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<int, T[]> GetGridValueOrderedByChunk<T>(this T[] unorderedIndices, in GridData gridData)
        where T : struct
        {
            int totalChunk = cmul(gridData.NumChunkXY);
            using NativeArray<T> nativeUnOrderedIndices = unorderedIndices.ToNativeArray();
            using NativeArray<T> nativeOrderedIndices = new (unorderedIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            JGenericOrderArrayByChunkIndex2<T> job = new JGenericOrderArrayByChunkIndex2<T>
            {
                MapSizeX = gridData.MapSize.x,
                ChunkCellWidth = gridData.ChunkCellWidth,
                ChunkSize = gridData.ChunkSize,
                NumChunkX = gridData.NumChunkXY.x,
                UnsortedArray = nativeUnOrderedIndices,
                SortedArray = nativeOrderedIndices
            };
            JobHandle jobHandle = job.ScheduleParallel(totalChunk, JobWorkerCount - 1, default);
            JobHandle.ScheduleBatchedJobs();
            
            Dictionary<int, T[]> chunkCells = new Dictionary<int, T[]>(totalChunk);
            jobHandle.Complete();
            int totalCellInChunk = Sq(gridData.ChunkCellWidth);
            for (int i = 0; i < totalChunk; i++)
            {
                chunkCells.Add(i, nativeOrderedIndices.Slice(i * totalCellInChunk, totalCellInChunk).ToArray());
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
        [ReadOnly] public int MapSizeX;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumChunkX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkPosY = (int)floor(index / (float)NumChunkX);
            int chunkPosX = index - (chunkPosY * NumChunkX);
            
            for (int z = 0; z < ChunkSize; z++) // z axis
            {
                int startY = (chunkPosY * MapSizeX) * ChunkSize;
                int startX = chunkPosX * ChunkSize;
                int startYChunk = z * MapSizeX;
                int start = startY + startX + startYChunk;

                for (int x = 0; x < ChunkSize; x++) // x axis
                {
                    int sliceIndex = mad(z, ChunkSize, x) + (index * (ChunkSize * ChunkSize));
                    SortedArray[sliceIndex] = UnsortedArray[start + x];
                }
            }
        }
    }
    
    //CAREFUL BURST MAKE UNITY CRASH HERE!
    //REPORT THIS TO UNITY'S BURST TEAM!
    //CAREFUL Burst does not support generic call
    //Only enable if the static function calling it is not a generic type!
    public struct JGenericOrderArrayByChunkIndex<T> : IJobFor
    where T : struct
    {
        [ReadOnly] public int MapSizeX;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumChunkX;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkPosY = (int)floor(index / (float)NumChunkX);
            int chunkPosX = index - (chunkPosY * NumChunkX);
            
            for (int z = 0; z < ChunkSize; z++) // z axis
            {
                int startY = (chunkPosY * MapSizeX) * ChunkSize;
                int startX = chunkPosX * ChunkSize;
                int startYChunk = z * MapSizeX;
                int start = startY + startX + startYChunk;

                for (int x = 0; x < ChunkSize; x++) // x axis
                {
                    int sliceIndex = mad(z, ChunkSize, x) + (index * (ChunkSize * ChunkSize));
                    SortedArray[sliceIndex] = UnsortedArray[start + x];
                }
            }
        }

    }
    
    public struct JGenericOrderArrayByChunkIndex2<T> : IJobFor
    where T : struct
    {
        [ReadOnly] public int MapSizeX;
        [ReadOnly] public int ChunkCellWidth;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumChunkX;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkPosY = (int)floor(index / (float)NumChunkX);
            int chunkPosX = index - (chunkPosY * NumChunkX);
            
            for (int z = 0; z < ChunkCellWidth; z++) // z axis
            {
                int startY = (chunkPosY * MapSizeX) * ChunkSize;
                int startX = chunkPosX * ChunkSize;
                int startYChunk = z * MapSizeX;
                int start = startY + startX + startYChunk;

                for (int x = 0; x < ChunkCellWidth; x++) // x axis
                {
                    int sliceIndex = mad(z, ChunkSize, x) + (index * (ChunkSize * ChunkSize));
                    SortedArray[sliceIndex] = UnsortedArray[start + x];
                }
            }
        }

    }
    
    //==================================================================================================================
    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // ✂ 1️⃣2️⃣3️⃣ ✂ 4️⃣5️⃣6️⃣ ✂ 7️⃣8️⃣9️⃣
    
#if EnableBurst
    [BurstCompile]
#endif
    public struct JOrderArrayByChunkIndex2<T> : IJobFor
        where T : struct
    {
        [ReadOnly] public int MapSizeX;
        [ReadOnly] public int CellSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumChunkX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkPosY = (int)floor(index / (float)NumChunkX);
            int chunkPosX = index - (chunkPosY * NumChunkX);

            for (int z = 0; z < ChunkSize; z++) // z axis
            {
                int startY = (chunkPosY * MapSizeX) * ChunkSize;
                int startX = chunkPosX * ChunkSize;
                int startYChunk = z * MapSizeX;
                int start = startY + startX + startYChunk;

                for (int x = 0; x < ChunkSize; x++) // x axis
                {
                    int sliceIndex = mad(z, ChunkSize, x) + (index * (ChunkSize * ChunkSize));
                    SortedArray[sliceIndex] = UnsortedArray[start + x];
                }
            }
        }
    }
    
}