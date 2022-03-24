#define EnableBurst
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

namespace KWUtils.KWGenericGrid
{
    /// <summary>
    /// CAREFUL FOR BLENDING
    /// NEED TO KNOW which cell is directly near an unwalkable chunk
    /// So we can set a value of 2 for the costfield
    /// </summary>
#if EnableBurst
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct JCostField : IJobFor
    {
        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<bool> Obstacles;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<byte> CostField;

        public JCostField(NativeArray<bool> obstacles, NativeArray<byte> costField)
        {
            Obstacles = obstacles;
            CostField = costField;
        }

        public void Execute(int index)
        {
            CostField[index] = Obstacles[index] == false ? (byte) 1 : byte.MaxValue;
        }
    }

#if EnableBurst
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct JIntegrationField : IJob
    {
        [ReadOnly] public int DestinationCellIndex;
        [ReadOnly] public int NumCellX;

        public NativeArray<byte> CostField;
        public NativeArray<int> BestCostField;

        public JIntegrationField(int destinationIndex, int numCellX, NativeArray<byte> costField,
            NativeArray<int> bestCostField)
        {
            DestinationCellIndex = destinationIndex;
            NumCellX = numCellX;
            CostField = costField;
            BestCostField = bestCostField;
        }

        public void Execute()
        {
            NativeQueue<int> cellsToCheck = new NativeQueue<int>(Allocator.Temp);
            NativeList<int> currentNeighbors = new NativeList<int>(4, Allocator.Temp);

            //Set Destination cell cost at 0
            CostField[DestinationCellIndex] = 0;
            BestCostField[DestinationCellIndex] = 0;

            cellsToCheck.Enqueue(DestinationCellIndex);

            while (cellsToCheck.Count > 0)
            {
                int currentCellIndex = cellsToCheck.Dequeue();
                GetNeighborCells(currentCellIndex, currentNeighbors);

                for (int i = 0; i < currentNeighbors.Length; i++)
                {
                    int neighborIndex = currentNeighbors[i];
                    if (CostField[neighborIndex] >= byte.MaxValue) continue;
                    if (CostField[neighborIndex] + BestCostField[currentCellIndex] < BestCostField[neighborIndex])
                    {
                        BestCostField[neighborIndex] = CostField[neighborIndex] + BestCostField[currentCellIndex];
                        cellsToCheck.Enqueue(neighborIndex);
                    }
                }

                currentNeighbors.Clear();
            }
        }

        private readonly void GetNeighborCells(int index, NativeList<int> curNeighbors)
        {
            int2 coord = index.GetXY2(NumCellX);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, NumCellX);
                if (neighborId == -1) continue;
                curNeighbors.AddNoResize(neighborId);
            }
        }
    }
#if EnableBurst
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct JBestDirection : IJobFor
    {
        [ReadOnly] public int NumCellX;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<int> BestCostField;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<float3> CellBestDirection;

        public JBestDirection(int numCellX, NativeArray<int> bestCostField, NativeArray<float3> cellBestDirection)
        {
            NumCellX = numCellX;
            BestCostField = bestCostField;
            CellBestDirection = cellBestDirection;
        }

        public void Execute(int index)
        {
            int currentBestCost = BestCostField[index];

            if (currentBestCost >= ushort.MaxValue)
            {
                CellBestDirection[index] = float3.zero;
                return;
            }

            int2 currentCellCoord = index.GetXY2(NumCellX);
            NativeList<int> neighbors = GetNeighborCells(index, currentCellCoord);
            for (int i = 0; i < neighbors.Length; i++)
            {
                int currentNeighbor = neighbors[i];
                if (BestCostField[currentNeighbor] < currentBestCost)
                {
                    currentBestCost = BestCostField[currentNeighbor];
                    int2 neighborCoord = currentNeighbor.GetXY2(NumCellX);
                    int2 bestDirection = neighborCoord - currentCellCoord;
                    CellBestDirection[index] = new float3(bestDirection.x, 0, bestDirection.y);
                }
            }
        }

        private readonly NativeList<int> GetNeighborCells(int index, in int2 coord)
        {
            NativeList<int> neighbors = new NativeList<int>(4, Allocator.Temp);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, NumCellX);
                if (neighborId == -1) continue;
                neighbors.AddNoResize(neighborId);
            }

            return neighbors;
        }
    }
}