using System;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.KWmath;
using static Unity.Mathematics.math;
using static KWUtils.InputSystemExtension;

namespace KWUtils.KWGenericGrid
{
    public class AStarPathfinding : MonoBehaviour, IGridHandler<GridType, Node, GenericGrid<Node>>
    {
        private const int CellSize = 1; //ADAPTATION NEEDED!
        
        private const int DiagonalCostMove = 14;
        private const int StraightCostMove = 10;
        
        [SerializeField] private Transform DestinationCell;
        [SerializeField] private Transform StartCell;
        
        private int startIndex = -1;
        private int endIndex = -1;
        
        private Vector3 startPosition;
        private Vector3 destination;
        
        private readonly RaycastHit[] hits = new RaycastHit[1];
        
        private int[] currentPath;
        //private Node grid;
        
        private bool jobSchedule;
        private JobHandle lastJobScheduled;

        private NativeArray<Node> nativeNodes;
        private NativeList<int> nativePathList;

        //==============================================================================================================
        /// Grid Interface
        public IGridSystem<GridType> GridSystem { get; set; }
        public GenericGrid<Node> Grid { get; private set; }
        public void InitializeGrid(int2 terrainBounds)
        {
            int numCellX = terrainBounds.x / CellSize;
            Grid = new GenericGrid<Node>(terrainBounds, CellSize, (index) => new Node(index.GetXY2(numCellX)));
        }
        //==============================================================================================================
        
        private void Awake()
        {
            destination = DestinationCell.position;
            endIndex = Grid.IndexFromPosition(destination);
        }
        
        private void Update()
        {
            if (!jobSchedule) return;
            if (!lastJobScheduled.IsCompleted) return;
            jobSchedule = CompleteJob();
        }

        private bool CompleteJob()
        {
            lastJobScheduled.Complete();
            DisposeAll();
            // Path Is Valid
            if (!nativePathList.IsEmpty) 
            {
                if (currentPath.Length != nativePathList.Length)
                {
                    Array.Resize(ref currentPath, nativePathList.Length);
                }
                nativePathList
                    .ToArray()
                    .Reverse()
                    .CopyTo((Span<int>)currentPath);
                
                return false;
                //currentPath = nativePathList.ToArray().Reverse();
            }
            
            // NO valid Path
            
            //Notify sender there is no Path => callback(true/false)? 
            //false:dont update // true:update currentPath
            return false;
        }
/*
        public void OnObstacleAdded(int index, in GridData gridData)
        {
            //We didn't calculate anyPath
            if (currentPath.IsNullOrEmpty()) return;
            
            for (int pathIndex = 0; pathIndex < currentPath.Length; pathIndex++)
            {
                if (currentPath[pathIndex] == index)
                {
                    startIndex = currentPath[pathIndex-1];
                    int[] segment = AStarProcess(gridData);
                
                    if (segment.IsNullOrEmpty()) return; // CAREFUL IT MEANS THERE IS NOS PATH!

                    currentPath.Resize(pathIndex + segment.Length);
                    segment.CopyTo(currentPath, pathIndex);
                    startIndex = Grid.IndexFromPosition(startPosition);
                    break;
                }
            }
        }
*/

        private void AStarProcess(in GridData gridData)
        {
            using NativeArray<bool> obstacles = new (GridSystem.RequestGridArray<bool>(GridType.Obstacles), Allocator.Temp);
            nativeNodes = new NativeArray<Node>(Grid.GridArray, Allocator.TempJob);

            nativePathList = new NativeList<int>(16, Allocator.TempJob);

            //Get Path from Start -> End
            JaStar job = new JaStar
            {
                NumCellX = gridData.NumCellXY.x,
                StartNodeIndex = startIndex,
                EndNodeIndex = endIndex,
                Nodes = nativeNodes,
                ObstaclesGrid = obstacles,
                PathList = nativePathList
            };
            lastJobScheduled = job.Schedule();
            JobHandle.ScheduleBatchedJobs();

            jobSchedule = true;
        }

        private void DisposeAll()
        {
            if (nativeNodes.IsCreated)    nativeNodes.Dispose();
            if (nativePathList.IsCreated) nativePathList.Dispose();
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct JaStar : IJob
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

                NativeList<int> neighbors = new NativeList<int>(8,Allocator.Temp);

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
                for (int i = 0; i < 8; i++)
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
                return DiagonalCostMove * cmin(xyDistance) + StraightCostMove * remaining;
            }
        }

        
    }
}