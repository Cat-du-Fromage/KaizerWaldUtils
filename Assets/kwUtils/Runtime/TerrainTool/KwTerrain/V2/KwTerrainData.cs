using static Unity.Mathematics.math;
using int2 = Unity.Mathematics.int2;

namespace KWUtils.KwTerrain
{
    public readonly struct KwTerrainData
    {
        public readonly int ChunkSize;
        public readonly int2 TerrainSizeXZ;
        public readonly int2 NumChunkXZ;
        
        public readonly int2 TerrainVerticesXZ; //: (TerrainNumQuadsXZ + int2(1,1))
        public readonly int2 ChunkVerticesXZ; //: (ChunkNumQuadsXZ + int2(1,1))

        public KwTerrainData(in int2 terrainSizeXZ, int chunkSIze)
        {
            ChunkSize = ceilpow2(chunkSIze);
            TerrainSizeXZ = ceilpow2(terrainSizeXZ);
            
            while (ChunkSize > cmin(TerrainSizeXZ))
            {
                if (ChunkSize == 1) break;
                ChunkSize >>= 1;
            }
            
            NumChunkXZ = max(1,TerrainSizeXZ >> floorlog2(ChunkSize));;
            TerrainVerticesXZ = TerrainSizeXZ + int2(1);
            ChunkVerticesXZ = NumChunkXZ + int2(1);
        }
        
        //if(any(TerrainSizeXZ > ))
    }
}
