#define EnableBurst



#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.


using KWUtils.KwTerrain;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
using static KWUtils.KWChunk;
using int2 = Unity.Mathematics.int2;
/*
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<bool>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<int>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<float>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<float2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<float3>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<Vector2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JOrderArrayByChunkIndex<Vector3>))]

[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<bool>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<int>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<float>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<float2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<float3>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<Vector2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JConvertGridBigToSmall<Vector3>))]

[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<bool>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<int>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<float>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<float2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<float3>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<Vector2>))]
[assembly: RegisterGenericJobType(typeof(KWUtils.JSharedOrderArrayByChunkIndex<Vector3>))]
*/
namespace KWUtils
{

#pragma warning disable 0219
    internal class KwChunkJobsGenericGeneration
    {

        private KwChunkJobsGenericGeneration(){ }

        private void GenJOrderArrayByChunkIndex()
        {
            JOrderArrayByChunkIndex<bool>    jbool    = new ();
            JOrderArrayByChunkIndex<int>     jint     = new ();
            JOrderArrayByChunkIndex<float>   jfloat   = new ();
            JOrderArrayByChunkIndex<float2>  jfloat2  = new ();
            JOrderArrayByChunkIndex<float3>  jfloat3  = new ();
            JOrderArrayByChunkIndex<Vector2> jVector2 = new ();
            JOrderArrayByChunkIndex<Vector3> jVector3 = new ();
        }

        private void GenJConvertGridBigToSmall()
        {
            JConvertGridBigToSmall<bool>    jbool    = new ();
            JConvertGridBigToSmall<int>     jint     = new ();
            JConvertGridBigToSmall<float>   jfloat   = new ();
            JConvertGridBigToSmall<float2>  jfloat2  = new ();
            JConvertGridBigToSmall<float3>  jfloat3  = new ();
            JConvertGridBigToSmall<Vector2> jVector2 = new ();
            JConvertGridBigToSmall<Vector3> jVector3 = new ();
        }
        
