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
        
        public List<T[]> Chunks { get; private set; }
        public sealed override GridData GridData => new GridData(MapXY, CellSize, ChunkSize);
        //==============================================================================================================
        //Constructors
        //============
        public GenericListedChunkedGrid(in int2 mapSize, int chunkSize, int cellSize, Func<int, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            ChunkSize = max(cellSize, ceilpow2(chunkSize));
            NumChunkXY = mapSize >> floorlog2(chunkSize);

            Chunks = new List<T[]>(NumChunkXY.x * NumChunkXY.y);
            Chunks = GetGridOrderedByListedChunk(GridArray, GridData);
        }

        public GenericListedChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            ChunkSize = max(cellSize, ceilpow2(chunkSize));
            NumChunkXY = mapSize >> floorlog2(chunkSize);
            
            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            Chunks = new List<T[]>(NumChunkXY.x * NumChunkXY.y);
            Chunks = GetGridOrderedByListedChunk(GridArray, GridData);
        }
        //==============================================================================================================
        
        // Get Chunk Cells
        public NativeArray<T> GetChunkCellsAt(int chunkIndex, Allocator allocator = Allocator.Temp)
        {
            NativeArray<T> fragment = new (Sq(ChunkSize),allocator);
            for (int i = 0; i < ChunkSize; i++)
            {
                int firstIndexRow = i * ChunkSize;
                int firstIndex = GetGridCellIndexFromChunkCellIndex(chunkIndex, GridData, firstIndexRow);
                fragment.AddRange(GridArray, firstIndexRow, firstIndex, ChunkSize);
            }
            return default;
        }
    }
}