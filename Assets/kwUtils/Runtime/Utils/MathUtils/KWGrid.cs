using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KWUtils
{
    [Flags]
    public enum AdjacentCell : int
    {
        Top         = 1 << 0,
        Right       = 1 << 1,
        Left        = 1 << 2,
        Bottom      = 1 << 3,
        TopLeft     = 1 << 4,
        TopRight    = 1 << 5,
        BottomRight = 1 << 6,
        BottomLeft  = 1 << 7,
        None        = 1 << 8,
    }
    public static class KWGrid
    {
        /// <summary>
        /// Get position (in Int2) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the grid</param>
        /// <returns>Int2 Pos</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetXY2(int i, int w)
        {
            (int x, int y) = GetXY(i,w);
            return (new int2(x,y));
        }
        
        /// <summary>
        /// Get position (in Int, Int) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the Grid</param>
        /// <returns>Int X, Int Y(return in this order)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int,int) GetXY(int i, int w)
        {
            int y = i / w;
            int x = i - (y * w);
            return (x, y);
        }
        
        /// <summary>
        /// USE FOR VOXEL GENERATION TYPE 3D
        /// </summary>
        /// <param name="i"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int,int,int) GetXYZ(int i, int w)
        {
            int x = i % w;
            int y = (i % (w * w)) / w;
            int z = i / (w * w);
            return (x, y, z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 GetXYZ3(int i, int w)
        {
            (int x, int y, int z) = GetXYZ(i, w);
            return new int3(x, y, z);
        }
        
        /// <summary>
        /// Get array Index from Coord
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex(in int2 coord, int gridWidth)
        {
            return coord.y * gridWidth + coord.x;
        }
        
//=====================================
//START : MIN INDEX
//=====================================
        /// <summary>
        /// Find the index of the minimum value of an array
        /// </summary>
        /// <param name="dis">array containing float distance value from point to his neighbors</param>
        /// <param name="cellIndex">HashGrid indices</param>
        /// <returns>index of the closest point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<float> dis, in NativeArray<int> cellIndex)
        {
            float minVal = float.MaxValue;
            int minIndex = 0;
/*
            int midLength = dis.Length / 2;
            for (int i = 0; i < midLength; i++)
            {
                int j = dis.Length - (1 + i);
                int minIJ = dis[i] < dis[j] ? i : j;
                minIndex = minVal < dis[minIJ] ? minIJ : minIndex;// select(minIndex, minIJ, minVal < dis[minIJ]);
                minVal = dis[minIndex];
            }
            minIndex = select(midLength+1, minIndex, minVal < dis[midLength+1]);
*/
            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < minVal)
                {
                    minIndex = cellIndex[i];
                    minVal = dis[i];
                }
            }
            return minIndex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<int> dis, in NativeArray<int> cellIndex)
        {
            int val = int.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] >= val) continue;
                index = cellIndex[i];
                val = dis[i];
            }
            return index;
        }