        private void GenJSharedOrderArrayByChunkIndex()
        {
            JSharedOrderArrayByChunkIndex<bool>    jbool    = new ();
            JSharedOrderArrayByChunkIndex<int>     jint     = new ();
            JSharedOrderArrayByChunkIndex<float>   jfloat   = new ();
            JSharedOrderArrayByChunkIndex<float2>  jfloat2  = new ();
            JSharedOrderArrayByChunkIndex<float3>  jfloat3  = new ();
            JSharedOrderArrayByChunkIndex<Vector2> jVector2 = new ();
            JSharedOrderArrayByChunkIndex<Vector3> jVector3 = new ();
        }
        
    }
#pragma warning restore 0219
    

    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // Grid Representation
    // chunk0: 4️⃣5️⃣ chunk1: 6️⃣7️⃣ 
    // chunk0: 0️⃣1️⃣ chunk1: 2️⃣3️⃣
    //Before SLice
    // 0️⃣1️⃣2️⃣3️⃣4️⃣5️⃣6️⃣7️⃣ 
    //After Slice
    // ✂ (chunk0): 0️⃣1️⃣4️⃣5️⃣ ✂ (chunk1): 2️⃣3️⃣6️⃣7️⃣ 
    [BurstCompile]
    public struct JOrderArrayByChunkIndex<T> : IJobFor
    where T : struct
    {
        [ReadOnly] private readonly int CellSize;
        [ReadOnly] private readonly int ChunkSize;
        [ReadOnly] private readonly int NumCellInChunkX;
        [ReadOnly] private readonly int NumCellX;
        [ReadOnly] private readonly int NumChunkX;

        [ReadOnly, NativeDisableParallelForRestriction]  private NativeArray<T> UnsortedArray;
        [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<T> SortedArray;
        
        public JOrderArrayByChunkIndex (in GridData gridData, NativeArray<T> unsortedArray, NativeArray<T> sortedArray)
        {
            CellSize = gridData.CellSize;
            ChunkSize = gridData.ChunkSize;
            NumCellInChunkX = gridData.NumCellInChunkX;
            NumCellX = gridData.NumCellXY.x;
            NumChunkX = gridData.NumChunkXY.x;
            UnsortedArray = unsortedArray;
            SortedArray = sortedArray;
        }
        
        public JOrderArrayByChunkIndex (in KwTerrainData data, NativeArray<T> unsortedArray, NativeArray<T> sortedArray)
        {
            CellSize = 1;
            ChunkSize = data.ChunkSize.x;
            NumCellInChunkX = data.ChunkVerticesXZ.x;
            NumCellX = data.TerrainSizeXZ.x;
            NumChunkX = data.TerrainSizeXZ.x / data.ChunkSize.x;
            UnsortedArray = unsortedArray;
            SortedArray = sortedArray;
        }
        
        public void Execute(int index)
        {
            int2 cellCoord = GetXY2(index,NumCellX);
            
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);
            int2 coordInChunk = cellCoord - (chunkCoord * NumCellInChunkX);

            int indexCellInChunk = GetIndex(coordInChunk, NumCellInChunkX);
            int chunkIndex = GetIndex(chunkCoord, NumChunkX);
            int totalCellInChunk = NumCellInChunkX * NumCellInChunkX;
            int indexFinal = mad(chunkIndex, totalCellInChunk, indexCellInChunk);
            
            SortedArray[indexFinal] = UnsortedArray[index];
        }
    }
    
    
    [BurstCompile]
    public struct JConvertGridBigToSmall<T> : IJobFor
    where T : struct
    {
        [ReadOnly] private readonly GridData BiggerGridData;

        [ReadOnly, NativeDisableParallelForRestriction]  private NativeArray<T> BigGridToConvert; //Smaller length
        [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<T> GridConverted; //Length * (OldCell/NewCell)^2

        public JConvertGridBigToSmall(in GridData biggerGridData, NativeArray<T> bigGridToConvert, NativeArray<T> gridConverted)
        {
            BiggerGridData = biggerGridData;
            BigGridToConvert = bigGridToConvert;
            GridConverted = gridConverted;
        }
        
        public void Execute(int index) // index = chunk index(GridToConvert)
        {
            for (int i = 0; i < BiggerGridData.TotalCellInChunk; i++)
            {
                int indexSmall = GetGridCellIndexFromChunkCellIndex(index, BiggerGridData, i);
                GridConverted[indexSmall] = BigGridToConvert[index];
            }
        }
    }

    //==================================================================================================================
    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // ✂ 1️⃣2️⃣3️⃣ ✂ 4️⃣5️⃣6️⃣ ✂ 7️⃣8️⃣9️⃣
    /*
#if EnableBurst
    [BurstCompile]
#endif
    public struct JOrderArrayByChunkIndex2<T> : IJobFor
    where T : struct
    {
        [ReadOnly] public int CellSize;
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int NumCellX;
        [ReadOnly] public int NumChunkX;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<T> UnsortedArray;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<T> SortedArray;
        public void Execute(int index)
        {
            int chunkCellWidth = ChunkSize / CellSize;
            
            int2 cellCoord = index.GetXY2(NumCellX);
            float ratioChunkCell = CellSize / (float)ChunkSize;
            
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratioChunkCell);
            int2 coordInChunk = cellCoord - (int2)floor((float2)chunkCoord * chunkCellWidth);

            int indexInChunk = mad(coordInChunk.y, chunkCellWidth, coordInChunk.x);
            int chunkIndex = mad(chunkCoord.y, NumChunkX, chunkCoord.x);
            int totalCellInChunk = Sq(chunkCellWidth);

            SortedArray[chunkIndex * totalCellInChunk + indexInChunk] = UnsortedArray[index];
        }
    }
    */
    
    // The Job will "slice" the array and reorder them
    // at the end when we cut the array given the number of cell in one chunk
    // we only get the value owned by the chunk
    // Grid Representation
    // chunk0: 4️⃣5️⃣ chunk1: 6️⃣7️⃣ 
    // chunk0: 0️⃣1️⃣ chunk1: 2️⃣3️⃣
    //Before SLice
    // 0️⃣1️⃣2️⃣3️⃣4️⃣5️⃣6️⃣7️⃣ 
    //After Slice
    // ✂ (chunk0): 0️⃣1️⃣4️⃣5️⃣ ✂ (chunk1): 2️⃣3️⃣6️⃣7️⃣ 
    [BurstCompile]
    public struct JSharedOrderArrayByChunkIndex<T> : IJobFor
    where T : struct
    {
        [ReadOnly] private readonly int CellSize;
        [ReadOnly] private readonly int ChunkSize;
        [ReadOnly] private readonly int NumCellInChunkX;
        [ReadOnly] private readonly int NumCellX;
        [ReadOnly] private readonly int NumChunkX;

        [ReadOnly, NativeDisableParallelForRestriction]  private NativeArray<T> UnsortedArray;
        [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<T> SortedArray;
        
        public JSharedOrderArrayByChunkIndex (in GridData gridData, NativeArray<T> unsortedArray, NativeArray<T> sortedArray)
        {
            CellSize = gridData.CellSize;
            ChunkSize = gridData.ChunkSize;
            NumCellInChunkX = gridData.NumCellInChunkX;
            NumCellX = gridData.NumCellXY.x;
            NumChunkX = gridData.NumChunkXY.x;
            UnsortedArray = unsortedArray;
            SortedArray = sortedArray;
        }
        
        public JSharedOrderArrayByChunkIndex (in KwTerrainData data, NativeArray<T> unsortedArray, NativeArray<T> sortedArray)
        {
            CellSize = 1;
            ChunkSize = data.ChunkSize.x;
            NumCellInChunkX = data.ChunkVerticesXZ.x;
            NumCellX = data.TerrainVerticesXZ.x;
            NumChunkX = data.TerrainSizeXZ.x / data.ChunkSize.x;
            UnsortedArray = unsortedArray;
            SortedArray = sortedArray;
        }
        
        public void Execute(int index)
        {
            int2 cellCoord = GetXY2(index,NumCellX);
            
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);
            
            //=================================================
            //Need to offset after Gap
            //think of shared vertices as "insertion" in the grid
            int offsetX = select(1,0, chunkCoord.x == 0);
            int offsetY = select(1,0, chunkCoord.y == 0);
            chunkCoord -= int2(offsetX, offsetY);
            //=================================================
            
            int2 coordInChunk = cellCoord - (chunkCoord * NumCellInChunkX);
            
            int indexCellInChunk = GetIndex(coordInChunk,NumCellInChunkX);
            int chunkIndex = GetIndex(chunkCoord,NumChunkX);
            int totalCellInChunk = NumCellInChunkX * NumCellInChunkX;

            int indexFinal = mad(chunkIndex, totalCellInChunk, indexCellInChunk);

            SortedArray[indexFinal] = UnsortedArray[index];
            
            //is on CHUNK edge BUT not Grid Edge
            int cellIndexInNextChunk;
            
            if (coordInChunk.x == NumCellInChunkX - 1 && cellCoord.x != NumCellX - 1)
            {
                cellIndexInNextChunk = GetIndex(int2(coordInChunk.x,0),NumCellInChunkX);
                SortedArray[mad(chunkIndex+1,totalCellInChunk,cellIndexInNextChunk)] = UnsortedArray[index];
            }
            
            if (coordInChunk.y == NumCellInChunkX - 1 && cellCoord.y != NumCellX - 1)
            {
                cellIndexInNextChunk = GetIndex(int2(0,coordInChunk.y),NumCellInChunkX);
                SortedArray[mad(chunkIndex+NumChunkX,totalCellInChunk,cellIndexInNextChunk)] = UnsortedArray[index];
            }

            if (all(coordInChunk == int2(NumCellInChunkX - 1)) && !all(cellCoord == int2(NumCellX - 1)))
            {
                cellIndexInNextChunk = 0;
                SortedArray[mad(chunkIndex+NumChunkX+1,totalCellInChunk,cellIndexInNextChunk)] = UnsortedArray[index];
            }
            
        }
    }
}
