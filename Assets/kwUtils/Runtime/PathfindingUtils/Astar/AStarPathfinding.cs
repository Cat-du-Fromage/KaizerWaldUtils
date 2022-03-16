using System;
using System.Diagnostics;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using static KWUtils.KWmath;
using static Unity.Mathematics.math;
using static KWUtils.InputSystemExtension;


namespace TowerDefense
{
    public partial class AStarPathfinding : MonoBehaviour, IGridHandler<Node, GenericGrid<Node>>
    {
        [SerializeField] private Transform DestinationGate;
        [SerializeField] private Transform agentStart;
        
        private const int CellSize = 1; //ADAPTATION NEEDED!
        
        private const int DiagonalCostMove = 14;
        private const int StraightCostMove = 10;
        
        private Vector3 startPosition;
        private Vector3 destination;
        
        private int startIndex = -1;
        private int endIndex = -1;
        
        private readonly RaycastHit[] hits = new RaycastHit[1];
        
        private int[] path;
        private Node grid;
        
        private bool jobSchedule;
        private JobHandle lastJobScheduled;

        private NativeArray<Node> nodes;
        private NativeList<int> pathList;

        //Interfaces
        public IGridSystem GridSystem { get; set; }

        public GenericGrid<Node> Grid { get; private set; }
        
        public void InitializeGrid(int2 terrainBounds)
        {
            Grid = new GenericGrid<Node>(terrainBounds, CellSize, (coord) => new Node(coord));
        }

        private void Awake()
        {
            destination = DestinationGate.position;
            endIndex = Grid.IndexFromPosition(destination);
        }
        
        private void Update()
        {
            if (!jobSchedule) return;
            if (!lastJobScheduled.IsCompleted) return;
            jobSchedule = CompleteJob();

            if(Keyboard.current.pKey.wasPressedThisFrame && agentStart != null)
            {
#if UNITY_EDITOR
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                startPosition = agentStart.position;
                startIndex = Grid.IndexFromPosition(startPosition);
                path = AStarProcess();
                
#if UNITY_EDITOR
                sw.Stop();
                print($"Path found: {sw.Elapsed} ms");          
#endif
                
            }
        }

        private bool CompleteJob()
        {
            int[] pathToFollow = pathList.ToArray().Reverse();

            DisposeAll();
            return false;
        }

        public void OnObstacleAdded(int index)
        {
            //Get the Node affected
            if (!path.IsNullOrEmpty())
            {
                for (int pathIndex = 0; pathIndex < path.Length; pathIndex++)
                {
                    if (path[pathIndex] != index) continue;
                    
                    startIndex = path[pathIndex-1];
                    int[] segment = AStarProcess();
                    
                    if (segment.IsNullOrEmpty()) return; // CAREFUL IT MEANS THERE IS NOS PATH!
                    
                    Array.Resize(ref path, pathIndex + segment.Length);
                    segment.CopyTo(path, pathIndex);
                    startIndex = Grid.IndexFromPosition(startPosition);
                    break;
                }
            }
            
            //Check if current Path contain the Node Index
        }

        public (Vector3[], int[]) RequestPath(in Vector3 currentPosition)
        {
            startIndex = Grid.IndexFromPosition(currentPosition);
            path = AStarProcess();
            Vector3[] nodesPosition = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                nodesPosition[i] = Grid.GetCellCenter(path[i]);
            }
            return (nodesPosition, path);
        }

        private int[] AStarProcess()
        {
            using NativeArray<bool> obstacles = GridSystem.RequestGrid<bool,GridType>(GridType.Obstacles).ToNativeArray();
            nodes = Grid.GridArray.ToNativeArray();

            pathList = new NativeList<int>(16, Allocator.TempJob);

            //Get Path from Start -> End
            JaStar job = new JaStar
            {
                NumCellX = Grid.GridBound.x,
                StartNodeIndex = startIndex,
                EndNodeIndex = endIndex,
                Nodes = nodes,
                ObstaclesGrid = obstacles,
                PathList = pathList
            };
            lastJobScheduled = job.Schedule();
            JobHandle.ScheduleBatchedJobs();

            jobSchedule = true;
            
            //WWARNING MOVE THIS
            lastJobScheduled.Complete(); 
            //WWARNING MOVE THIS
            DisposeAll();
            int[] pathToFollow = pathList.ToArray().Reverse();
            return pathToFollow;
        }

        private void DisposeAll()
        {
            if (nodes.IsCreated)    nodes.Dispose();
            if (pathList.IsCreated) pathList.Dispose();
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