using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.SEIDInfo;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils.KWGenericGrid
{
    public class GenericGrid<T>
    where T : struct
    {
        protected readonly int CellSize;
        protected readonly int2 MapXY;
        protected readonly int2 NumCellXY;

        public readonly T[] GridArray;
        public event Action OnGridChange;
        
        //==============================================================================================================
        //CONSTRUCTOR
        //==============================================================================================================
        public GenericGrid(in int2 mapSize, int cellSize, Func<int, T> createGridObject)
        {
            CellSize = cellSize;
            MapXY = ceilpow2(mapSize);

            NumCellXY = mapSize / cellSize;
            GridArray = new T[NumCellXY.x * NumCellXY.y];
            
            //Init Grid
            for (int i = 0; i < GridArray.Length; i++)
            {
                GridArray[i] = createGridObject(i);
            }
        }
        
        public GenericGrid(in int2 mapSize, int cellSize)
        {
            CellSize = cellSize;

            MapXY = ceilpow2(mapSize);
            
            NumCellXY = mapSize / cellSize;
            GridArray = new T[NumCellXY.x * NumCellXY.y];
        }
        
        public virtual GridData GridData => new GridData(MapXY, CellSize);
        
        //Clear Events
        public virtual void ClearEvents()
        {
            if (OnGridChange == null) return;
            foreach (Delegate action in OnGridChange.GetInvocationList())
            {
                OnGridChange -= (Action)action;
            }
        }

        //==============================================================================================================
        //CELLS INFORMATION
        //==============================================================================================================

        public Vector3 GetCellCenter(int index)
        {
            float2 cellCoord = index.GetXY2(NumCellXY.x) * CellSize + new float2(CellSize/2f);
            return new Vector3(cellCoord.x,0,cellCoord.y);
        }
        
        //==============================================================================================================
        //ARRAY MANIPULATION
        //==============================================================================================================
        
        public virtual void CopyFrom(T[] otherArray)
        {
            otherArray.CopyTo((Span<T>) GridArray);
        }
        
        public T this[int cellIndex]
        {
            get => GridArray[cellIndex];
            set => SetValue(cellIndex, value);
        }
        
        public T GetValue(int index)
        {
            return GridArray[index];
        }

        public virtual void SetValue(int index, T value)
        {
            GridArray[index] = value;
            OnGridChange?.Invoke();
        }
        
        //Operation from World Position
        //==============================================================================================================
        public int IndexFromPosition(in Vector3 position)
        {
            return position.XZ().GetIndexFromPosition(MapXY, CellSize);
        }

        //==============================================================================================================
        //Adaptation to an other Grid with different Cell
        //==============================================================================================================

        //AdaptGrid(GenericGrid<T> grid)
        public void AdaptGrid<T1>(GenericGrid<T1> otherGrid)
        where T1 : struct
        {
            T1[] beforeConversion = otherGrid.GridArray;
            T1[] afterConversion;
            
            if (otherGrid.CellSize > CellSize)
            {
                //We Receive Grid With bigger Cells!
                afterConversion = new T1[GridArray.Length];
                return;
            }
            else if (otherGrid.CellSize < CellSize)
            {
                //We Receive Grid With smaller Cells!
                afterConversion = new T1[2];
            }
            else
            {
                //Return otherGrid.GridArray;
            }
        }
    }
}
