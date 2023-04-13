using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    /**
     * Full Terrain Data
     */
    public readonly struct MapData
    {
        public readonly int2 NumChunkAxis;
        public readonly int2 NumQuadsAxis;
        public readonly int2 NumVerticesAxis;
        
        // Accessors
        // =============================================================================================================

        public readonly int ChunkSize => NumQuadsAxis.x / NumChunkAxis.x;
        
        // Chunks
        // -------------------------------------------------------------------------------------------------------------
        public readonly int NumChunkX => NumChunkAxis.x;
        public readonly int NumChunkY => NumChunkAxis.y;
        public readonly int ChunksCount => NumChunkX * NumChunkY;
        
        // QUAD
        // -------------------------------------------------------------------------------------------------------------
        public readonly int NumQuadX => NumQuadsAxis.x;
        public readonly int NumQuadY => NumQuadsAxis.y;
        public readonly int MapQuadCount => NumQuadX * NumQuadY;
        
        // VERTEX
        // -------------------------------------------------------------------------------------------------------------
        public readonly int NumVerticesX => NumVerticesAxis.x;
        public readonly int NumVerticesY => NumVerticesAxis.y;
        public readonly int MapVerticesCount => NumVerticesX * NumVerticesY;

        public MapData(TerrainSettings settings)
        {
            NumChunkAxis = settings.NumChunkAxis;
            NumQuadsAxis = settings.NumQuadsAxis;
            NumVerticesAxis = settings.NumVerticesAxis;
        }
    }
}
