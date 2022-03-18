using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils.KWGenericGrid
{
    public readonly struct GridData
    {
        public readonly int CellSize;
        public readonly int ChunkSize;
        public readonly int NumCellInChunkX;

        public readonly int2 MapSize;
        public readonly int2 NumCellXY;
        public readonly int2 NumChunkXY;

        public GridData(in int2 mapSize, int cellSize, int chunkSize = 1)
        {
            CellSize = cellSize;
            ChunkSize = cellSize > chunkSize ? cellSize : chunkSize;
            MapSize = mapSize;
            
            NumCellInChunkX = ChunkSize >> floorlog2(CellSize);
            NumChunkXY = MapSize >> floorlog2(ChunkSize);
            NumCellXY = MapSize >> floorlog2(CellSize);
        }
        public readonly int TotalCells => NumCellXY.x * NumCellXY.y;
        public readonly int TotalChunk => NumChunkXY.x * NumChunkXY.y;
        public readonly int TotalCellInChunk => NumCellInChunkX * NumCellInChunkX;
    }

}
