using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KWUtils.KWGenericGrid
{

    /// <summary>
    /// CAREFUL ALL SIZE must be POW 2!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleGrid<T>
    {
        //Need this in order to get value from world position

        private readonly int cellSize;
        private readonly int gridWidth;
        private readonly int gridHeight;
        
        private readonly int2 mapWidthHeight;
        private readonly int2 gridBounds;
        
        public readonly T[] GridArray;

        public SimpleGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            GridArray = new T[gridWidth * gridHeight];
            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize, Func<SimpleGrid<T>, int2 , T> createGridObject)
        {
            this.cellSize = cellSize;
            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            gridBounds = new int2(gridWidth, gridHeight);

            GridArray = new T[gridWidth * gridHeight];
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(this, i.GetXY2(gridWidth));
            }
        }
        
        public SimpleGrid(int mapWidth, int mapHeight, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = new int2(mapWidth, mapHeight);
            
            gridWidth = mapWidth / cellSize;
            gridHeight = mapHeight / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            GridArray = new T[gridWidth * gridHeight];
        }
        
        public SimpleGrid(in int2 mapSize, int cellSize)
        {
            this.cellSize = cellSize;

            mapWidthHeight = mapSize;
            
            gridWidth = mapSize.x / cellSize;
            gridHeight = mapSize.y / cellSize;
            
            gridBounds = new int2(gridWidth, gridHeight);
            
            GridArray = new T[gridWidth * gridHeight];
        }

        public T[] GetGridArray => GridArray;
        public int GridLength => GridArray.Length;

        public int GetGridWidth => gridWidth;
        
        //Get Grid's Cell World Position
        public Vector3 GetCenterCellAt(int index)
        {
            (int x, int z) = index.GetXY(gridWidth);
            Vector3 pointPosition = (new Vector3(x, 0, z) * cellSize) + (Vector3.one * (cellSize / 2f));
            return pointPosition.Flat();
        }

        //Get Value
        //==============================================================================================================
        public T this[int index]
        {
            get => GridArray[index];
            set => GridArray[index] = value;
        }

        public T this[int x, int y]
        {
            get => GridArray[y * gridWidth + x];
            set => GridArray[y * gridWidth + x] = value;
        }

        public T this[in int2 coord]
        {
            get => GridArray[coord.y * gridWidth + coord.x];
            set => GridArray[coord.y * gridWidth + coord.x] = value;
        }

        
        
        //Operation from World Position
        //==============================================================================================================
        public int IndexFromPosition(in Vector3 position)
        {
            return position.XZ().GetIndexFromPosition(mapWidthHeight, cellSize);
        }
        
        public T GetValueFromWorldPosition(in Vector3 position)
        {
            return GridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)];
        }

        public void SetValueFromPosition(in Vector3 position, T value)
        {
            GridArray[position.GetIndexFromPosition(mapWidthHeight, cellSize)] = value;
        }
    }
}
