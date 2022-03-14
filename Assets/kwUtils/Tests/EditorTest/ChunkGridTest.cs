using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using KWUtils.KWGenericGrid;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;

using static KWUtils.KWmath;

namespace KWUtils.Tests
{
    [TestFixture]
    public class ChunkGridTest
    {
        private int2 mapSize = new int2(8, 8);

        private const int bound1 = 4;
        private const int bound2 = 8;
        
        //Case 1
        private const int cellSize = 1;
        private const int chunkSize = 2;
        
        //Case 2
        private const int cellSize2 = 2;
        private const int chunkSize2 = 4;

        private GenericChunkedGrid<int> chunkedGrid;

        //GridArray
        //Case 1 : Cell Size = 1
        private int[] grid8X8Cell1;
        private Dictionary<int, int[]> chunk4X4Cell1 = new Dictionary<int, int[]>(16);
        
        //Case 2 : Cell Size = 2
        private int[] grid8X8Cell2;
        private Dictionary<int, int[]> chunk2X2Cell1 = new Dictionary<int, int[]>(4);
        
        //ChunkArray

        private int[] PopulateArray(int[] array, int cellSize)
        {
            array = new int[cmul(mapSize/cellSize)];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            return array;
        }
        
        [SetUp]
        public void SetUp()
        {
            grid8X8Cell1 = PopulateArray(grid8X8Cell1, cellSize);
            grid8X8Cell2 = PopulateArray(grid8X8Cell2, cellSize2);
        }
        
        //==============================================================================================================
        //GridData
        //==============================================================================================================

        [TestCase(chunkSize,cellSize, ExpectedResult = 16)]
        [TestCase(chunkSize2,cellSize2, ExpectedResult = 4)]
        public int ChunkGridTest_Are_Parameters_TotalChunk_OK(int chunkSize,int cellSize)
        {
            GridData gridData = new GridData(cellSize, chunkSize, mapSize);
            return gridData.TotalChunk;
        }
        
        [TestCase(chunkSize,cellSize, ExpectedResult = 4)]
        [TestCase(chunkSize2,cellSize2, ExpectedResult = 4)]
        public int ChunkGridTest_Are_Parameters_TotalCellInChunk_OK(int chunkSize,int cellSize)
        {
            GridData gridData = new GridData(cellSize, chunkSize, mapSize);
            return gridData.TotalCellInChunk;
        }
        
        //==============================================================================================================
        //Chunk Parameters
        //==============================================================================================================
        [Test]
        public void ChunkGridTest_Is_DictionaryCount_Case1()
        {
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize, cellSize, (index) => index);
            Assert.AreEqual(16, chunkedGrid.ChunkDictionary.Count);
            Assert.AreEqual(chunkedGrid.GridData.TotalChunk, chunkedGrid.ChunkDictionary.Count);
        }
        
        [Test]
        public void ChunkGridTest_Is_DictionaryCount_Case2()
        {
            mapSize = new int2(8, 8);
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize2, cellSize2, (index) => index);
            Assert.AreEqual(4, chunkedGrid.ChunkDictionary.Count);
            //Assert.AreEqual(chunkedGrid.GridData.TotalChunk, chunkedGrid.ChunkDictionary.Count);
        }
        
        //==============================================================================================================
        //CHECK : Ordered Chunk
        //==============================================================================================================
        
        [Test]
        public void ChunkGridTest_SimplePasses_CaseCell1()
        {
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize, cellSize, (index) => index);
            Assert.AreEqual(grid8X8Cell1, chunkedGrid.GridArray);
        }
        
        [Test]
        public void ChunkGridTest_SimplePasses_CaseCell2()
        {
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize2, cellSize2, (index) => index);
            Assert.AreEqual(grid8X8Cell2, chunkedGrid.GridArray);
        }
    }
}

