using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

namespace KWUtils
{
    public partial class HPAPathfinding : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;
        
        private NativeArray<Node> nativeNodes;

        private void Awake()
        {
            TryGetComponent(out terrain);
        }

        private void Start()
        {
            nativeNodes = new NativeArray<Node>(terrain.Settings.ChunksCount, Persistent);
        }

        private void OnDestroy()
        {
            if (nativeNodes.IsCreated) nativeNodes.Dispose();
        }
    }
    
    [BurstCompile]
    public struct JGetChunkPath : IJob
    {
        [ReadOnly] public int StartIndex;
        [ReadOnly] public int EndIndex;
        [ReadOnly] public int2 NumChunkXY;
        [ReadOnly] public NativeArray<bool4> ChunkSidesIsClosed;
        [WriteOnly] public NativeList<int> PathList;
        
        public JGetChunkPath(int startIndex, int endIndex, in int2 numChunkXY, 
            NativeArray<bool4> chunkSidesIsClosed, NativeList<int> pathList)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            NumChunkXY = numChunkXY;
            ChunkSidesIsClosed = chunkSidesIsClosed;
            PathList = pathList;
        }

        public void Execute()
        {
            NativeArray<Node> Nodes = new (cmul(NumChunkXY), Temp);
            Nodes[EndIndex] = new Node(GetXY2(EndIndex, NumChunkXY.x));
            Nodes[StartIndex] = new Node(GetXY2(StartIndex, NumChunkXY.x));
            
            NativeHashSet<int> openSet = new (16, Temp);
            NativeHashSet<int> closeSet = new (16, Temp);
            
            Nodes[StartIndex] = StartNode(Nodes[StartIndex], Nodes[EndIndex]);
            openSet.Add(StartIndex);

            NativeList<int> neighbors = new (4,Temp);
            
            int currentNode = GetLowestFCostNodeIndex(ref Nodes, openSet);
            bool pathExist = currentNode == EndIndex;
            while (!openSet.IsEmpty && !pathExist) //notEmpty and noPath
            {
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);
                GetNeighborCells(ref Nodes, currentNode, neighbors, closeSet);
                if (neighbors.Length > 0)
                {
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        openSet.Add(neighbors[i]);
                    }
                }
                neighbors.Clear();
                currentNode = GetLowestFCostNodeIndex(ref Nodes, openSet);
                pathExist = currentNode == EndIndex;
            };

            if (!pathExist) return;
            CalculatePath(ref Nodes);
        }
        
        private void CalculatePath(ref NativeArray<Node> Nodes)
        {
            PathList.Add(EndIndex);
            int currentNode = EndIndex;
            while(currentNode != StartIndex)
            {
                currentNode = Nodes[currentNode].CameFromNodeIndex;
                PathList.Add(currentNode);
            }
        }

        private void GetNeighborCells(ref NativeArray<Node> Nodes, int index, NativeList<int> curNeighbors, NativeHashSet<int> closeSet)
        {
            int2 coord = GetXY2(index,NumChunkXY.x);
            for (int i = 0; i < 4; i++)
            {
                int neighborIndex = AdjCellFromIndex(index,i, coord, NumChunkXY);
                if (neighborIndex == -1 || ChunkSidesIsClosed[index][i]) continue;
                if (closeSet.Contains(neighborIndex)) continue;
                
                //Node neighborNode = Nodes[neighborIndex];
                Node neighborNode = new Node(GetXY2(neighborIndex, NumChunkXY.x));
                int tentativeCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],neighborNode);
                if (tentativeCost < neighborNode.GCost)
                {
                    curNeighbors.Add(neighborIndex);
                    int gCost = CalculateDistanceCost(neighborNode, Nodes[StartIndex]);
                    int hCost = CalculateDistanceCost(neighborNode, Nodes[EndIndex]);
                    Nodes[neighborIndex] = new Node(index, gCost, hCost, neighborNode.Coord);
                }
            }
        }

        private int GetLowestFCostNodeIndex(ref NativeArray<Node> Nodes, NativeHashSet<int> openSet)
        {
            int lowest = -1;
            foreach (int index in openSet)
            {
                lowest = select(lowest,index,lowest == -1);
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
