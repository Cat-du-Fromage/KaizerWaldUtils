using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    
    public readonly struct ChunkData
    {
        public readonly int CellSize;
        public readonly int ChunkSize;
        
        public readonly int NumCellInChunkX;
        
        public readonly int2 MapSize;
        public readonly int2 NumCellXY;
        public readonly int2 NumChunkXY;

        public ChunkData(int chunkSize, in GridData gridData)
        {
            CellSize = gridData.CellSize;
            ChunkSize = chunkSize;
            MapSize = gridData.MapSize;
            NumCellInChunkX = chunkSize / gridData.CellSize;
            NumChunkXY = gridData.MapSize / chunkSize;
            NumCellXY = gridData.MapSize / gridData.CellSize;
        }
        public readonly int TotalCells => NumCellXY.x * NumCellXY.y;
        public readonly int TotalChunk => NumChunkXY.x * NumChunkXY.y;
        public readonly int TotalCellInChunk => NumCellInChunkX * NumCellInChunkX;
    }
    
    //==================================================================================================================
    
    //==================================================================================================================
    public interface IChunkData
    {
        public int ChunkSize  { get; }
        public int2 NumChunkXY{ get; }

        public int TotalChunk();
        public int TotalCellInChunk();
    }
    
    public readonly struct ChunkData2 : IGridData, IChunkData
    {
        public int CellSize        { get; }
        public int NumCellInChunkX { get; }
        public int2 MapSize        { get; }
        public int2 NumCellXY      { get; }
        
        public int ChunkSize       { get; }
        public int2 NumChunkXY     { get; }
        
        public ChunkData2(int cellSize, int chunkSize, in int2 mapSize)
        {
            ChunkSize = chunkSize;
            CellSize = cellSize;
            MapSize = mapSize;
            NumCellInChunkX = chunkSize / cellSize;
            NumCellXY = mapSize / cellSize;
            NumChunkXY = MapSize / ChunkSize;
        }
        public int TotalCells() => NumCellXY.x * NumCellXY.y;
        public int TotalChunk() => NumChunkXY.x * NumChunkXY.y;
        public int TotalCellInChunk() => NumCellInChunkX * NumCellInChunkX;
    }
    
}
