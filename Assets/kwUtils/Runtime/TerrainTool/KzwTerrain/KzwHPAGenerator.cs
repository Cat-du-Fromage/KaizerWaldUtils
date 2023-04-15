using System;
using System.Collections;
using System.Collections.Generic;
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

namespace KWUtils
{
    [RequireComponent(typeof(KzwTerrainGenerator))]
    public partial class KzwHPAGenerator : MonoBehaviour
    {
        private KzwTerrainGenerator terrain;
        private List<Gate> TerrainGates;
        private Gate[] MapGates;

        private void Awake()
        {
            bool terrainExist = TryGetComponent(out terrain);
        }

        private void Start()
        {
            using NativeArray<Gate> tempGates = BuildGate2(terrain.Grid.GridData);
            MapGates = tempGates.ToArray();
            //BuildGates(terrain.Grid.GridData);
        }

        private bool IsSideOpen()
        {
            return true;
        }

        private void BuildGates(in GridData gridData)
        {
            int2 numChunksAxis = terrain.Settings.NumChunkAxis;
            int chunkQuadsPerLine = terrain.Settings.ChunkQuadsPerLine;
            
            int baseCapacity = (numChunksAxis.x - 1) * numChunksAxis.y;
            TerrainGates = new List<Gate>(baseCapacity * 2 * chunkQuadsPerLine);
            
            GetHorizontalGates(TerrainGates, numChunksAxis, chunkQuadsPerLine);
            GetVerticalGates(TerrainGates, numChunksAxis, chunkQuadsPerLine);
            
            // On va traverser les espaces entre les partitions
            // + = porte
            // [m]+[n]+[o]+[p]
            // [i]+[j]+[k]+[l]
            // [e]+[f]+[g]+[h]
            // [a]+[b]+[c]+[d]
            // ATTENTION !!!
            // porte par ligne = (Nombre Partition sur X) - 1
            // MAIS! la hauteur est INCHANGEE!!! donc bounds: int2(numChunkX-1, numChunkX)
            void GetHorizontalGates(List<Gate> terrainGates, in int2 numChunkXY, int chunkSize)
            {
                int mapSizeX = numChunkXY.x * chunkSize;
                int width = numChunkXY.x - 1;
                int numSpaceBetweenChunks = width * numChunkXY.y;
                for (int i = 0; i < numSpaceBetweenChunks; i++)
                {
                    //coordonnées correspondant ici à l'espace entre 2 partitions
                    (int x, int y) = GetXY(i, width);
                    
                    //Nous Allons utiliser le chunk a gauche
                    //le nombre d'espace par ligne est différent du nombre de chunk par ligne de 1
                    //il faut donc compenser, formule : x += (y * différence Width (ici 1 donc juste y))
                    int2 chunkCoord = int2(x+y, y);
                    int chunkIndex = GetIndex(chunkCoord, numChunkXY.x - 1);
                    int startCellIndex = chunkSize - 1;
                    for (int j = 0; j < chunkSize; j++)
                    {
                        int indexInChunk = startCellIndex + (chunkSize * j);
                        int indexInGrid = GetGridCellIndexFromChunkCellIndex(chunkIndex, mapSizeX, chunkSize, indexInChunk);
                        Gate gate = new Gate(indexInGrid, indexInGrid + 1);
                        terrainGates.Add(gate);
                    }
                }
            }


            
            void GetVerticalGates(List<Gate> terrainGates, in int2 numChunkXY, int chunkSize)
            {
                int mapSizeX = numChunkXY.x * chunkSize;
                int width = numChunkXY.x;
                int numSpaceBetweenChunks = width * (numChunkXY.y-1);
                for (int i = 0; i < numSpaceBetweenChunks; i++)
                {
                    //coordonnées correspondant ici à l'espace entre 2 partitions
                    int2 spaceCoord = GetXY2(i, width);
                    int chunkIndex = GetIndex(spaceCoord, numChunkXY.x); //spaceCoord == chunkCoord in this case
                    int startCellIndex = Sq(chunkSize) - chunkSize;
                    for (int j = 0; j < chunkSize; j++)
                    {
                        int indexInChunk = startCellIndex + j;
                        int indexInGrid = GetGridCellIndexFromChunkCellIndex(chunkIndex, mapSizeX, chunkSize, indexInChunk);
                        Gate gate = new Gate(indexInGrid, indexInGrid + mapSizeX);
                        terrainGates.Add(gate);
                    }
                }
            }
        }

