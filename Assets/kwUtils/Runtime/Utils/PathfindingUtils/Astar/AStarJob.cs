using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static Unity.Mathematics.math;
using static KWUtils.InputSystemExtension;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace KWUtils.KWGenericGrid
{
    [BurstCompile]
    public struct JaStar : IJob
    {
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int StartNodeIndex;
        [ReadOnly] public int EndNodeIndex;
        
        [ReadOnly] public NativeArray<bool> ObstaclesGrid;
        public NativeArray<Node> Nodes;
        
        [WriteOnly] public NativeList<int> PathList; // if PathNode.Length == 0 means No Path!
        
        public void Execute()
        {
            NativeHashSet<int> openSet = new (16, Temp);
            NativeHashSet<int> closeSet = new (16, Temp);
            
            Nodes[StartNodeIndex] = StartNode(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
            openSet.Add(StartNodeIndex);

            NativeList<int> neighbors = new (4,Temp);

            while (!openSet.IsEmpty)
            {
                int currentNode = GetLowestFCostNodeIndex(openSet);
                //Check if we already arrived
                if (currentNode == EndNodeIndex)
                {
                    CalculatePath();
                    return;
                }
                //Add "already check" Node AND remove from "To check"
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);
                //Add Neighbors to OpenSet
                GetNeighborCells(currentNode, neighbors, closeSet);
                if (neighbors.Length > 0)
                {
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        openSet.Add(neighbors[i]);
                    }
                }
                neighbors.Clear();
            }
        }

        private void CalculatePath()
        {
            PathList.Add(EndNodeIndex);
            int currentNode = EndNodeIndex;
            while(currentNode != StartNodeIndex)
            {
                currentNode = Nodes[currentNode].CameFromNodeIndex;
                PathList.Add(currentNode);
            }
        }
        
        private void GetNeighborCells(int index, NativeList<int> curNeighbors, NativeHashSet<int> closeSet)
        {
            int2 coord = GetXY2(index,NumCellX);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = AdjCellFromIndex(index,i, coord, NumCellX);
                if (neighborId == -1 || ObstaclesGrid[neighborId] || closeSet.Contains(neighborId)) continue;

                int tentativeCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],Nodes[neighborId]);
                if (tentativeCost < Nodes[neighborId].GCost)
                {
                    curNeighbors.Add(neighborId);
                    int gCost = CalculateDistanceCost(Nodes[neighborId], Nodes[StartNodeIndex]);
                    int hCost = CalculateDistanceCost(Nodes[neighborId], Nodes[EndNodeIndex]);
                    Nodes[neighborId] = new Node(index, gCost, hCost, Nodes[neighborId].Coord);
                }
            }
        }

        private int GetLowestFCostNodeIndex(NativeHashSet<int> openSet)
        {
            int lowest = -1;
            foreach (int index in openSet)
            {
                lowest = lowest == -1 ? index : lowest;
                lowest = select(lowest, index, Nodes[index].FCost < Nodes[lowest].FCost);
            }
            return lowest;
        }

        private Node StartNode(in Node start, in Node end)
        {
            int hCost = CalculateDistanceCost(start, end);
            return new Node(-1, 0, hCost, start.Coord);
        }

        private int CalculateDistanceCost(in Node a, in Node b)
        {
            int2 xyDistance = abs(a.Coord - b.Coord);
            int remaining = abs(xyDistance.x - xyDistance.y);
            return 14 * cmin(xyDistance) + 10 * remaining;
        }
    }

    [BurstCompile]
    public struct JCheckPath : IJob
    {
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int StartNodeIndex;
        [ReadOnly] public int EndNodeIndex;
        
        [ReadOnly] public NativeArray<bool> ObstaclesGrid;
        public NativeArray<Node> Nodes;

        [WriteOnly] public NativeReference<bool> PathExist;

        public void Execute()
        {
            NativeHashSet<int> openSet = new (16, Temp);
            NativeHashSet<int> closeSet = new (16, Temp);
            
            Nodes[StartNodeIndex] = StartNode(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
            openSet.Add(StartNodeIndex);

            NativeList<int> neighbors = new (4,Temp);
            
            int currentNode = GetLowestFCostNodeIndex(openSet);
            PathExist.Value = currentNode == EndNodeIndex;
            while (!openSet.IsEmpty || !PathExist.Value)
            {
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);
                GetNeighborCells(currentNode, neighbors, closeSet);
                if (neighbors.Length > 0)
                {
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        openSet.Add(neighbors[i]);
                    }
                }
                neighbors.Clear();
                currentNode = GetLowestFCostNodeIndex(openSet);
                PathExist.Value = currentNode == EndNodeIndex;
            };
        }

        private void GetNeighborCells(int index, NativeList<int> curNeighbors, NativeHashSet<int> closeSet)
        {
            int2 coord = GetXY2(index,NumCellX);
            for (int i = 0; i < 4; i++)
            {
                int neighborIndex = AdjCellFromIndex(index,i, coord, NumCellX);
                if (neighborIndex == -1 || ObstaclesGrid[neighborIndex] || closeSet.Contains(neighborIndex)) continue;

                int tentativeCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],Nodes[neighborIndex]);
                if (tentativeCost < Nodes[neighborIndex].GCost)
                {
                    curNeighbors.Add(neighborIndex);
                    int gCost = CalculateDistanceCost(Nodes[neighborIndex], Nodes[StartNodeIndex]);
                    int hCost = CalculateDistanceCost(Nodes[neighborIndex], Nodes[EndNodeIndex]);
                    Nodes[neighborIndex] = new Node(index, gCost, hCost, Nodes[neighborIndex].Coord);
                }
            }
        }

        private int GetLowestFCostNodeIndex(NativeHashSet<int> openSet)
        {
            int lowest = openSet.GetEnumerator().Current;
            foreach (int index in openSet)
            {
                lowest = select(lowest, index, Nodes[index].FCost < Nodes[lowest].FCost);
            }
            return lowest;
        }

        private Node StartNode(in Node start, in Node end)
        {
            int hCost = CalculateDistanceCost(start, end);
            return new Node(-1, 0, hCost, start.Coord);
        }

        private int CalculateDistanceCost(in Node a, in Node b)
        {
            int2 xyDistance = abs(a.Coord - b.Coord);
            int remaining = abs(xyDistance.x - xyDistance.y);
            return 14 * cmin(xyDistance) + 10 * remaining;
        }
    }
}
