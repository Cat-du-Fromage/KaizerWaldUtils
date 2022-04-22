using static Unity.Mathematics.math;
using int2 = Unity.Mathematics.int2;

namespace KWUtils.KwTerrain
{
    public readonly struct KwTerrainData
    {
        public readonly int2 ChunkSize;
        public readonly int2 TerrainSizeXZ;
        public readonly int2 NumChunkXZ;
        
        public readonly int2 TerrainVerticesXZ; //: (TerrainNumQuadsXZ + int2(1,1))
        public readonly int2 ChunkVerticesXZ; //: (ChunkNumQuadsXZ + int2(1,1))

        public KwTerrainData(in int2 terrainSizeXZ, int2 chunkSIze)
        {
            ChunkSize = ceilpow2(chunkSIze);
            TerrainSizeXZ = ceilpow2(terrainSizeXZ);
            
            //Check by X
            if (ChunkSize.x > TerrainSizeXZ.x || ChunkSize.y > TerrainSizeXZ.y)
            {
                while (ChunkSize.x > TerrainSizeXZ.x)
                {
                    if (ChunkSize.x == 1) break;
                    ChunkSize.x >>= 1;
                }
                while (ChunkSize.y > TerrainSizeXZ.y)
                {
                    if (ChunkSize.y == 1) break;
                    ChunkSize.y >>= 1;
                }
            }
            
/*
            while (ChunkSize > cmin(TerrainSizeXZ))
            {
                if (ChunkSize.x == 1) break;
                ChunkSize >>= 1;
            }
            */

            //NumChunkXZ.x = max(1,TerrainSizeXZ.x >> floorlog2(ChunkSize.x));
            //NumChunkXZ.y = max(1,TerrainSizeXZ.y >> floorlog2(ChunkSize.y));

            NumChunkXZ = TerrainSizeXZ / ChunkSize;
            TerrainVerticesXZ = TerrainSizeXZ + int2(1);
            ChunkVerticesXZ = NumChunkXZ + int2(1);
        }
    }
}
