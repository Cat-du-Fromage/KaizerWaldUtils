using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public class GenericGrid<T>
    //where T : struct
    {
        protected int CellSize;
        protected int GridWidth;
        protected int GridHeight;
        
        protected int2 MapWidthHeight;
        protected int2 GridBounds;
        
        public T[] GridArray;
        
        public GenericGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            CellSize = cellSize;

            MapWidthHeight = mapSize;
            
            GridWidth = mapSize.x / cellSize;
            GridHeight = mapSize.y / cellSize;
            
            GridBounds = new int2(GridWidth, GridHeight);
            
            GridArray = new T[GridWidth * GridHeight];
            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i.GetXY2(GridWidth));
            }
        }
        //==============================================================================================================
        //ARRAY MANIPULATION
        //==============================================================================================================
        public T this[int index]
        {
            get => GridArray[index];
            set => GridArray[index] = value;
        }

        public T this[int x, int y]
        {
            get => GridArray[y * GridWidth + x];
            set => GridArray[y * GridWidth + x] = value;
        }

        public T this[in int2 coord]
        {
            get => GridArray[coord.y * GridWidth + coord.x];
            set => GridArray[coord.y * GridWidth + coord.x] = value;
        }
    }
}
