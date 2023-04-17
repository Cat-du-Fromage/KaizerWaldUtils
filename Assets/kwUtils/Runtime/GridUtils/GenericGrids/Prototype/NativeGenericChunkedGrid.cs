using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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

        private float HalfChunk => ChunkSize / 2f;
        private int2 NumCellsXY => NumChunkXY * ChunkSize;
        
        //==============================================================================================================
        //CONSTRUCTOR
        //==============================================================================================================
        
        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, bool isCentered = false) 
            : base(in numCellXY, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, T[] source, bool isCentered = false) 
            : base(in numCellXY, source, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, NativeArray<T> source, bool isCentered = false) 
            : base(in numCellXY, source, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, Func<T[]> providerFunction, bool isCentered = false) 
            : base(in numCellXY, providerFunction, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
        }

        public NativeGenericChunkedGrid(int chunkSize, in int2 numCellXY, Func<NativeArray<T>> providerFunction, bool isCentered = false) 
            : base(in numCellXY, providerFunction, isCentered)
        {
            ChunkSize = chunkSize;
            NumChunkXY = numCellXY / chunkSize;
        }
        
        //==============================================================================================================
        // CHUNK INFORMATION
        //==============================================================================================================
        
        public float3 GetChunkCenter(int chunkIndex)
        {
            float2 chunkCoord = GetXY2(chunkIndex,NumChunkXY.x) * ChunkSize + float2(HalfChunk) - Offset;
            return new float3(chunkCoord.x, 0, chunkCoord.y);
        }

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
    }
}
