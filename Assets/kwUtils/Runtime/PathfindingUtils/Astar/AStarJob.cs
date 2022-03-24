using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static KWUtils.KWmath;
using static Unity.Mathematics.math;
using static KWUtils.InputSystemExtension;

namespace KWUtils.KWGenericGrid
{
    [BurstCompile(CompileSynchronously = true)]
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
            NativeHashSet<int> openSet = new NativeHashSet<int>(16, Allocator.Temp);
            NativeHashSet<int> closeSet = new NativeHashSet<int>(16, Allocator.Temp);
            
            Nodes[StartNodeIndex] = StartNode(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
            openSet.Add(StartNodeIndex);

            NativeList<int> neighbors = new NativeList<int>(4,Allocator.Temp);

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
            int2 coord = index.GetXY2(NumCellX);
            for (int i = 0; i < 4; i++)
            {
                int neighborId = index.AdjCellFromIndex((1 << i), coord, NumCellX);
                if (neighborId == -1 || ObstaclesGrid[neighborId] == true || closeSet.Contains(neighborId)) continue;

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
}
