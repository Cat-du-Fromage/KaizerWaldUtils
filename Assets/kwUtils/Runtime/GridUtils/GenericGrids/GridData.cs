using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public readonly struct GridData
    {
        public readonly int CellSize;
        public readonly int ChunkSize;
        
        public readonly int NumCellInChunkX;
        
        public readonly int2 MapSize;
        public readonly int2 NumCellXY;
        public readonly int2 NumChunkXY;

        public GridData(int cellSize, int chunkSize, in int2 mapSize)
        {
            CellSize = cellSize;
            ChunkSize = chunkSize;
            MapSize = mapSize;
            NumCellInChunkX = chunkSize / cellSize;
            NumChunkXY = MapSize / ChunkSize;
            NumCellXY = mapSize / cellSize;
        }
        public readonly int TotalCells => NumCellXY.x * NumCellXY.y;
        public readonly int TotalChunk => NumChunkXY.x * NumChunkXY.y;
        public readonly int TotalCellInChunk => NumCellInChunkX * NumCellInChunkX;
    }
    
    //==================================================================================================================
    //==================================================================================================================
    //==================================================================================================================
    public interface IGridData
    {
        public int CellSize   { get; }
        public int2 MapSize   { get; }
        public int2 NumCellXY { get; }
        public int TotalCells();
    }
    
    public readonly struct GridData2 : IGridData
    {
        public int CellSize   { get; }
        public int2 MapSize   { get; }
        public int2 NumCellXY { get; }
        public GridData2(int cellSize, in int2 mapSize)
        {
            CellSize = cellSize;
            MapSize = mapSize;
            NumCellXY = mapSize / cellSize;
        }
        public int TotalCells() => NumCellXY.x * NumCellXY.y;
    }
    
}
