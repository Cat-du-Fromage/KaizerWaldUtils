using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static System.Array;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using int2 = Unity.Mathematics.int2;

namespace KWUtils
{
    public class GateGrid
    {
        private GridData gridData;
        
        private int GatesLength;
        private int OffsetHtoV;
        public int2 NumSpaceHV{ get; private set; }
        
        // Individuals Gates
        public Gate[] TerrainGates { get; private set; }
        
        // State of each side at chunkIndex, 0 = top, 1 = right, 2 = bot, 3 = left
        public bool4[] ChunkSides { get; private set; }

        //Continuous Gates on edges of chunks connected to other chunks
        public GateCluster[] GateClusters{ get; private set; }
        
        //Chunks with all gates connected to them
        public ChunkComponent[] ChunkComponents { get; private set; }
        
        // SubCluster by chunk AND by sides(inside struct)
        //public ChunkSubCluster[] ChunkSubClusters { get; private set; }
        
        public ClusterSubClusters[] ClustersSubClusters { get; private set; }

// =====================================================================================================================
// --- Gates Construction ---
// =====================================================================================================================
        public GateGrid(int chunkQuadsPerLine, in int2 numChunkXY)
        {
            gridData = new GridData(numChunkXY * chunkQuadsPerLine, chunkQuadsPerLine);
            GatesLength = chunkQuadsPerLine;
            OffsetHtoV = max(0, numChunkXY.x - 1) * numChunkXY.y * chunkQuadsPerLine;
            NumSpaceHV = int2(max(0, numChunkXY.x - 1) * numChunkXY.y, numChunkXY.x * max(0, numChunkXY.y - 1));

            ChunkSides = new bool4[numChunkXY.x * numChunkXY.y];
            BuildGates(chunkQuadsPerLine, numChunkXY);
            BuildGateClusters(chunkQuadsPerLine, numChunkXY);
            CreateChunkComponents(numChunkXY);
            
            // SubCluster By Cluster
            ClustersSubClusters = ClusterSubClusters.Build(NumSpaceHV, GateClusters);
            
            /*
            ChunkSubClusters = new ChunkSubCluster[ChunkComponents.Length];
            for (int i = 0; i < ChunkComponents.Length; i++)
                ChunkSubClusters[i] = new ChunkSubCluster(ChunkComponents[i]);
            */
        }
        private void BuildGateClusters(int chunkQuadsPerLine, in int2 numChunkXY)
        {
            int numGroup = csum(NumSpaceHV);
            GateClusters = new GateCluster[numGroup];
            for (int i = 0; i < numGroup; i++)
            {
                int startIndex = chunkQuadsPerLine * i;
                int2 chunksPair = GetChunkPair(i, NumSpaceHV.x, numChunkXY);
                //UnityEngine.Debug.Log($"index : {i} max: {numGroup} | startIndex: {startIndex} max: {TerrainGates.Length} | chunkPair = {chunksPair}");
                GateClusters[i] = new GateCluster
                (
                    i,
                    NumSpaceHV,
                    numChunkXY,
                    new Memory<Gate>(TerrainGates, startIndex, chunkQuadsPerLine),
                    new Memory<bool4>(ChunkSides, chunksPair.x, 1),
                    new Memory<bool4>(ChunkSides, chunksPair.y, 1)
                );
                
            }
            
            int2 GetChunkPair(int clusterIndex, int numIndicesHorizontal, int2 numChunkXY)
            {
                GateOrientation orientation = (numChunkXY.x is 1 || numChunkXY.y is 1) 
                ? numChunkXY.x is 1 ? GateOrientation.Vertical : GateOrientation.Horizontal
                : (clusterIndex < numIndicesHorizontal) ? GateOrientation.Horizontal : GateOrientation.Vertical;
                
                bool isHorizontal = orientation is GateOrientation.Horizontal;
            
                int offsetIndex   = isHorizontal ? clusterIndex : clusterIndex - numIndicesHorizontal;
                int width         = isHorizontal ? numChunkXY.x - 1 : numChunkXY.x;
                int2 coord        = GetXY2(offsetIndex, width);
  
                int chunk1Index   = isHorizontal ? clusterIndex + coord.y : offsetIndex;
                int chunk2Index   = isHorizontal ? chunk1Index + 1 : chunk1Index + numChunkXY.x;

                return new int2(chunk1Index, chunk2Index);
            }
        }
        private void CreateChunkComponents(in int2 numChunkXY)
        {
            ChunkComponents = new ChunkComponent[numChunkXY.x * numChunkXY.y];
            for (int i = 0; i < ChunkComponents.Length; i++)
            {
                (int x, int y) = GetXY(i, numChunkXY.x);
                bool2 leftRight = bool2(x > 0, x < numChunkXY.x - 1);
                bool2 bottomTop = bool2(y > 0, y < numChunkXY.y - 1);
                //Horizontal: start Indexes
                int2 startLeftRightIndex = int2(i - y - 1, i - y);
                //Vertical: start Indexes, numSpace because we work on chunks! not cells
                int2 startBotTopIndex = NumSpaceHV.x + int2(mad(y-1, numChunkXY.x,x), mad(y,numChunkXY.x,x));
                ChunkComponents[i] = new ChunkComponent
                {
                    TopGates    = bottomTop[1] ? new Memory<GateCluster>(GateClusters, startBotTopIndex[1],1)    : Memory<GateCluster>.Empty,
                    RightGates  = leftRight[1] ? new Memory<GateCluster>(GateClusters, startLeftRightIndex[1],1) : Memory<GateCluster>.Empty,
                    BottomGates = bottomTop[0] ? new Memory<GateCluster>(GateClusters, startBotTopIndex[0],1)    : Memory<GateCluster>.Empty,
                    LeftGates   = leftRight[0] ? new Memory<GateCluster>(GateClusters, startLeftRightIndex[0],1) : Memory<GateCluster>.Empty,
                };
            }
        }
        
