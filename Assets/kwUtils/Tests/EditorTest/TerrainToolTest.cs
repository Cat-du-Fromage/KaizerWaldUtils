using System.Collections;
using System.Collections.Generic;
using KWUtils.KwTerrain;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils.Tests
{
    [TestFixture]
    public class TerrainToolTest
    {
        [Test]
        public void ChunkGridTest_Is_DictionaryCount_Case1()
        {
            KwTerrainData data = new KwTerrainData(int2(8), 2);
        }
        /*
        [TestCase(chunkSize,cellSize, ExpectedResult = 4)]
        [TestCase(chunkSize2,cellSize2, ExpectedResult = 4)]
        public int ChunkGridTest_Are_Parameters_TotalCellInChunk_OK(int chunkSize,int cellSize)
        {
            GridData gridData = new GridData(mapSize, cellSize, chunkSize);
            return gridData.TotalCellInChunk;
        }
        */
    }
}