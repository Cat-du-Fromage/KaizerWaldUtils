using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils.KWGenericGrid
{
    public sealed class GenericChunkedGrid<T> : GenericGrid<T>
    where T : struct
    {
        //Chunk Fields
        private int chunkSize;
        private int2 chunkWidthHeight;
        
        private Dictionary<int, T[]> chunkDictionary;
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize ,int cellSize, Func<int2, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            this.chunkSize = chunkSize;
            chunkWidthHeight = MapWidthHeight / chunkSize;
            
            chunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, GridBounds));
        }
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize ,int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            this.chunkSize = chunkSize;
            chunkWidthHeight = MapWidthHeight / chunkSize;

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            chunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, GridBounds));
        }

        //==============================================================================================================
        //Set both grid value and Chunk Value
        //==============================================================================================================
        public void GetCellChunkIndexFromGridArrayIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(GridWidth);
            
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize); //TEST REQUIRED!
            int chunkIndex = chunkCoord.y * chunkWidthHeight.x + chunkCoord.x;
            
            //FIND INDEX IN CHUNK!
            //int2 chunkCoord = chunkIndex.GetXY2(gridData.NumChunkXY.x);
            // cellCoordInChunk = cellIndexInsideChunk.GetXY2(gridData.ChunkSize);
            //int2 cellGridCoord = cellCoordInChunk.GetGridCellCoordFromChunkCellCoord(gridData.ChunkSize, chunkCoord);
            //return (cellGridCoord.y * gridData.MapSize.x) + cellGridCoord.x;
            
            
            
            int2 cellChunkCoord = cellCoord / cellCoord;
        }
        
        public void UpdateChunk()
        {
            
        }
        
        public sealed override void SetValue(int index, T value)
        {
            base.SetValue(index, value);
            UpdateChunk();
        }

        //==============================================================================================================
        //Get Values inside a chunk
        //==============================================================================================================
        public T[] GetValues(int index) => chunkDictionary[index];
        public T[] GetValues(int x, int y) => chunkDictionary[y * GridWidth + x];
        public T[] GetValues(int2 coord) => chunkDictionary[coord.y * GridWidth + coord.x];


        

        
    }
}