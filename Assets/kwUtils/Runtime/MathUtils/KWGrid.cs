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
        public static int2 GetXY2(this int i, int w)
        {
            int y = (int)floor((float)i/w);
            int x = i - (y * w);
            return int2(x, y);
        }
        
        /// <summary>
        /// Get position (in Int, Int) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the Grid</param>
        /// <returns>Int X, Int Y(return in this order)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int,int) GetXY(this int i, int w)
        {
            int y = (int)floor((float)i / w);
            int x = i - (y * w);
            return (x, y);
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
            float val = float.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
            }
            return index;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<int> dis, in NativeArray<int> cellIndex)
        {
            int val = int.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
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
        
        /// <summary>
        /// NEED A HUGE CLEAN UP THIS IS A MESS!!
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetCoordFromPosition(this float3 pointPos, int gridSize, float spacing)
        {
            float mapOffset = gridSize / 2f;
            
            float percentX = (pointPos.x + mapOffset) / (gridSize * spacing);
            float percentY = (pointPos.z + mapOffset) / (gridSize * spacing);
            
            percentX = clamp(percentX, 0, 1f); //CAREFUL NEED ABS!
            percentY = clamp(percentY, 0, 1f); //CAREFUL NEED ABS!
 
            int x = clamp((int)floor((gridSize) * percentX), 0, gridSize - 1);
            int y = clamp((int)floor((gridSize) * percentY), 0, gridSize - 1);
            
            return int2(x,y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this float3 pointPos, int gridSize, float spacing, bool offset = false)
        {
            float mapOffset = offset ? gridSize / 2f : 0;
            
            float percentX = (pointPos.x + mapOffset) / (gridSize * spacing);
            float percentY = (pointPos.z + mapOffset) / (gridSize * spacing);
            
            percentX = clamp(percentX, 0, 1f); //CAREFUL NEED ABS!
            percentY = clamp(percentY, 0, 1f); //CAREFUL NEED ABS!
 
            int x = clamp((int)floor(gridSize * percentX), 0, gridSize - 1);
            int y = clamp((int)floor(gridSize * percentY), 0, gridSize - 1);
            return mad(y, (int)(gridSize/spacing), x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this float3 pointPos, int2 gridSize, int cellSize)
        {
            float percentX = (pointPos.x) / (gridSize.x * cellSize);
            float percentY = (pointPos.z) / (gridSize.y * cellSize);
            
            percentX = clamp(percentX, 0, 1f); //CAREFUL NEED ABS!
            percentY = clamp(percentY, 0, 1f); //CAREFUL NEED ABS!
 
            int x = clamp((int)floor(gridSize.x * percentX), 0, gridSize.x - 1);
            int y = clamp((int)floor(gridSize.y * percentY), 0, gridSize.y - 1);
            return mad(y, gridSize.x/cellSize, x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this float2 pointPos, int2 gridSize, int cellSize)
        {
            float2 percents = pointPos / (gridSize * cellSize);
            percents = clamp(percents, 0, 1f);

            int2 xy =  clamp((int2)floor(gridSize * percents), 0, gridSize - 1);
            
            return mad(xy.y, gridSize.x/cellSize, xy.x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPositionOffseted(this float2 pointPos, int2 gridSize, int cellSize, int2 offset)
        {
            float2 percents = (pointPos - offset) / (gridSize * cellSize);
            percents = clamp(percents, 0, 1f);

            int2 xy =  clamp((int2)floor(gridSize * percents), 0, gridSize - 1);
            
            return mad(xy.y, gridSize.x/cellSize, xy.x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this float3 pointPos, int2 gridSize, int cellSize, bool offset)
        {
            float2 mapOffset = offset ? gridSize / 2 : 0;
            
            float percentX = (pointPos.x + mapOffset.x) / (gridSize.x * cellSize);
            float percentY = (pointPos.z + mapOffset.y) / (gridSize.y * cellSize);
            
            percentX = clamp(percentX, 0, 1f); //CAREFUL NEED ABS!
            percentY = clamp(percentY, 0, 1f); //CAREFUL NEED ABS!
 
            int x = clamp((int)floor(gridSize.x * percentX), 0, gridSize.x - 1);
            int y = clamp((int)floor(gridSize.y * percentY), 0, gridSize.y - 1);
            return mad(y, gridSize.x/cellSize, x);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this Vector3 pointPos, int2 gridSize, int cellSize)
        {
            float percentX = pointPos.x / (gridSize.x * cellSize);
            float percentY = pointPos.z / (gridSize.y * cellSize);

            percentX = Clamp01(percentX); //CAREFUL NEED ABS!
            percentY = Clamp01(percentY); //CAREFUL NEED ABS!
            
            int x = Clamp(FloorToInt(gridSize.x * percentX), 0, gridSize.x-1);
            int y = Clamp(FloorToInt(gridSize.y * percentY), 0, gridSize.y-1);

            return y * (gridSize.x/cellSize) + x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexFromPosition(this Vector3 pointPos, int2 gridSize, float cellSize)
        {
            float percentX = pointPos.x / (gridSize.x * cellSize);
            float percentY = pointPos.z / (gridSize.y * cellSize);

            percentX = Clamp01(percentX); //CAREFUL NEED ABS!
            percentY = Clamp01(percentY); //CAREFUL NEED ABS!
            
            int x = Clamp(FloorToInt(gridSize.x * percentX), 0, gridSize.x-1);
            int y = Clamp(FloorToInt(gridSize.y * percentY), 0, gridSize.y-1);

            return (y * FloorToInt(gridSize.x/cellSize) + x);
        }

        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellSize"></param>
        /// <param name="botLeftPoint"></param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(this float2 pointPos, int numCellMap, float cellSize, float2 botLeftPoint = default)
        {
            botLeftPoint = abs(botLeftPoint);
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) 
                    cellGrid.y = select(numCellMap, i, pointPos.y + botLeftPoint.y <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) 
                    cellGrid.x = select(numCellMap, i, pointPos.x + botLeftPoint.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) 
                    break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }

        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellSize"></param>
        /// <param name="botLeftPoint"></param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(this float3 pointPos, int numCellMap, float cellSize, float3 botLeftPoint = default)
        {
            botLeftPoint = abs(botLeftPoint);
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) 
                    cellGrid.y = select(numCellMap, i, pointPos.z + botLeftPoint.z <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) 
                    cellGrid.x = select(numCellMap, i, pointPos.x + botLeftPoint.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) 
                    break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }
        
        
        /// <summary>
        /// Use to check around a cell (1 tile around with diagonal)
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="gridWidth"></param>
        /// <returns>number of cell to check; X Range; Y Range</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int numCell, int2 xRange, int2 yRange) CellGridRanges(this int cellId, int gridWidth)
        {
            int y = (int)floor((float)cellId / gridWidth);
            int x = cellId - (y * gridWidth);

            bool corner = (x == 0 && y == 0) 
                          || (x == 0 && y == gridWidth - 1) 
                          || (x == gridWidth - 1 && y == 0) 
                          || (x == gridWidth - 1 && y == gridWidth - 1);
            
            bool yOnEdge = y == 0 || y == gridWidth - 1;
            bool xOnEdge = x == 0 || x == gridWidth - 1;

            //check if on edge 0 : int2(0, 1) ; if not NumCellJob - 1 : int2(-1, 0)
            int2 OnEdge(int e) => select(int2(-1, 0), int2(0, 1), e == 0);
            
            int2 yRange = select(OnEdge(y), int2(-1, 1), !yOnEdge);
            int2 xRange = select(OnEdge(x), int2(-1, 1), !xOnEdge);
            int numCell = select(select(9, 6, yOnEdge || xOnEdge), 4, corner);
            
            return (numCell, xRange, yRange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int x, int y, int gridWidth)
        {
            return (x == 0 && y == 0) 
                   || (x == 0 && y == gridWidth - 1) 
                   || (x == gridWidth - 1 && y == 0) 
                   || (x == gridWidth - 1 && y == gridWidth - 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(this int2 xy, int gridWidth)
        {
            int x = xy.x;
            int y = xy.y;
            return (x == 0 && y == 0) 
                   || (x == 0 && y == gridWidth - 1) 
                   || (x == gridWidth - 1 && y == 0) 
                   || (x == gridWidth - 1 && y == gridWidth - 1);
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
        public static int GetLeftIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - 1, coords.x > 0);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRightIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + 1, coords.x < width - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - width, coords.y > 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width, coords.y < width - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopLeftIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width - 1, coords.y < (width - 1) && coords.x > 0);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopRightIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width + 1, coords.y < width - 1 && coords.x < width - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomLeftIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x ) - width - 1, coords.y > 0 && coords.x > 0);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomRightIndex(this int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - width + 1, coords.y > 0 && coords.x < width - 1);
        
        /// <summary>
        /// this will replace individual functions (see functions above) when burst will be switch friendly
        /// NOW SUPPORTED SINCE Burst 1.3!
        /// </summary>
        /// <param name="adjCell">adjacent cell you want the index</param>
        /// <param name="index">index of the cell you are checking from</param>
        /// <param name="pos">coords IN the grid</param>
        /// <param name="width">width of the grid</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AdjCellFromIndex(this int index, AdjacentCell adjCell, in int2 pos, int width) 
        => adjCell switch
        {
            AdjacentCell.Left        when pos.x > 0                              => index - 1,
            AdjacentCell.Right       when pos.x < width - 1                      => index + 1,
            AdjacentCell.Top         when pos.y < width - 1                      => index + width,
            AdjacentCell.TopLeft     when pos.y < width - 1 && pos.x > 0         => (index + width) - 1,
            AdjacentCell.TopRight    when pos.y < width - 1 && pos.x < width - 1 => (index + width) + 1,
            AdjacentCell.Bottom      when pos.y > 0                              => index - width,
            AdjacentCell.BottomLeft  when pos.y > 0 && pos.x > 0                 => (index - width) - 1,
            AdjacentCell.BottomRight when pos.y > 0 && pos.x < width - 1         => (index - width) + 1,
            _ => -1,
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AdjCellFromIndex(this int index, int adjCell, in int2 pos, int width) 
        => adjCell switch
        {
            (int)AdjacentCell.Left        when pos.x > 0                              => index - 1,
            (int)AdjacentCell.Right       when pos.x < width - 1                      => index + 1,
            (int)AdjacentCell.Top         when pos.y < width - 1                      => index + width,
            (int)AdjacentCell.TopLeft     when pos.y < width - 1 && pos.x > 0         => (index + width) - 1,
            (int)AdjacentCell.TopRight    when pos.y < width - 1 && pos.x < width - 1 => (index + width) + 1,
            (int)AdjacentCell.Bottom      when pos.y > 0                              => index - width,
            (int)AdjacentCell.BottomLeft  when pos.y > 0 && pos.x > 0                 => (index - width) - 1,
            (int)AdjacentCell.BottomRight when pos.y > 0 && pos.x < width - 1         => (index - width) + 1,
            _ => -1,
        };
    }
}
