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
    public partial class ChunkGridTest
    {
        private int2 mapSize = new int2(8, 8);

        private const int bound1 = 4;
        private const int bound2 = 8;
        
        //Case 1 : grid Cell = 8x8; grid chunk = 4x4
        private const int cellSize = 1;
        private const int chunkSize = 2;
        
        //Case 2 : grid Cell = 4x4; grid chunk = 2x2
        private const int cellSize2 = 2;
        private const int chunkSize2 = 4;
        
        //Case 3 : grid Cell = 4x4; grid chunk = 2x2
        private int2 mapSizeCase3 = new int2(16, 16);
        private const int cellSize3 = 4;
        private const int chunkSize3 = 8;

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
            GridData gridData = new GridData(mapSize,cellSize, chunkSize);
            return gridData.TotalChunk;
        }
        
        [TestCase(chunkSize,cellSize, ExpectedResult = 4)]
        [TestCase(chunkSize2,cellSize2, ExpectedResult = 4)]
        public int ChunkGridTest_Are_Parameters_TotalCellInChunk_OK(int chunkSize,int cellSize)
        {
            GridData gridData = new GridData(mapSize, cellSize, chunkSize);
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
            Assert.AreEqual(chunkedGrid.GridData.TotalChunk, chunkedGrid.ChunkDictionary.Count);
        }
        
        //==============================================================================================================
        //CHECK : Ordered Chunk
        //==============================================================================================================
        
        //Test : arrays are init correctly
        //===================================
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
        
        //Test if Partition (ordered Array is Correct)
        //===============================================================
        [Test]
        public void ChunkGridTest_Ordered_CaseCell1()
        {
            int[] chunk0 = new int[] {0, 1, 8, 9};
            int[] chunk5 = new int[] {18, 19, 26, 27};
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize, cellSize, (index) => index);
            Assert.AreEqual(chunk0, chunkedGrid.ChunkDictionary[0]);
            Assert.AreEqual(chunk5, chunkedGrid.ChunkDictionary[5]);
        }
        
        [Test]
        public void ChunkGridTest_Ordered_CaseCell2()
        {
            int[] chunk0 = new int[] {0,1,4,5};
            int[] chunk1 = new int[] {2,3,6,7};
            int[] chunk2 = new int[] {8,9,12,13};
            int[] chunk3 = new int[] {10,11,14,15};
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize2, cellSize2, (index) => index);
            Assert.AreEqual(chunk0, chunkedGrid.ChunkDictionary[0]);
            Assert.AreEqual(chunk1, chunkedGrid.ChunkDictionary[1]);
            Assert.AreEqual(chunk2, chunkedGrid.ChunkDictionary[2]);
            Assert.AreEqual(chunk3, chunkedGrid.ChunkDictionary[3]);
        }
        
        //Test with a bigger ration
        [Test]
        public void ChunkGridTest_Ordered_CaseCell3()
        {
            int[] chunk0 = new int[] {0,1,4,5};
            int[] chunk1 = new int[] {2,3,6,7};
            int[] chunk2 = new int[] {8,9,12,13};
            int[] chunk3 = new int[] {10,11,14,15};
            chunkedGrid = new GenericChunkedGrid<int>(mapSizeCase3, chunkSize3, cellSize3, (index) => index);
            Assert.AreEqual(chunk0, chunkedGrid.ChunkDictionary[0]);
            Assert.AreEqual(chunk1, chunkedGrid.ChunkDictionary[1]);
            Assert.AreEqual(chunk2, chunkedGrid.ChunkDictionary[2]);
            Assert.AreEqual(chunk3, chunkedGrid.ChunkDictionary[3]);
        }
        
        //Test Case 1 : ChunkCellIndexToGridIndex
        //===================================
        [TestCase(0, 0, ExpectedResult = 0)]
        [TestCase(0, 1, ExpectedResult = 1)]
        [TestCase(0, 2, ExpectedResult = 8)]
        [TestCase(0, 3, ExpectedResult = 9)]
        
        [TestCase(5, 0, ExpectedResult = 18)]
        [TestCase(5, 1, ExpectedResult = 19)]
        [TestCase(5, 2, ExpectedResult = 26)]
        [TestCase(5, 3, ExpectedResult = 27)]
        
        [TestCase(7, 0, ExpectedResult = 22)]
        [TestCase(7, 1, ExpectedResult = 23)]
        [TestCase(7, 2, ExpectedResult = 30)]
        [TestCase(7, 3, ExpectedResult = 31)]
        
        [TestCase(10, 0, ExpectedResult = 36)]
        [TestCase(10, 1, ExpectedResult = 37)]
        [TestCase(10, 2, ExpectedResult = 44)]
        [TestCase(10, 3, ExpectedResult = 45)]
        //ALWAYS TEST last index!
        [TestCase(15, 0, ExpectedResult = 54)]
        [TestCase(15, 1, ExpectedResult = 55)]
        [TestCase(15, 2, ExpectedResult = 62)]
        [TestCase(15, 3, ExpectedResult = 63)]
        public int ChunkGridTest_ChunkCellIndexToGridIndex_CaseCell1(int chunkIndex, int cellInChunkIndex)
        {
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize, cellSize, (index) => index);
            return chunkIndex.GetGridCellIndexFromChunkCellIndex(chunkedGrid.GridData, cellInChunkIndex);
        }
        
        //Test Case 2 : ChunkCellIndexToGridIndex
        //===================================
        [TestCase(0, 0, ExpectedResult = 0)]
        [TestCase(0, 1, ExpectedResult = 1)]
        [TestCase(0, 2, ExpectedResult = 4)]
        [TestCase(0, 3, ExpectedResult = 5)]
        
        [TestCase(1, 0, ExpectedResult = 2)]
        [TestCase(1, 1, ExpectedResult = 3)]
        [TestCase(1, 2, ExpectedResult = 6)]
        [TestCase(1, 3, ExpectedResult = 7)]
        
        [TestCase(2, 0, ExpectedResult = 8)]
        [TestCase(2, 1, ExpectedResult = 9)]
        [TestCase(2, 2, ExpectedResult = 12)]
        [TestCase(2, 3, ExpectedResult = 13)]
        
        [TestCase(3, 0, ExpectedResult = 10)]
        [TestCase(3, 1, ExpectedResult = 11)]
        [TestCase(3, 2, ExpectedResult = 14)]
        [TestCase(3, 3, ExpectedResult = 15)]
        public int ChunkGridTest_ChunkCellIndexToGridIndex_CaseCell2(int chunkIndex, int cellInChunkIndex)
        {
            chunkedGrid = new GenericChunkedGrid<int>(mapSize, chunkSize2, cellSize2, (index) => index);
            return chunkIndex.GetGridCellIndexFromChunkCellIndex(chunkedGrid.GridData, cellInChunkIndex);
        }


    }
}

