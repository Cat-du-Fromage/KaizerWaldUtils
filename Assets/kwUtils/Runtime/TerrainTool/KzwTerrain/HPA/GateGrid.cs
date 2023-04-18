using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        
        //Chunks with all gates connected to them
        public ChunkComponent[] ChunkComponents { get; private set; }
        
        // Individuals Gates
        public Gate[] TerrainGates{ get; private set; }
        
        //Continuous Gates on edges of chunks connected to other chunks
        public List<ArraySegment<Gate>> GroupedGates{ get; private set; }

// =====================================================================================================================
// --- Gates Construction ---
// =====================================================================================================================
        public GateGrid(int chunkQuadsPerLine, in int2 numChunkXY)
        {
            GatesLength = chunkQuadsPerLine;
            OffsetHtoV = max(0, numChunkXY.x - 1) * numChunkXY.y * chunkQuadsPerLine;
            NumSpaceHV = int2(max(0,numChunkXY.x-1) * numChunkXY.y, numChunkXY.x * max(0, numChunkXY.y-1));
            
            BuildGates(chunkQuadsPerLine, numChunkXY);
            CreateGroupedGate(chunkQuadsPerLine);
            CreateChunkComponents(numChunkXY);
        }
        
        private void CreateGroupedGate(int chunkQuadsPerLine)
        {
            int numGroup = csum(NumSpaceHV);
            GroupedGates = new List<ArraySegment<Gate>>(numGroup);
            for (int i = 0; i < numGroup; i++)
            {
                int startIndex = chunkQuadsPerLine * i;
                GroupedGates.Add(new ArraySegment<Gate>(TerrainGates, startIndex, chunkQuadsPerLine));
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
                    TopGates    = bottomTop[1] ? GroupedGates[startBotTopIndex[1]]    : Empty<Gate>(),
                    RightGates  = leftRight[1] ? GroupedGates[startLeftRightIndex[1]] : Empty<Gate>(),
                    BottomGates = bottomTop[0] ? GroupedGates[startBotTopIndex[0]]    : Empty<Gate>(),
                    LeftGates   = leftRight[0] ? GroupedGates[startLeftRightIndex[0]] : Empty<Gate>(),
                };
            }
        }

        private void BuildGates(int chunkQuadsPerLine, int2 numChunkXY)
        {
            int2 startIndexHV = int2(chunkQuadsPerLine - 1, Sq(chunkQuadsPerLine) - chunkQuadsPerLine);
            TerrainGates = new Gate[csum(NumSpaceHV * chunkQuadsPerLine)];
            GetGates(chunkQuadsPerLine);

            void GetGates(int chunkSize)
            {
                int mapNumQuadX = chunkSize * numChunkXY.x;
                if (all(NumSpaceHV <= int2.zero)) return;

                //int arrayOffset = max(0, numChunkXY.x - 1) * numChunkXY.y * chunkSize;
                int numIteration = cmax(NumSpaceHV);
                for (int index = 0; index < numIteration; index++)
                {
                    //Horizontal
                    int2 spaceCoord = GetXY2(index, numChunkXY.x - 1); //coordonnées correspondant ici à l'espace entre 2 partitions
                    int2 chunkCoordH = int2(csum(spaceCoord), spaceCoord.y);
                    int chunkIndexH = GetIndex(chunkCoordH, numChunkXY.x - 1);
                    int baseIndexH = index * chunkSize;

                    //Vertical
                    int2 chunkCoordV = GetXY2(index, numChunkXY.x); //spaceCoord == chunkCoord in this case
                    int chunkIndexV = GetIndex(chunkCoordV, numChunkXY.x);
                    int baseIndexV = index * chunkSize + OffsetHtoV;

                    for (int gateIndex = 0; gateIndex < chunkSize; gateIndex++)
                    {
                        if (index < NumSpaceHV[0])
                        {
                            int indexInChunkH = mad(chunkSize, gateIndex, startIndexHV[0]);
                            int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, chunkSize,
                                indexInChunkH);
                            int position = baseIndexH + gateIndex;
                            TerrainGates[position] = new Gate(indexInGridH, indexInGridH + 1);
                        }

                        if (index < NumSpaceHV[1])
                        {
                            int indexInChunkV = startIndexHV[1] + gateIndex; //top left cell on the chunk + index
                            int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, chunkSize, indexInChunkV);
                            int position = baseIndexV + gateIndex;
                            TerrainGates[position] = new Gate(indexInGridV, indexInGridV + mapNumQuadX);
                        }
                    } //end for
                } //end for
            } //end GetGates
        }
    }
}
