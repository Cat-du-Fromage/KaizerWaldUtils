using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    [RequireComponent(typeof(KzwTerrainGenerator))]
    public partial class KzwHPAGenerator : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;
        private Gate[] TerrainGates;

        private void Awake()
        {
            bool terrainExist = TryGetComponent(out terrain);
        }

        private void Start()
        {
            TerrainGates = BuildGates(terrain.Grid.GridData).ToArray();
        }

        private NativeArray<Gate> BuildGates(in GridData gridData)
        {
            int2 numChunkXY = gridData.NumChunkXY;
            int2 numSpaceHV = int2(max(0,numChunkXY.x-1) * numChunkXY.y, numChunkXY.x * max(0, numChunkXY.y-1));
            int2 startIndexHV = int2(gridData.ChunkSize - 1, Sq(gridData.ChunkSize) - gridData.ChunkSize);
            NativeArray<Gate> gates = new(csum(numSpaceHV * gridData.ChunkSize), Temp, UninitializedMemory);
            GetGates(gridData.ChunkSize);
            return gates;

            void GetGates(int chunkSize)
            {
                int mapNumQuadX = chunkSize * numChunkXY.x;
                if(all(numSpaceHV <= int2.zero)) return;
                
                int arrayOffset = max(0,numChunkXY.x - 1) * numChunkXY.y * chunkSize;
                int numIteration = cmax(numSpaceHV);
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
                    int baseIndexV = index * chunkSize + arrayOffset;
                    
                    for (int gateIndex = 0; gateIndex < chunkSize; gateIndex++)
                    {
                        if (index < numSpaceHV[0])
                        {
                            int indexInChunkH = mad(chunkSize, gateIndex, startIndexHV[0]);
                            int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, chunkSize, indexInChunkH);
                            int position = baseIndexH + gateIndex;
                            gates[position] = new Gate(indexInGridH, indexInGridH + 1);
                        }
                        
                        if (index < numSpaceHV[1])
                        {
                            int indexInChunkV = startIndexHV[1] + gateIndex; //top left cell on the chunk + index
                            int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, chunkSize, indexInChunkV);
                            int position = baseIndexV + gateIndex;
                            gates[position] = new Gate(indexInGridV, indexInGridV + mapNumQuadX);
                        }
                    }//end for
                }//end for
            }//end GetGates
        }
        
    }
}
/*
        private void GetGates(int chunkSize, in int2 numChunkXY, in int2 startIndexHV, in int2 numSpaceChunksHV)
        {
            int mapNumQuadX = chunkSize * numChunkXY.x;
            //int numSpaceBetweenChunksH =  max(0,numChunkXY.x - 1) * numChunkXY.y;
            //int numSpaceBetweenChunksV = numChunkXY.x * max(0,numChunkXY.y - 1);
            if(all(numSpaceChunksHV <= int2.zero)) return;
            //if (numSpaceBetweenChunksH <= 0 && numSpaceBetweenChunksV <= 0) return;
            
            int arrayOffset = max(0,numChunkXY.x - 1) * numChunkXY.y * chunkSize;
            
            int numIteration = cmax(numSpaceChunksHV);
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
                int baseIndexV = index * chunkSize + arrayOffset;
                
                for (int gateIndex = 0; gateIndex < chunkSize; gateIndex++)
                {
                    if (index < numSpaceChunksHV[0])
                    {
                        int indexInChunkH = mad(chunkSize, gateIndex, startIndexHV[0]);
                        int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, chunkSize, indexInChunkH);
                        int position = baseIndexH + gateIndex;
                        gates[position] = new Gate(indexInGridH, indexInGridH + 1);
                    }
                    
                    if (index < numSpaceChunksHV[1])
                    {
                        int indexInChunkV = startIndexHV[1] + gateIndex; //top left cell on the chunk + index
                        int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, chunkSize, indexInChunkV);
                        int position = baseIndexV + gateIndex;
                        gates[position] = new Gate(indexInGridV, indexInGridV + mapNumQuadX);
                    }
                }
            }
        }
        */
