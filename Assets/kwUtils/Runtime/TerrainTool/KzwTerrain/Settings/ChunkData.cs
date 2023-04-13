using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    public readonly struct ChunkData
    {
        public readonly int NumQuadPerLine;
        public readonly int QuadsCount;
        public readonly int NumVerticesPerLine;
        public readonly int VerticesCount;
        public readonly int TrianglesCount;
        public readonly int TriangleIndicesCount;
        
        public ChunkData(TerrainSettings settings)
        {
            NumQuadPerLine       = settings.NumQuadPerLine;
            QuadsCount           = settings.ChunkQuadsCount;
            NumVerticesPerLine   = settings.ChunkVerticesPerLine;
            VerticesCount        = settings.ChunkVerticesCount;
            TrianglesCount       = settings.ChunkTrianglesCount;
            TriangleIndicesCount = settings.ChunkIndicesCount;
        }
    }
}