        private void BuildGates(int chunkQuadsPerLine, int2 numChunkXY)
        {
            TerrainGates = new Gate[csum(NumSpaceHV * chunkQuadsPerLine)];
            using NativeArray<Gate> tempGates = new(csum(NumSpaceHV * chunkQuadsPerLine), TempJob, UninitializedMemory);
            JBuildGates job = new()
            {
                ChunkQuadsPerLine = chunkQuadsPerLine,
                OffsetHorizontalToVertical = OffsetHtoV,
                NumChunkX = numChunkXY.x,
                StartIndexHorizontalVertical = int2(chunkQuadsPerLine - 1, Sq(chunkQuadsPerLine) - chunkQuadsPerLine),
                NumSpaceHorizontalVertical = NumSpaceHV,
                Gates = tempGates
            };
            job.Schedule(default).Complete();
            tempGates.CopyTo(TerrainGates);
        }
        
        // ==============================================================================================
        // --- UPDATE ELEMENTS ---
        // ==============================================================================================

        public int GetClusterIndex(int gridIndex)
        {
            int chunkIndex = GetChunkIndexFromGridIndex(gridIndex, gridData.NumCellInChunkX, gridData.NumChunkXY.x);
            int2 chunkCoord = GetXY2(chunkIndex, gridData.NumChunkXY.x);
            int indexInChunk = GetCellChunkIndexFromGridIndex(gridIndex, gridData.NumCellInChunkX, gridData.NumChunkXY.x);
            int2 cellCoord = GetXY2(indexInChunk, gridData.NumCellInChunkX);
            
            // Gauche
            if (cellCoord.x == 0 && chunkCoord.x != 0)
            {
                int rightClusterIndex = chunkIndex - 1 - chunkCoord.y;
                int gateIndex = cellCoord.y + rightClusterIndex * gridData.NumCellInChunkX;
                return gateIndex;
            } 
            // Droite
            if (cellCoord.x == gridData.NumCellInChunkX-1 && chunkCoord.x != gridData.NumChunkXY.x-1)
            {
                int leftClusterIndex = chunkIndex - chunkCoord.y;
                int gateIndex = cellCoord.y + leftClusterIndex * gridData.NumCellInChunkX;
                return gateIndex;
            }
            
            // TOP / BOTTOM => OFFSET
            
            // Bas
            if (cellCoord.y == 0 && chunkCoord.y != 0)
            {
                int botClusterIndex = NumSpaceHV.x + chunkIndex - gridData.NumChunkXY.x;
                int gateIndex = cellCoord.x + botClusterIndex * gridData.NumCellInChunkX;
                return gateIndex;
            }
            // Haut
            if (cellCoord.y == gridData.NumCellInChunkX-1 && chunkCoord.y != gridData.NumCellInChunkX-1)
            {
                int topClusterIndex = NumSpaceHV.x + chunkIndex;
                int gateIndex = cellCoord.x + topClusterIndex * gridData.NumCellInChunkX;
                return gateIndex;
            }
            return 0;
            
            
        }
        
