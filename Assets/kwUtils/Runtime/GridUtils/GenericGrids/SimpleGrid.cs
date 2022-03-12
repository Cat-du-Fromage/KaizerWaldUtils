using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.SEIDInfo;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    public class GenericGrid<T>
    where T : struct
    {
        protected int CellSize;
        protected float HalfCell;
        protected int GridWidth;
        protected int GridHeight;
        
        protected int2 MapWidthHeight;
        protected int2 GridBounds;
        
        public T[] GridArray;
        
        public GenericGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
            CellSize = cellSize;
            HalfCell = cellSize / 2f;
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
        
        public GenericGrid(in int2 mapSize, int cellSize)
        {
            CellSize = cellSize;
            HalfCell = cellSize / 2f;
            
            MapWidthHeight = mapSize;
            
            GridWidth = mapSize.x / cellSize;
            GridHeight = mapSize.y / cellSize;
            
            GridBounds = new int2(GridWidth, GridHeight);
            
            GridArray = new T[GridWidth * GridHeight];
        }

        //==============================================================================================================
        //ARRAY MANIPULATION
        //==============================================================================================================

        //Update all the array at once
        public virtual void CopyFrom(T[] otherArray)
        {
            otherArray.CopyTo((Span<T>) GridArray);
        }
        
        public Vector3 GetCellCenter(int index)
        {
            float2 cellCoord = index.GetXY2(GridBounds.x) * CellSize + new float2(HalfCell, HalfCell);
            return new Vector3(cellCoord.x,0,cellCoord.y);
        }
        
        public virtual void SetValue(int index, T value)
        {
            GridArray[index] = value;
        }

        public T GetValue(int index) => GridArray[index];


        //==============================================================================================================
        //Adaptation to an other Grid with different Cell
        //==============================================================================================================

        public void SetFromAnOtherGrid<U>(GenericGrid<U> otherGrid, int index) 
        where U : struct
        {
            int cellSizeOtherGrid = otherGrid.CellSize;

            if (cellSizeOtherGrid < this.CellSize)
            {
                int numCellAffected = this.CellSize / cellSizeOtherGrid;
                
                
            }

            return;
        }
    }
}
