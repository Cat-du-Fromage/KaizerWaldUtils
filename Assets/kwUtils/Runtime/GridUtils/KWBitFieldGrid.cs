using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using static KWUtils.KWChunk;
using static KWUtils.KWmath;

namespace KWUtils
{
    public class ChunkedBitFieldGrid
    {
        //Need this in order to get value from world position
        private int2 mapWidthHeight;

        private int chunkSize;
        private float halfChunk;
        private int2 chunkWidthHeight;

        private int cellSize;
        private float halfCell;
        private int2 cellWidthHeight;

        private int cellPerChunkWidth;

        private BitField64[] chunkBitfield;
        //private Dictionary<int, BitField64> chunkBitfield;
        private readonly bool[] gridArray; //cell Independant From the Chunk

        //private Vector3[] chunkCenters;
        
        public ChunkedBitFieldGrid(int mapWidth, int mapHeight, int cellSize = 1)
        {
            chunkSize = (int)sqrt(64) * cellSize;
            halfChunk = chunkSize / 2f;
            
            this.cellSize = cellSize;
            halfCell = cellSize / 2f;

            cellPerChunkWidth = chunkSize / cellSize;
            
            mapWidthHeight = new int2(mapWidth, mapHeight);
            chunkWidthHeight = mapWidthHeight / chunkSize;
            
            cellWidthHeight = cellSize is 1 ? mapWidthHeight : mapWidthHeight / cellSize ;

            gridArray = new bool[cmul(cellWidthHeight)];
            
            int capacity = cmul(chunkWidthHeight);
            chunkBitfield = new BitField64[capacity];
            
            //chunkBitfield = new Dictionary<int, BitField64>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                chunkBitfield[i] = new BitField64();
                //chunkBitfield.Add(i, new BitField64());
            }
        }
        
        
        public ChunkedBitFieldGrid(int2 mapSize, int cellSize = 1)
        {
            chunkSize = (int)sqrt(64) * cellSize;
            halfChunk = chunkSize / 2f;
            
            this.cellSize = cellSize;
            halfCell = cellSize / 2f;
            
            cellPerChunkWidth = chunkSize / cellSize;
            
            mapWidthHeight = mapSize;
            chunkWidthHeight = mapWidthHeight / chunkSize;
            
            cellWidthHeight = cellSize is 1 ? mapWidthHeight : new int2(mapWidthHeight / cellSize);
            
            gridArray = new bool[cmul(cellWidthHeight)];

            int capacity = cmul(chunkWidthHeight);
            chunkBitfield = new BitField64[capacity];
            //chunkBitfield = new Dictionary<int, BitField64>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                chunkBitfield[i] = new BitField64();
                //chunkBitfield.Add(i, new BitField64());
            }
        }

        public int ChunkLength => chunkBitfield.Length;
        public int GridLength => gridArray.Length;
        //==============================================================================================================
        /// <summary>
        /// From Cell coordinate : Get the cell Index Inside a Chunk
        /// Index Correspond to the CHUNK's index (0->63), not the full grid 
        /// </summary>
        //==============================================================================================================
        public int GetChunkIndexAtCoord(int x, int y)
        {
            float2 cellCenter = float2(x + halfCell, y + halfCell);
            return cellCenter.GetIndexFromPosition(mapWidthHeight, chunkSize);
        }

        //==============================================================================================================
        /// <summary>
        /// From World Position : Get the cell Index Inside a Chunk
        /// Index Correspond to the CHUNK's index not the full grid (0->63)
        /// </summary>
        //==============================================================================================================
        public int GetChunkIndexAtPosition(Vector3 position) => position.XZ().GetIndexFromPosition(mapWidthHeight, chunkSize);
        public int GetChunkIndexAtPosition(float3 position) => position.xz.GetIndexFromPosition(mapWidthHeight, chunkSize);
        
        //==============================================================================================================
        /// <summary>
        /// Get The Center Of a Chunk Given an index
        /// </summary>
        //==============================================================================================================
        public Vector3 GetChunkCenterAt(int chunkIndex)
        {
            (int x, int z) = chunkIndex.GetXY(chunkWidthHeight.x);
            Vector3 pointPosition = (new Vector3(x, 0, z) * chunkSize) + (Vector3.one * halfChunk);
            return pointPosition.Flat();
        }
        
        //==============================================================================================================
        /// <summary>
        /// Get Cell Center given an Index
        /// Index Is from world grid ! not the index inside a chunk
        /// </summary>
        //==============================================================================================================
        public Vector3 GetCellCenterAt(int cellIndex)
        {
            (int x, int z) = cellIndex.GetXY(cellWidthHeight.x);
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (halfCell));
            return pointPosition.Flat();
        }
        
        //==============================================================================================================
        /// <summary>
        /// Get Cell Center given an Index
        /// Index Is from Chunk Grid
        /// </summary>
        //==============================================================================================================
        public Vector3 GetCellCenterFromChunkIndexAt(int chunkIndex, int cellIndex)
        {
            int2 chunkCoord = chunkIndex.GetXY2(chunkWidthHeight.x);
            int2 cellCoord = cellIndex.GetXY2(cellPerChunkWidth);
            
            float2 chunkOffset = float2(chunkCoord * chunkSize);
            float2 centerCell = (cellCoord * cellSize + float2(halfCell)) + chunkOffset; // OK
            return new Vector3(centerCell.x,0,centerCell.y);
        }
        
        //==============================================================================================================
        /// <summary>
        /// From Chunk Grid : Get Value
        /// Without Chunk indices
        /// </summary>
        //==============================================================================================================
        public bool GetChunkCellValueAt(int chunkIndex, int index)
        {
            return chunkBitfield[chunkIndex].IsSet(index);
        }
        
        public bool GetChunkCellValueAt(int chunkIndex, in int2 coord)
        {
            return chunkBitfield[chunkIndex].IsSet(mad(coord.y, chunkWidthHeight.x, coord.x));
        }

        public bool GetChunkCellValueAt(int chunkIndex, int x, int y)
        {
            return chunkBitfield[chunkIndex].IsSet(mad(y, chunkWidthHeight.x, x));
        }

        //==============================================================================================================
        /// <summary>
        /// From Grid : Get Value
        /// Without Chunk indices
        /// </summary>
        //==============================================================================================================
        public bool GetValueAt(int x, int y) => gridArray[y * cellWidthHeight.x + x];
        public bool GetValueAt(in int2 coord) => gridArray[coord.y * cellWidthHeight.x + coord.x];
        public bool GetValueFromWorldPosition(Vector3 position) => gridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
        
        
        //Retrieve bot chunk Index and the cell Index from a coord
        private (int, int) GetChunkCellIndexFromCellArray(float x, float y, float offset = 0)
        {
            float2 centerCell = float2(mad(x,cellSize,offset), mad(y,cellSize,offset)); // OK
            //UnityEngine.Debug.Log($"centerCell : {centerCell}");
            int chunkIndex = centerCell.GetIndexFromPosition(mapWidthHeight, chunkSize);
            
            int2 chunkCoord = chunkIndex.GetXY2(chunkWidthHeight.x);
            int chunkCellIndex = centerCell.GetIndexFromPositionOffseted(int2(chunkSize), cellSize, chunkCoord * chunkSize);
            
            return (chunkIndex, chunkCellIndex);
        }
        
        // Cell Grid : Set Value
        public void SetValue(int x, int y, bool value)
        {
            gridArray[y * cellWidthHeight.x + x] = value;
            //CHECK THIS
            //UnityEngine.Debug.Log($"position = {float3(x,0,y)};");
            (int chunkIndex, int chunkCellIndex) = GetChunkCellIndexFromCellArray(x, y, halfCell);
            //UnityEngine.Debug.Log($"chunkIndex = {chunkIndex}; chunkCellIndex = {chunkCellIndex}");

            chunkBitfield[chunkIndex].SetBits(chunkCellIndex, value);
            //UnityEngine.Debug.Log(chunkBitfield[chunkIndex].IsSet(chunkCellIndex));
        }

        public void SetValueFromPosition(Vector3 position, bool value)
        {
            gridArray[position.GetIndexFromPosition(cellWidthHeight, cellSize)] = value;
            //CHECK THIS
            
            (int chunkIndex, int chunkCellIndex) = GetChunkCellIndexFromCellArray((int)position.x, (int)position.z);
            UnityEngine.Debug.Log($"chunkIndex = {chunkIndex}; chunkCellIndex = {chunkCellIndex}");
            chunkBitfield[chunkIndex].SetBits(chunkCellIndex, value);
        }
    }
}