/*
// ---------------------------------------------------------------------------------------------------------
// HORIZONTAL
// ---------------------------------------------------------------------------------------------------------
void GetHorizontalGates(int chunkSize, int mapNumQuadX, in int2 numChunkXY)
{
    int numSpaceBetweenChunks =  max(0,numChunkXY.x - 1) * numChunkXY.y;
    if (numSpaceBetweenChunks <= 0) return;
    for (int index = 0; index < numSpaceBetweenChunks; index++)
    {
        //Nous Allons utiliser le chunk a gauche
        //le nombre d'espace par ligne est différent du nombre de chunk par ligne de 1
        //il faut donc compenser, formule : x += (y * différence Width (ici 1 donc juste y))
        int chunkIndexH = GetHorizontalChunkIndex(index, numChunkXY.x);
        int baseIndexH = index * chunkSize;
        for (int gateIndex = 0; gateIndex < chunkSize; gateIndex++)
        {
            int position = baseIndexH + gateIndex;
            gates[position] = GetHorizontalGate(gateIndex, chunkIndexH, mapNumQuadX, chunkSize);
        }
    }

    Gate GetHorizontalGate(int gateIndex, int chunkIndexH, int mapNumQuadX, int chunkSize)
    {
        int startCellIndexH = chunkSize - 1;
        int indexInChunkH = mad(chunkSize, gateIndex, startCellIndexH);
        int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapNumQuadX, chunkSize, indexInChunkH);
        return new Gate(indexInGridH, indexInGridH + 1);
    }
    
    int GetHorizontalChunkIndex(int index, int numChunkX)
    {
        int2 spaceCoord = GetXY2(index, numChunkX - 1); //coordonnées correspondant ici à l'espace entre 2 partitions
        int2 chunkCoordH = int2(csum(spaceCoord), spaceCoord.y);
        return GetIndex(chunkCoordH, numChunkX - 1);
    }
}
// ---------------------------------------------------------------------------------------------------------
// VERTICAL
// ---------------------------------------------------------------------------------------------------------
void GetVerticalGates(int chunkSize, int mapNumQuadX, in int2 numChunkXY)
{
    int numSpaceBetweenChunks = numChunkXY.x * max(0,numChunkXY.y - 1);
    if (numSpaceBetweenChunks <= 0) return;
    
    //we place vertical gates after horizontals
    int arrayOffset = max(0,numChunkXY.x - 1) * numChunkXY.y * chunkSize;
    for (int index = 0; index < numSpaceBetweenChunks; index++)
    {
        //Nous Allons utiliser le chunk a gauche
        //le nombre d'espace par ligne est différent du nombre de chunk par ligne de 1
        //il faut donc compenser, formule : x += (y * différence Width (ici 1 donc juste y))
        int chunkIndexV = GetVerticalChunkIndex(index, numChunkXY.x);
        int baseIndexV = index * chunkSize + arrayOffset;
        for (int gateIndex = 0; gateIndex < chunkSize; gateIndex++)
        {
            int position = baseIndexV + gateIndex;
            gates[position] = GetVerticalGate(gateIndex, chunkIndexV, mapNumQuadX, chunkSize);
        }
    }
    
    Gate GetVerticalGate(int gateIndex, int chunkIndexV, int mapNumQuadX, int chunkSize)
    {
        int startCellIndexV = Sq(chunkSize) - chunkSize; //top left cell on the chunk
        int indexInChunkV = startCellIndexV + gateIndex; //top left cell on the chunk + index
        int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapNumQuadX, chunkSize, indexInChunkV);
        return new Gate(indexInGridV, indexInGridV + mapNumQuadX);
    }
    
    int GetVerticalChunkIndex(int index, int numChunkX)
    {
        int2 chunkCoordV = GetXY2(index, numChunkX); //spaceCoord == chunkCoord in this case
        return GetIndex(chunkCoordV, numChunkX);
    }
}
 */
