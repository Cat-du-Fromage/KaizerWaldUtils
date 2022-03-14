using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;

namespace KWUtils.KWGenericGrid
{
    public sealed class GenericChunkedGrid<T> : GenericGrid<T>
    where T : struct
    {
        //Chunk Fields
        private int chunkSize;
        private float halfChunk;
        private int2 chunkWidthHeight;
        
        public GridData gridData { get; private set; }
        public new event Action OnGridChange;
        public Dictionary<int, T[]> ChunkDictionary { get; private set; }

        public GenericChunkedGrid(in int2 mapSize, int chunkSize ,int cellSize, Func<int2, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            this.chunkSize = GetChunkSize(chunkSize, cellSize);
            halfChunk = this.chunkSize / 2f;

            chunkWidthHeight = MapWidthHeight / chunkSize;

            gridData = new GridData(cellSize, chunkSize, GridBounds);
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(gridData);
        }
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            this.chunkSize = GetChunkSize(chunkSize, cellSize);
            halfChunk = this.chunkSize / 2f;

            chunkWidthHeight = MapWidthHeight / chunkSize;

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            gridData = new GridData(cellSize, chunkSize, GridBounds);
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(gridData);
        }

        /// <summary>
        /// Make sur ChunkSize is Greater than cellSize
        /// </summary>
        private int GetChunkSize(int chunksSize ,int cellSize)
        {
            int value = ceilpow2(chunksSize);
            while (value <= cellSize) { value *= 2; }
            return value;
        }
        
        //Clear Events
        public sealed override void ClearEvents()
        {
            if (OnGridChange == null) return;
            foreach (Delegate action in OnGridChange.GetInvocationList())
            {
                OnGridChange -= (Action)action;
            }
        }
        
        //Accessors
        public int2 ChunkBound => chunkWidthHeight;
        
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
        public int ChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapWidthHeight.x);
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize);
            return chunkCoord.GetIndex(chunkWidthHeight.x);
        }
        
        public int CellChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapWidthHeight.x);
            int2 chunkCoord = (int2)floor(cellCoord / chunkSize);
            int2 cellCoordInChunk = cellCoord - (chunkCoord * chunkSize);
            return cellCoordInChunk.GetIndex(chunkSize);
        }

        //==============================================================================================================
        //Set both grid value and Chunk Value
        //==============================================================================================================
        
        public sealed override void CopyFrom(T[] otherArray)
        {
            base.CopyFrom(otherArray);
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(gridData);
        }

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
        
        public void UpdateGrid(int chunkIndex, T[] values)
        {
            //int numCellInChunk = Sq(gridData.ChunkCellWidth);
            for (int i = 0; i < values.Length; i++)
            {
                GridArray[chunkIndex.GetGridCellIndexFromChunkCellIndex(gridData, i)] = values[i]; //CALCUL FAUX!!!
            }
        }
        
        public sealed override void SetValue(int index, T value)
        {
            GridArray[index] = value;
            UpdateChunk(index, value);
            OnGridChange?.Invoke();
        }
        
        public void SetValues(int chunkIndex, T[] values)
        {
            ChunkDictionary[chunkIndex] = values;
            UpdateGrid(chunkIndex, values);
            OnGridChange?.Invoke();
        }

        //==============================================================================================================
        //Get Values inside a chunk
        //==============================================================================================================
        public T[] GetValues(int index) => ChunkDictionary[index];
        public T[] GetValues(int x, int y) => ChunkDictionary[y * GridBounds.x + x];
        public T[] GetValues(int2 coord) => ChunkDictionary[coord.y * GridBounds.x + coord.x];
    }
}