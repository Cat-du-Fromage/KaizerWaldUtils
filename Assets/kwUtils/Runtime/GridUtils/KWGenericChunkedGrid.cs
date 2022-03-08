using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWChunk;
using static KWUtils.KWmath;
using float2 = Unity.Mathematics.float2;

namespace KWUtils.KWGenericGrid
{
    /// <summary>
    /// CAREFUL ALL SIZE must be POW 2!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChunkedGrid<T>
    where T : struct
    {
        //Need this in order to get value from world position
        private int2 mapWidthHeight;

        private int chunkSize;
        private int2 chunkWidthHeight;

        private int cellSize;
        private int2 cellWidthHeight;

        private Dictionary<int, T[]> chunkDictionary;
        private T[] arrayGrid; //cell Independant From the Chunk
        
        //TODO : Try Make a Composition, instead of arrayGrid => SimpleGrid<T>

        private float2 cachedCellCenter = float2.zero; //Use for SetValue

        private void BaseConstructor(int mapWidth, int mapHeight, int chunksSize, int cellsSize)
        {
            this.chunkSize = chunksSize;
            this.cellSize = cellsSize;
            mapWidthHeight = new int2(mapWidth, mapHeight);
            chunkWidthHeight = mapWidthHeight / chunkSize;
            cellWidthHeight = cellSize is 1 ? mapWidthHeight : mapWidthHeight / cellSize ;
            arrayGrid = new T[cmul(cellWidthHeight)];
        }
        
        public ChunkedGrid(int mapWidth, int mapHeight, int chunkSize, int cellSize = 1)
        {
            BaseConstructor(mapWidth, mapHeight, chunkSize, cellSize);

            chunkDictionary = arrayGrid.GetGridValueOrderedByChunk(new GridData(chunkSize, cellWidthHeight));
        }
        
        
        public ChunkedGrid(int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null)
        {
            BaseConstructor(mapSize.x, mapSize.y, chunkSize, cellSize);

            providerFunction?.Invoke()?.CopyTo((Memory<T>) arrayGrid);
            chunkDictionary = arrayGrid.GetGridValueOrderedByChunk(new GridData(chunkSize, cellWidthHeight));
        }

        /// <summary>
        /// Accessors
        /// </summary>
        public T[] ArrayGrid => arrayGrid;



        /// <summary>
        /// Get values contained in the chunk
        /// </summary>
        public T[] this[int index] => chunkDictionary[index];
        public T[] this[int x, int y] => chunkDictionary[y*chunkWidthHeight.x+x];
        public T[] this[int2 coord] => chunkDictionary[coord.y * chunkWidthHeight.x + coord.x];
        
        
        /// <summary>
        /// Get World position of the chunk
        /// </summary>
        /// <param name="index"></param>
        /// <returns>chunk's center's world position</returns>
        public Vector3 ChunkCenterAt(int index)
        {
            (int x, int z) = index.GetXY(chunkWidthHeight.x); //we offset by 1,0,1 so we just remove the last one
            Vector3 pointPosition = (new Vector3(x, 0, z) * chunkSize) + (Vector3.one * (chunkSize / 2f)); //* cellBound not needed this time since value = Vector3(1,1,1)
            return pointPosition.Flat();
        }


        // Array Grid : Get Value
        public Vector3 CellCenterAt(int index)
        {
            (int x, int z) = index.GetXY(cellWidthHeight.x);
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (cellSize / 2f));
            return pointPosition.Flat();
        }
        //==============================================================================================================
        //Get Values given coords
        //==============================================================================================================
        public T ArrayValueAt(int x, int y) => arrayGrid[y * chunkWidthHeight.x + x];
        public T ArrayValueAt(in int2 coord) => arrayGrid[coord.y * chunkWidthHeight.x + coord.x];
        public T ArrayValueFromWorldPosition(in Vector3 position) => arrayGrid[position.GetIndexFromPosition(mapWidthHeight, cellSize)];

        
        // Cell Grid : Set Value
        public void SetValue(int x, int y, T value)
        {
            arrayGrid[y * chunkWidthHeight.x + x] = value;
            
            cachedCellCenter = new float2(x + 0.5f, y + 0.5f);
            int chunkIndex = cachedCellCenter.GetIndexFromPosition(mapWidthHeight, chunkSize);
            int2 chunkCoord = chunkIndex.GetXY2(mapWidthHeight.x);
            int chunkCellIndex = cachedCellCenter.GetIndexFromPositionOffseted(chunkSize, cellSize, chunkCoord * chunkSize);
        }

        public void SetValueFromPosition(Vector3 position, T value)
        {
            arrayGrid[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
