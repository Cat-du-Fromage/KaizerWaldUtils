using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public readonly struct GridData
    {
        public readonly int CellSize;
        public readonly int ChunkSize;
        
        public readonly int ChunkCellWidth;
        
        public readonly int2 MapSize;
        public readonly int2 NumChunkXY;

        public GridData(int cellSize, int chunkSize, in int2 mapSize)
        {
            CellSize = cellSize;
            ChunkSize = chunkSize;
            MapSize = mapSize;
            ChunkCellWidth = chunkSize / cellSize;
            NumChunkXY = MapSize / ChunkSize;
        }

        public readonly int TotalChunk => NumChunkXY.x * NumChunkXY.y;
        public readonly int TotalCellInChunk => ChunkCellWidth * ChunkCellWidth;
    }
}
