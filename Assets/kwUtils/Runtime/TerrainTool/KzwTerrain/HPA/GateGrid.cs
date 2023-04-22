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
        private int GatesLength;
        private int OffsetHtoV;
        public int2 NumSpaceHV{ get; private set; }
        
        // Individuals Gates
        public Gate[] TerrainGates { get; private set; }
        
        // State of each side at chunkIndex, 0 = top, 1 = right, 2 = bot, 3 = left
        public bool4[] ChunkSides { get; private set; }

        //Continuous Gates on edges of chunks connected to other chunks
        public List<Memory<Gate>> GroupedGates{ get; private set; }
        
        //Chunks with all gates connected to them
        public ChunkComponent[] ChunkComponents { get; private set; }

// =====================================================================================================================
// --- Gates Construction ---
// =====================================================================================================================
        public GateGrid(int chunkQuadsPerLine, in int2 numChunkXY)
        {
            GatesLength = chunkQuadsPerLine;
            OffsetHtoV = max(0, numChunkXY.x - 1) * numChunkXY.y * chunkQuadsPerLine;
            NumSpaceHV = int2(max(0, numChunkXY.x - 1) * numChunkXY.y, numChunkXY.x * max(0, numChunkXY.y - 1));

            ChunkSides = new bool4[csum(numChunkXY)];
            BuildGates(chunkQuadsPerLine, numChunkXY);
            CreateGroupedGate(chunkQuadsPerLine);
            CreateChunkComponents(numChunkXY);
        }
        
        private void CreateGroupedGate(int chunkQuadsPerLine)
        {
            int numGroup = csum(NumSpaceHV);
            GroupedGates = new List<Memory<Gate>>(numGroup);
            for (int i = 0; i < numGroup; i++)
            {
                int startIndex = chunkQuadsPerLine * i;
                GroupedGates.Add(new Memory<Gate>(TerrainGates, startIndex, chunkQuadsPerLine));
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
                /*
                ChunkComponents[i] = new ChunkComponent
                {
                    TopGates    = bottomTop[1] ? GroupedGates[startBotTopIndex[1]]    : Empty<Gate>(),
                    RightGates  = leftRight[1] ? GroupedGates[startLeftRightIndex[1]] : Empty<Gate>(),
                    BottomGates = bottomTop[0] ? GroupedGates[startBotTopIndex[0]]    : Empty<Gate>(),
                    LeftGates   = leftRight[0] ? GroupedGates[startLeftRightIndex[0]] : Empty<Gate>(),
                };
                */
                ChunkComponents[i] = new ChunkComponent
                {
                    TopGates    = bottomTop[1] ? GroupedGates[startBotTopIndex[1]]    : Memory<Gate>.Empty,
                    RightGates  = leftRight[1] ? GroupedGates[startLeftRightIndex[1]] : Memory<Gate>.Empty,
                    BottomGates = bottomTop[0] ? GroupedGates[startBotTopIndex[0]]    : Memory<Gate>.Empty,
                    LeftGates   = leftRight[0] ? GroupedGates[startLeftRightIndex[0]] : Memory<Gate>.Empty,
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
        /*
        ~GateGrid()
        {
            //if (TerrainGates.IsCreated) TerrainGates.Dispose();
            if (ChunkSides.IsCreated) ChunkSides.Dispose();
        }
        */
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
                        int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, ChunkQuadsPerLine,
                            indexInChunkH);
                        int position = baseIndexH + gateIndex;
                        Gates[position] = new Gate(indexInGridH, indexInGridH + 1);
                    }

                    if (index < NumSpaceHorizontalVertical[1])
                    {
                        int indexInChunkV = StartIndexHorizontalVertical[1] + gateIndex; //top left cell on the chunk + index
                        int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, ChunkQuadsPerLine, indexInChunkV);
                        int position = baseIndexV + gateIndex;
                        Gates[position] = new Gate(indexInGridV, indexInGridV + mapNumQuadX);
                    }
                } //end for
            } //end for
        }
    }
}

/*
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
            TopGates    = bottomTop[1] ? GroupedGates[startBotTopIndex[1]]    : Empty<Gate>(),
            RightGates  = leftRight[1] ? GroupedGates[startLeftRightIndex[1]] : Empty<Gate>(),
            BottomGates = bottomTop[0] ? GroupedGates[startBotTopIndex[0]]    : Empty<Gate>(),
            LeftGates   = leftRight[0] ? GroupedGates[startLeftRightIndex[0]] : Empty<Gate>(),
        };
    }
}
*/