        public void CloseGateAt(int gateIndex)
        {
            TerrainGates[gateIndex].IsClosed = true;
        }
    }

    [BurstCompile]
    public struct JBuildGates : IJob
    {
        [ReadOnly] public int ChunkQuadsPerLine;
        [ReadOnly] public int OffsetHorizontalToVertical;
        [ReadOnly] public int NumChunkX;
        [ReadOnly] public int2 StartIndexHorizontalVertical;
        [ReadOnly] public int2 NumSpaceHorizontalVertical;
        [WriteOnly] public NativeArray<Gate> Gates;
        public void Execute()
        {
            int mapNumQuadX = ChunkQuadsPerLine * NumChunkX;
            if (all(NumSpaceHorizontalVertical <= int2.zero)) return;

            //int arrayOffset = max(0, numChunkXY.x - 1) * numChunkXY.y * chunkSize;
            int numIteration = cmax(NumSpaceHorizontalVertical);
            for (int index = 0; index < numIteration; index++)
            {
                //Horizontal
                int2 spaceCoord = GetXY2(index, NumChunkX - 1); //coordonnées correspondant ici à l'espace entre 2 partitions
                int2 chunkCoordH = int2(csum(spaceCoord), spaceCoord.y);
                int chunkIndexH = GetIndex(chunkCoordH, NumChunkX - 1);
                int baseIndexH = index * ChunkQuadsPerLine;

                //Vertical
                int2 chunkCoordV = GetXY2(index, NumChunkX); //spaceCoord == chunkCoord in this case
                int chunkIndexV = GetIndex(chunkCoordV, NumChunkX);
                int baseIndexV = index * ChunkQuadsPerLine + OffsetHorizontalToVertical;

                for (int gateIndex = 0; gateIndex < ChunkQuadsPerLine; gateIndex++)
                {
                    if (index < NumSpaceHorizontalVertical[0])
                    {
                        int indexInChunkH = mad(ChunkQuadsPerLine, gateIndex, StartIndexHorizontalVertical[0]);
                        int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, ChunkQuadsPerLine, indexInChunkH);
                        int position = baseIndexH + gateIndex;
                        Gates[position] = new Gate(position, indexInGridH, indexInGridH + 1);
                    }

                    if (index < NumSpaceHorizontalVertical[1])
                    {
                        int indexInChunkV = StartIndexHorizontalVertical[1] + gateIndex; //top left cell on the chunk + index
                        int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, ChunkQuadsPerLine, indexInChunkV);
                        int position = baseIndexV + gateIndex;
                        Gates[position] = new Gate(position,indexInGridV, indexInGridV + mapNumQuadX);
                    }
                } //end for
            } //end GetGates
        } //end for
    }
}
