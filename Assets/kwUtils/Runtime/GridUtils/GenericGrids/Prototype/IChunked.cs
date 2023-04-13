using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

namespace KWUtils
{
    public interface IChunked<in T1, T2>
    where T1 : GenericGrid<T2>
    where T2 : struct
    {
        public GridData GetGridData { get; }
        
        public NativeArray<T2> GetChunkCellsAt(int chunkIndex, Allocator allocator = Allocator.Temp)
        {
            int chunkSize = GetGridData.ChunkSize;
            NativeArray<T2> slice = new (Sq(chunkSize), allocator);
            for (int i = 0; i < chunkSize; i++)
            {
                int firstIndexRow = i * chunkSize;
                int firstIndex = GetGridCellIndexFromChunkCellIndex(chunkIndex, GetGridData, firstIndexRow);
                //slice.
            }
            return default;
        }
    }
}
