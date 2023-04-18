using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static System.Array;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;

using float2 = Unity.Mathematics.float2; 

namespace KWUtils
{
    public sealed class NativeGenericChunkedGrid<T> : NativeGenericGrid<T>
    where T : struct
    {
        private readonly int ChunkSize;
        private readonly int2 NumChunkXY;
        public new event Action OnGridChange;

        private int NumCellsPerChunk => ChunkSize * ChunkSize;
        private float HalfChunk => ChunkSize / 2f;
        public GridData GridData => new GridData(NumCellXY, ChunkSize);
        
        //==============================================================================================================
        //CONSTRUCTOR
        //==============================================================================================================
        
        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, bool isCentered = false) 
            : base(in numCellXY, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
            GridArray.OrderNativeArrayByChunk(GridData);
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, T[] source, bool isCentered = false) 
            : base(in numCellXY, source, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
            GridArray.OrderNativeArrayByChunk(GridData);
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, NativeArray<T> source, bool isCentered = false) 
            : base(in numCellXY, source, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
            GridArray.OrderNativeArrayByChunk(GridData);
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, Func<T[]> providerFunction, bool isCentered = false) 
            : base(in numCellXY, providerFunction, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
            GridArray.OrderNativeArrayByChunk(GridData);
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, Func<NativeArray<T>> providerFunction, bool isCentered = false) 
            : base(in numCellXY, providerFunction, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
            GridArray.OrderNativeArrayByChunk(GridData);
        }

        //==============================================================================================================
        // CHUNK INFORMATION
        //==============================================================================================================
        
        public float3 GetChunkCenter(int chunkIndex)
        {
            float2 chunkCoord = GetXY2(chunkIndex,NumChunkXY.x) * ChunkSize + float2(HalfChunk) - Offset;
            return new float3(chunkCoord.x, 0, chunkCoord.y);
        }
        
        //==============================================================================================================
        // Part du principe que "GridArray" est ordré!
        //==============================================================================================================
        
        public sealed override void CopyFrom(T[] otherArray)
        {
            using NativeArray<T> temp = otherArray.ToNativeArray().OrderNativeArrayByChunk(GridData);
            GridArray.CopyFrom(temp);
        }
        
        public sealed override void CopyFrom(NativeArray<T> otherArray)
        {
            using NativeArray<T> temp = otherArray.OrderNativeArrayByChunk(GridData);
            GridArray.CopyFrom(temp);
        }
        
        // KEY FEATURE
        //Permet d'obtenir l'index comme si le tableau n'était pas ordré
        private int GetOffsetIndex(int cellIndex)
        {
            int chunkIndex = GetChunkIndexFromGridIndex(cellIndex, ChunkSize, NumChunkXY.x);
            int startIndexChunk = chunkIndex * NumCellsPerChunk;
            return startIndexChunk + GetCellChunkIndexFromGridIndex(cellIndex, ChunkSize, NumChunkXY.x);
        }
        
        public override float3 GetCellCenter(int cellIndex)
        {
            int offsetIndex = GetOffsetIndex(cellIndex);
            return base.GetCellCenter(offsetIndex);
        }
        
        public override int IndexFromPosition(in float3 position)
        {
            int gridIndex = IsCentered ? GetIndexFromPositionOffset(position, NumCellXY) : GetIndexFromPosition(position, NumCellXY);
            return GetOffsetIndex(gridIndex);
        }
        
        public override T ElementAt(int cellIndex)
        {
            int indexOffset = GetOffsetIndex(cellIndex);
            return GridArray[indexOffset];
        }
        public override T this[int cellIndex]
        {
            get => ElementAt(cellIndex);
            set => SetValue(cellIndex, value);
        }
        
        public override void SetValue(int index, T value)
        {
            GridArray[GetOffsetIndex(index)] = value;
            OnGridChange?.Invoke();
        }
        
        public NativeSlice<T> GetChunkCellsAt(int chunkIndex)
        {
            int startIndex = chunkIndex * NumCellsPerChunk;
            return GridArray.Slice(startIndex, NumCellsPerChunk);
        }
/*
        //==============================================================================================================
        // Part du principe que "GridArray" n'est PAS ordré!
        //==============================================================================================================
        // Retrive Elements by chunks
        public NativeArray<T> GetChunkCellsAt(int index, Allocator allocator = Allocator.Temp)
        {
            int chunkNumCells = ChunkSize * ChunkSize;
            
            NativeArray<T> temp = new(chunkNumCells, allocator, NativeArrayOptions.UninitializedMemory);
            int2 chunkCoord = GetXY2(index, NumChunkXY.x);
            int startX = ChunkSize * chunkCoord.x;
            int startY = ChunkSize * NumChunkXY.x * NumCellsXY.x;
            
            for (int i = 0; i < ChunkSize; i++)
            {
                startY += i * NumCellsXY.x;
                int startSrcIndex = startY * NumCellsXY.x + startX;
                int startDstIndex = i * chunkNumCells;
                NativeArray<T>.Copy(GridArray,startSrcIndex,temp,startDstIndex,ChunkSize);
            }
            return temp;
        }
*/
        //==============================================================================================================
        // DESTRUCTOR
        //==============================================================================================================
        public sealed override void ClearEvents()
        {
            if (OnGridChange == null) return;
            ForEach(OnGridChange.GetInvocationList(),action => OnGridChange -= (Action)action);
            //foreach (Delegate action in OnGridChange.GetInvocationList()) { OnGridChange -= (Action)action; }
        }

        ~NativeGenericChunkedGrid() => ClearEvents();
    }
}
