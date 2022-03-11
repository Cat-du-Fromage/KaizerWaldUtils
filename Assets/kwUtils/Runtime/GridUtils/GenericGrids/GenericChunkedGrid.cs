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
        private float halfChunk;
        private int2 chunkWidthHeight;
        
        public readonly Dictionary<int, T[]> ChunkDictionary;
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize ,int cellSize, Func<int2, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            this.chunkSize = GetChunkSize(chunkSize, cellSize);
            halfChunk = this.chunkSize / 2f;

            chunkWidthHeight = MapWidthHeight / chunkSize;
            
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, GridBounds));
        }
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            this.chunkSize = GetChunkSize(chunkSize, cellSize);
            halfChunk = this.chunkSize / 2f;
            UnityEngine.Debug.Log($"chunkSize = {this.chunkSize}; half : {halfChunk}");
            
            chunkWidthHeight = MapWidthHeight / chunkSize;

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(new GridData(chunkSize, GridBounds));
        }

        private int GetChunkSize(int chunksSize ,int cellSize)
        {
            int value = ceilpow2(chunksSize);
            while (value <= cellSize) { value *= 2; }
            return value;
        }
        //==============================================================================================================
        //Cell Data
        //==============================================================================================================
        public Vector3 GetChunkCenter(int chunkIndex)
        {
            float2 chunkCoord = ((chunkIndex.GetXY2(chunkWidthHeight.x) * chunkSize) + new float2(halfChunk));
            return new Vector3(chunkCoord.x, 0, chunkCoord.y);
        }
        
        //==============================================================================================================
        //Connection between chunk and Grid
        //==============================================================================================================
        public int GetChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapWidthHeight.x);
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize);
            return chunkCoord.GetIndex(chunkWidthHeight.x);
        }
        
        public int GetCellChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapWidthHeight.x);
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize);
            int2 cellCoordInChunk = cellCoord - (chunkCoord * chunkSize);
            return cellCoordInChunk.GetIndex(chunkSize);
        }
        //==============================================================================================================
        //Set both grid value and Chunk Value
        //==============================================================================================================
        private void UpdateChunk(int gridIndex, T value)
        {
            int2 cellCoord = gridIndex.GetXY2(MapWidthHeight.x);
            //Chunk Index
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize);
            int chunkIndex = chunkCoord.GetIndex(chunkWidthHeight.x);
            //CellIndex
            int2 cellCoordInChunk = cellCoord - (chunkCoord * chunkSize);
            int cellIndexInChunk = cellCoordInChunk.GetIndex(chunkSize);
            
            ChunkDictionary[chunkIndex][cellIndexInChunk] = value;
        }
        
        public sealed override void SetValue(int index, T value)
        {
            base.SetValue(index, value);
            UpdateChunk(index, value);
        }

        //==============================================================================================================
        //Get Values inside a chunk
        //==============================================================================================================
        public T[] GetValues(int index) => ChunkDictionary[index];
        public T[] GetValues(int x, int y) => ChunkDictionary[y * GridWidth + x];
        public T[] GetValues(int2 coord) => ChunkDictionary[coord.y * GridWidth + coord.x];


        

        
    }
}