//=====================================
// END : MIN INDEX
//=====================================
        
        //=====================================
        //HASHGRID : Cell Grid
        //You need to precompute the hashGrid to use the function
        //may need either : NativeArray<Position> PointInsideHashGrid OR NativeArray<ID> CellId containing the point
        //=====================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(float2 pointPos, int2 mapXY, int cellSize = 1)
        {
            float2 percents = pointPos / (mapXY * cellSize);
            percents = clamp(percents, 0, 1f);
            int2 xy =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1);
            return mad(xy.y, mapXY.x/cellSize, xy.x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPositionOffset(float2 pointPos, int2 mapXY, int cellSize = 1)
        {
            float2 offset = mapXY / new float2(2f);
            float2 percents = (pointPos + offset) / (mapXY * cellSize);
            percents = clamp(percents, 0, 1f);
            int2 xy =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1); // Cellsize not applied?!
            return mad(xy.y, mapXY.x/cellSize, xy.x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(float3 pointPos, int2 mapXY, int cellSize = 1)
        {
            float2 percents = pointPos.xz / (mapXY * cellSize);
            percents = clamp(percents, 0, 1f);
            int2 xy =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1);
            return mad(xy.y, mapXY.x/cellSize, xy.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(Vector3 pointPos, int2 mapXY, int cellSize = 1)
        {
            float2 percents = (float2)pointPos.xz() / (mapXY * cellSize);
            percents = clamp(percents, 0, 1f);
            int2 xy =  clamp((int2)floor(mapXY * percents), 0, mapXY - 1);
            return mad(xy.y, mapXY.x/cellSize, xy.x);
        }

        public static Vector3 GetCellCenterFromPosition(Vector3 positionInWorld, int2 mapXY, int cellSize = 1)
        {
            int index = GetIndexFromPosition((float2)positionInWorld.xz(), mapXY, cellSize);
            float2 cellCoord = GetXY2(index,mapXY.x/cellSize) * cellSize + new float2(cellSize/2f);
            return new Vector3(cellCoord.x,0,cellCoord.y);
        }
        
        public static Vector3 GetCellCenterFromIndex(int index, int2 mapXY, int cellSize = 1)
        {
            float2 cellCoord = GetXY2(index,mapXY.x/cellSize) * cellSize + new float2(cellSize/2f);
            return new Vector3(cellCoord.x,0,cellCoord.y);
        }


        /// <summary>
        /// Use to check around a cell (1 tile around with diagonal)
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="gridWidth"></param>
        /// <returns>number of cell to check; X Range; Y Range</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int numCell, int2 xRange, int2 yRange) CellGridRanges(int cellId, int2 gridBounds)
        {
            int2 cellCoord = GetXY2(cellId, gridBounds.x);
            bool corner = IsCellOnCorner(cellCoord, gridBounds);

            bool yOnEdge = IsCellOnEdge(cellCoord.y, gridBounds.y);
            bool xOnEdge = IsCellOnEdge(cellCoord.x, gridBounds.x);

            //check if on edge 0 : int2(0, 1) ; if not NumCellJob - 1 : int2(-1, 0)
            int2 OnEdge(int e) => select(int2(-1, 0), int2(0, 1), e == 0);
            
            int2 yRange = select(OnEdge(cellCoord.y), int2(-1, 1), !yOnEdge);
            int2 xRange = select(OnEdge(cellCoord.x), int2(-1, 1), !xOnEdge);
            int numCell = select(select(9, 6, yOnEdge || xOnEdge), 4, corner);
            
            return (numCell, xRange, yRange);
        }
        
//=====================================================================================================================
// IS CELL ON CORNER
//=====================================================================================================================
        
        /// <summary>
        /// Return if the cell is on one of the 4 corner of the grid
        /// </summary>
        /// <param name="xCellCoord">Cell : Position x on the grid</param>
        /// <param name="yCellCoord">Cell : Position y on the grid</param>
        /// <param name="gridWidth">Grid Width (x component)</param>
        /// <param name="gridHeight">Grid Height (y component)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int xCellCoord, int yCellCoord, int gridWidth, int gridHeight)
        {
            bool xCorner = xCellCoord == 0 || xCellCoord == gridWidth - 1;
            bool yCorner = yCellCoord == 0 || yCellCoord == gridHeight - 1;
            return xCorner && yCorner;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int xCellCoord, int yCellCoord, int2 gridBounds)
        {
            return IsCellOnCorner(xCellCoord,yCellCoord,gridBounds.x,gridBounds.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int2 cellCoord, int2 gridBounds)
        {
            return IsCellOnCorner(cellCoord.x, cellCoord.y, gridBounds);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int2 cellCoord, int gridWidth, int gridHeight)
        {
            return IsCellOnCorner(cellCoord.x, cellCoord.y,gridWidth,gridHeight);
        }
        
        /// <summary>
        /// Get if a cell is on a chosen Edge (X or Y)
        /// </summary>
        /// <param name="coord">coord (X or Y) you want to check</param>
        /// <param name="gridWidth"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnEdge(int coord, int gridWidth) => coord == 0 || coord == gridWidth - 1;


//=====================================================================================================================
// Methods Below are used for :
// Get the Index of any cells around them
//=====================================================================================================================
        /// <summary>
        /// Get Left Index of a point in a grid
        /// </summary>
        /// <param name="coords">coordinate of the point to check from</param>
        /// <param name="width">width of the grid</param>
        /// <returns>index of the left point, -1 means point is on corner</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLeftIndex(in int2 coords, int width)
        {
            return select(-1, mad(coords.y, width, coords.x) - 1, coords.x > 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRightIndex(in int2 coords, int width)
        {
            return select(-1, mad(coords.y, width, coords.x) + 1, coords.x < width - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomIndex(in int2 coords, int width)
        {
            return select(-1, mad(coords.y, width, coords.x) - width, coords.y > 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopIndex(in int2 coords, in int2 bounds)
        {
            return select(-1, mad(coords.y, bounds.x, coords.x) + bounds.x, coords.y < bounds.y - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopLeftIndex(in int2 coords, in int2 bounds)
        {
            bool inBound = coords.y < (bounds.y - 1) && coords.x > 0;
            return select(-1, mad(coords.y, bounds.x, coords.x) + bounds.x - 1, inBound);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopRightIndex(in int2 coords, in int2 bounds)
        {
            bool inBound = coords.y < bounds.y - 1 && coords.x < bounds.x - 1;
            return select(-1, mad(coords.y, bounds.x, coords.x) + bounds.x + 1, inBound);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomLeftIndex(in int2 coords, int width)
        {
            bool inBound = coords.y > 0 && coords.x > 0;
            return select(-1, mad(coords.y, width, coords.x) - width - 1, inBound);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomRightIndex(in int2 coords, int width)
        {
            bool inBound = coords.y > 0 && coords.x < width - 1;
            return select(-1, mad(coords.y, width, coords.x) - width + 1, inBound);
        }

        /// <summary>
        /// this will replace individual functions (see functions above) when burst will be switch friendly
        /// NOW SUPPORTED SINCE Burst 1.3!
        /// </summary>
        /// <param name="adjCell">adjacent cell you want the index</param>
        /// <param name="index">index of the cell you are checking from</param>
        /// <param name="pos">coords IN the grid</param>
        /// <param name="bounds">bounds of the grid, x = width, y = height</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AdjCellFromIndex(int index, AdjacentCell adjCell, in int2 pos, in int2 bounds) 
        => adjCell switch
        {
            AdjacentCell.Left        when pos.x > 0                                    => index - 1,
            AdjacentCell.Right       when pos.x < bounds.x - 1                         => index + 1,
            AdjacentCell.Top         when pos.y < bounds.y - 1                         => index + bounds.x,
            AdjacentCell.TopLeft     when pos.y < bounds.y - 1 && pos.x > 0            => (index + bounds.x) - 1,
            AdjacentCell.TopRight    when pos.y < bounds.y - 1 && pos.x < bounds.x - 1 => (index + bounds.x) + 1,
            AdjacentCell.Bottom      when pos.y > 0                                    => (index - bounds.x),
            AdjacentCell.BottomLeft  when pos.y > 0 && pos.x > 0                       => (index - bounds.x) - 1,
            AdjacentCell.BottomRight when pos.y > 0 && pos.x < bounds.x - 1            => (index - bounds.x) + 1,
            _ => -1,
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AdjCellFromIndex(int index, int adjCell, in int2 pos, in int2 bounds) 
        => adjCell switch
        {
            (int)AdjacentCell.Left        when pos.x > 0                                    => index - 1,
            (int)AdjacentCell.Right       when pos.x < bounds.x - 1                         => index + 1,
            (int)AdjacentCell.Top         when pos.y < bounds.y - 1                         => index + bounds.x,
            (int)AdjacentCell.TopLeft     when pos.y < bounds.y - 1 && pos.x > 0            => (index + bounds.x) - 1,
            (int)AdjacentCell.TopRight    when pos.y < bounds.y - 1 && pos.x < bounds.x - 1 => (index + bounds.x) + 1,
            (int)AdjacentCell.Bottom      when pos.y > 0                                    => (index - bounds.x),
            (int)AdjacentCell.BottomLeft  when pos.y > 0 && pos.x > 0                       => (index - bounds.x) - 1,
            (int)AdjacentCell.BottomRight when pos.y > 0 && pos.x < bounds.x - 1            => (index - bounds.x) + 1,
            _ => -1,
        };
    }
}
