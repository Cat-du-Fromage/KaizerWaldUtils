using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using KWUtils.KWGenericGrid;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;

using static KWUtils.KWmath;

namespace KWUtils.Tests
{
    public partial class ChunkGridTest
    {
        private GenericGrid<int> AdaptGridTest_Small;
        private GenericGrid<int> AdaptGridTest_Big;
        //==============================================================================================================
        //CHECK : ADAPT CHUNK TO AN OTHER
        //==============================================================================================================
        
        //Test Case 2 : ChunkCellIndexToGridIndex
        //==============================================================================================================
        [Test]
        public void AdaptGrid_Test_CellRatio()
        {
            AdaptGridTest_Small = new GenericGrid<int>(mapSize, 2, (index) => index);
            AdaptGridTest_Big = new GenericGrid<int>(mapSize, 4, (i) => i);

            GridData smallData = AdaptGridTest_Small.GridData;
            GridData bigData = AdaptGridTest_Big.GridData;
            
            Assert.That(smallData.CellSize*smallData.CellSize, Is.EqualTo(Sq(bigData.CellSize/smallData.CellSize)));
        }
        
        [Test]
        public void AdaptGrid_Test_FakeChunk()
        {
            AdaptGridTest_Small = new GenericGrid<int>(mapSize, 2, (index) => index);
            AdaptGridTest_Big = new GenericGrid<int>(mapSize, 4, (i) => i);

            GridData smallData = AdaptGridTest_Small.GridData;
            GridData bigData = AdaptGridTest_Big.GridData;

            GridData fakeChunk = new GridData(mapSize, 2, 4);
            
            Assert.That(fakeChunk.NumChunkXY.x, Is.EqualTo(2));
            Assert.That(fakeChunk.NumCellInChunkX, Is.EqualTo(2));
            Assert.That(fakeChunk.NumCellXY.x, Is.EqualTo(4));
            //Assert.That(smallData.CellSize*smallData.CellSize, Is.EqualTo(Sq(bigData.CellSize/smallData.CellSize)));
        }
        
        [Test]
        public void AdaptGrid_Test_BigToSmall()
        {
            AdaptGridTest_Small = new GenericGrid<int>(mapSize, 2, (index) => index);
            AdaptGridTest_Big = new GenericGrid<int>(mapSize, 4, (i) => i);

            GridData smallData = AdaptGridTest_Small.GridData;
            GridData bigData = AdaptGridTest_Big.GridData;

            int lengthBig = AdaptGridTest_Big.GridArray.Length;
            int lengthSmall = AdaptGridTest_Small.GridArray.Length;

            int ratio = bigData.CellSize / smallData.CellSize;

            //Conversion Grid must be LengthBig * Ratio^2
            int expectedLength = (ratio * ratio) * lengthBig;

            //Assert.AreEqual(lengthBig, expectedLength);
            Assert.That(lengthSmall, Is.EqualTo(expectedLength));
        }
        
        [Test]
        public void AdaptGrid_Test_ModifiedCell()
        {
            AdaptGridTest_Small = new GenericGrid<int>(mapSize, 2, (index) => index);
            AdaptGridTest_Big = new GenericGrid<int>(mapSize, 4, (i) => i);

            GridData smallData = AdaptGridTest_Small.GridData;
            GridData bigData = AdaptGridTest_Big.GridData;

            int[] expectedGrid = new[]
            {
                0, 0, 1, 1,
                0, 0, 1, 1,
                2, 2, 3, 3,
                2, 2, 3, 3
            };
            //using NativeArray<int> adaptedGrid = AdaptGridTest_Big.NativeAdaptGrid(AdaptGridTest_Small);
            int[] adaptedGrid = AdaptGridTest_Big.AdaptGrid(AdaptGridTest_Small);
            Assert.That(expectedGrid, Is.EqualTo(adaptedGrid));
        }
        
        [Test]
        public void AdaptGrid_Test_ModifiedCell2()
        {
            AdaptGridTest_Small = new GenericGrid<int>(mapSize, 1, (index) => index);
            AdaptGridTest_Big = new GenericGrid<int>(mapSize, 4, (i) => i);

            GridData smallData = AdaptGridTest_Small.GridData;
            GridData bigData = AdaptGridTest_Big.GridData;

            int[] expectedGrid = new[]
            {
                0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 1, 1, 1, 1,
                0, 0, 0, 0, 1, 1, 1, 1,
                2, 2, 2, 2, 3, 3, 3, 3,
                2, 2, 2, 2, 3, 3, 3, 3,
                2, 2, 2, 2, 3, 3, 3, 3,
                2, 2, 2, 2, 3, 3, 3, 3,
            };
            //using NativeArray<int> adaptedGrid = AdaptGridTest_Big.NativeAdaptGrid(AdaptGridTest_Small);
            int[] adaptedGrid = AdaptGridTest_Big.AdaptGrid(AdaptGridTest_Small);
            Assert.That(expectedGrid, Is.EqualTo(adaptedGrid));
        }
    }
}