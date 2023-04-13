using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

namespace KWUtils
{
    public sealed class GenericListedChunkedGrid<T> : GenericGrid<T>
    where T : struct
    {
        private readonly int ChunkSize;
        private readonly int2 NumChunkXY;
        
        public GenericListedChunkedGrid(in int2 mapSize, int chunkSize, int cellSize, Func<int, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            ChunkSize = max(cellSize, ceilpow2(chunkSize));
            NumChunkXY = mapSize >> floorlog2(chunkSize);
            
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i);
            }
        }

        public GenericListedChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            ChunkSize = max(cellSize, ceilpow2(chunkSize));
            NumChunkXY = mapSize >> floorlog2(chunkSize);
        }
        //==============================================================================================================
        
        // Get Chunk Cells
        private T[] GetChunkCellsAt(int chunkIndex)
        {
            return default;
        }
    }
}
