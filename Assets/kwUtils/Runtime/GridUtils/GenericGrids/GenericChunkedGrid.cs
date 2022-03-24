using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
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
        private readonly int ChunkSize;
        private readonly int2 NumChunkXY;
        public new event Action OnGridChange;
        public Dictionary<int, T[]> ChunkDictionary { get; private set; }
        public sealed override GridData GridData => new GridData(MapXY, CellSize, ChunkSize);
        //==============================================================================================================
        //Constructors
        //============
        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize, Func<int, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
            this.ChunkSize = GetChunkSize(chunkSize, cellSize);
            NumChunkXY = mapSize >> floorlog2(chunkSize);

            ChunkDictionary = new Dictionary<int, T[]>(NumChunkXY.x * NumChunkXY.y);
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(GridData);
        }
        
        public GenericChunkedGrid(in int2 mapSize, int chunkSize, int cellSize = 1, [CanBeNull] Func<T[]> providerFunction = null) : base(in mapSize, cellSize)
        {
            this.ChunkSize = GetChunkSize(chunkSize, cellSize);
            NumChunkXY = mapSize >> floorlog2(chunkSize);

            providerFunction?.Invoke()?.CopyTo((Span<T>) GridArray); //CAREFULL may switch with Memory<T>!
            ChunkDictionary = new Dictionary<int, T[]>(NumChunkXY.x * NumChunkXY.y);
            ChunkDictionary = GridArray.GetGridValueOrderedByChunk(GridData);
        }
        
        /// Make sur ChunkSize is Greater than cellSize
        private int GetChunkSize(int chunksSize ,int cellSize)
        {
            int value = ceilpow2(chunksSize);
            while (value <= cellSize) { value *= 2; }
            return value;
        }
        //==============================================================================================================

        //Clear Events
        public sealed override void ClearEvents()
        {
            if (OnGridChange == null) return;
            foreach (Delegate action in OnGridChange.GetInvocationList())
            {
                OnGridChange -= (Action)action;
            }
        }

        //==============================================================================================================
        //Cell Data
        //==========
        public Vector3 GetChunkCenter(int chunkIndex)
        {
            float2 chunkCoord = ((chunkIndex.GetXY2(NumChunkXY.x) * ChunkSize) + new float2(ChunkSize/2f));
            return new Vector3(chunkCoord.x, 0, chunkCoord.y);
        }
        public Vector3 GetChunkCellCenter(int chunkIndex, int cellIndexInChunk)
        {
            int indexInGrid = chunkIndex.GetGridCellIndexFromChunkCellIndex(GridData, cellIndexInChunk);
            return GetCellCenter(indexInGrid);
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Connection between chunk and Grid
        //==================================
        public int ChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapXY.x);
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            return chunkCoord.GetIndex(NumChunkXY.x);
        }
        
        public int CellChunkIndexFromGridIndex(int gridIndex)
        {
            int2 cellCoord = gridIndex.GetXY2(MapXY.x);
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            int2 cellCoordInChunk = cellCoord - (chunkCoord * ChunkSize);
            return cellCoordInChunk.GetIndex(ChunkSize);
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Set both grid value and Chunk Value
        //====================================
        public sealed override void CopyFrom(T[] otherArray)
        {
            base.CopyFrom(otherArray);
            ChunkDictionary.PopulateChunkedGrid(GridArray, GridData);
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Update Chunk or Array according to the type of value set
        //=========================================================
        
        /// Chunk : Update made after the Array was modified
        private void UpdateChunk(int gridIndex, T value)
        {
            int2 cellCoord = gridIndex.GetXY2(MapXY.x);
            //Chunk Index
            int2 chunkCoord = (int2)floor(cellCoord / ChunkSize);
            int chunkIndex = chunkCoord.GetIndex(NumChunkXY.x);
            //CellIndex
            int2 cellCoordInChunk = cellCoord - (chunkCoord * ChunkSize);
            int cellIndexInChunk = cellCoordInChunk.GetIndex(ChunkSize);
            
            ChunkDictionary[chunkIndex][cellIndexInChunk] = value;
        }
        
        /// Array : Update made after a Chunk was modified
        private void UpdateGrid(int chunkIndex, T[] values)
        {
            for (int i = 0; i < values.Length; i++)
                GridArray[chunkIndex.GetGridCellIndexFromChunkCellIndex(GridData, i)] = values[i];
        }
        
        private void UpdateGrid(int chunkIndex, NativeSlice<T> values)
        {
            for (int i = 0; i < values.Length; i++)
                GridArray[chunkIndex.GetGridCellIndexFromChunkCellIndex(GridData, i)] = values[i];
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Set Values inside a chunk
        public sealed override void SetValue(int index, T value)
        {
            GridArray[index] = value;
            UpdateChunk(index, value);
            OnGridChange?.Invoke();
        }
        public void SetValues(int chunkIndex, T[] values)
        {
            values.CopyTo((Span<T>)ChunkDictionary[chunkIndex]);
            UpdateGrid(chunkIndex, values);
            OnGridChange?.Invoke();
        }
        
        public void SetValues(int chunkIndex, NativeSlice<T> values)
        {
            values.CopyTo(ChunkDictionary[chunkIndex]);
            UpdateGrid(chunkIndex, values);
            OnGridChange?.Invoke();
        }
        //==============================================================================================================
        
        //==============================================================================================================
        //Get Values inside a chunk
        public T[] GetValues(int index) => ChunkDictionary[index];
        public T[] GetValues(int x, int y) => ChunkDictionary[y * NumCellXY.x + x];
        public T[] GetValues(int2 coord) => ChunkDictionary[coord.y * NumCellXY.x + coord.x];
        //==============================================================================================================
        
        //==============================================================================================================
        //Get/Set Value By Index
        public new T[] this[int chunkIndex]
        {
            get => ChunkDictionary[chunkIndex];
            set => SetValues(chunkIndex, value);
        }
        //==============================================================================================================
    }
}