        private NativeArray<Gate> BuildGate2(in GridData gridData)
        {
            int numSpaceBetweenChunks = (gridData.NumChunkXY.x - 1) * gridData.NumChunkXY.y;
            int arrayCapacity = numSpaceBetweenChunks * gridData.ChunkSize * 2;
            NativeArray<Gate> gates = new(arrayCapacity, TempJob, UninitializedMemory);
            JGetGates job = new JGetGates
            {
                ChunkSize = gridData.ChunkSize,
                NumChunkXY = gridData.NumChunkXY,
                Gates = gates
            };
            job.ScheduleParallel(numSpaceBetweenChunks, JobWorkerCount - 1, default).Complete();
            return gates;
        }

        // On va traverser les espaces entre les partitions
        // + = porte
        // [m]+[n]+[o]+[p]
        // [i]+[j]+[k]+[l]
        // [e]+[f]+[g]+[h]
        // [a]+[b]+[c]+[d]
        // ATTENTION !!!
        // porte par ligne = (Nombre Partition sur X) - 1
        // MAIS! la hauteur est INCHANGEE!!! donc bounds: int2(numChunkX-1, numChunkX)
        public struct JGetGates : IJobFor
        {
            [ReadOnly] public int ChunkSize;
            [ReadOnly] public int2 NumChunkXY;

            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<Gate> Gates;

            public void Execute(int index)
            {
                int mapSizeX = NumChunkXY.x * ChunkSize;
                int arrayOffset = NumChunkXY.x * (NumChunkXY.y - 1) * ChunkSize;
                
                //Nous Allons utiliser le chunk a gauche
                //le nombre d'espace par ligne est différent du nombre de chunk par ligne de 1
                //il faut donc compenser, formule : x += (y * différence Width (ici 1 donc juste y))
                
                //Horizontal part
                int2 spaceCoord = GetXY2(index, NumChunkXY.x - 1); //coordonnées correspondant ici à l'espace entre 2 partitions
                int2 chunkCoordH = int2(csum(spaceCoord), spaceCoord.y);
                int chunkIndexH = GetIndex(chunkCoordH, NumChunkXY.x - 1);
                int startCellIndexH = ChunkSize - 1;
                
                // Vertical
                int2 chunkCoordV = GetXY2(index, NumChunkXY.x); //spaceCoord == chunkCoord in this case
                int chunkIndexV = GetIndex(chunkCoordV, NumChunkXY.x);
                int startCellIndexV = Sq(ChunkSize) - ChunkSize;
                
                for (int j = 0; j < ChunkSize; j++)
                {
                    int baseIndex = index * ChunkSize + j;
                    // Horizontal
                    int indexInChunkH = mad(ChunkSize, j, startCellIndexH);
                    int indexInGridH = GetGridCellIndexFromChunkCellIndex(chunkIndexH, mapSizeX, ChunkSize, indexInChunkH);
                    Gates[baseIndex] = new Gate(indexInGridH, indexInGridH + 1);
                    
                    // Vertical
                    int indexInChunkV = startCellIndexV + j;
                    int indexInGridV = GetGridCellIndexFromChunkCellIndex(chunkIndexV, mapSizeX, ChunkSize, indexInChunkV);
                    Gates[baseIndex + arrayOffset] = new Gate(indexInGridV, indexInGridV + mapSizeX);
                }
            }
        }
    }
}
