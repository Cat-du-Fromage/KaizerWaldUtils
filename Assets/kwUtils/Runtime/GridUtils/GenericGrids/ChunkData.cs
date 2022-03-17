using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
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
}
