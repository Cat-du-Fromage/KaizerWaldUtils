using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;

namespace KWUtils.Tests
{
    [TestFixture]
    public class ChunkGridTest
    {
        private int2 mapSize = new int2(4, 4);

        private const int bound1 = 4;
        private const int bound2 = 8;
        //Unitary Grid
        private int cellSize = 1;
        private int chunkSize = 2;
        
        //2X
        private int cellSize2 = 2;
        private int chunkSize2 = 4;

        private GenericChunkedGrid<bool> chunkedGrid;
        
        [SetUp]
        public void SetUp()
        { 
            mapSize = new int2(4, 4);
            cellSize = 1;
            chunkSize = 2;
            chunkedGrid = new GenericChunkedGrid<bool>(mapSize, chunkSize, cellSize);
        }
        
        // A Test behaves as an ordinary method
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(16)]
        public void ChunkGridTest_SimplePasses(int mapBound)
        {
            mapSize = new int2(mapBound, mapBound);
            chunkedGrid = new GenericChunkedGrid<bool>(mapSize, chunkSize, cellSize);
            Assert.That(chunkedGrid.GridArray, Has.All.EqualTo(false));
        }
        /*
        [TestCase(4, ExpectedResult=4)]
        [TestCase(8, ExpectedResult=4)]
        [TestCase(16, ExpectedResult=4)]
        public void ChunkGridTestSimplePasses(int mapBound)
        {
            mapSize = new int2(mapBound, mapBound);
            chunkedGrid = new GenericChunkedGrid<bool>(mapSize, chunkSize, cellSize);
            Assert.That(chunkedGrid.GridArray, Has.All.EqualTo(false));
        }
        */
    }
}

