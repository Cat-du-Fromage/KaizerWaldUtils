#define EnableBurst

using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using int2 = Unity.Mathematics.int2;

namespace KWUtils.KWGenericGrid
{
        
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
#if EnableBurst
    [BurstCompile]
#endif
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
        
        public void Execute(int index)
        {
            int2 cellCoord = index.GetXY2(NumCellX);
            
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);
            int2 coordInChunk = cellCoord - (chunkCoord * NumCellInChunkX);

            int indexCellInChunk = coordInChunk.GetIndex(NumCellInChunkX);
            int chunkIndex = chunkCoord.GetIndex(NumChunkX);
            int totalCellInChunk = NumCellInChunkX * NumCellInChunkX;
            
            SortedArray[mad(chunkIndex,totalCellInChunk,indexCellInChunk)] = UnsortedArray[index];
        }
    }
    
    //CAREFUL Burst does not support generic call (result in crash)
    //Only enable if the static function calling it is not a generic type!
    public struct JGenericOrderArrayByChunkIndex<T> : IJobFor
    where T : struct
    {
        [ReadOnly] private readonly int CellSize;
        [ReadOnly] private readonly int ChunkSize;
        [ReadOnly] private readonly int NumCellInChunkX;
        [ReadOnly] private readonly int NumCellX;
        [ReadOnly] private readonly int NumChunkX;
        
        [ReadOnly, NativeDisableParallelForRestriction]  private NativeArray<T> UnsortedArray;
        [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<T> SortedArray;
        
        public JGenericOrderArrayByChunkIndex (in GridData gridData, NativeArray<T> unsortedArray, NativeArray<T> sortedArray)
        {
            CellSize = gridData.CellSize;
            ChunkSize = gridData.ChunkSize;
            NumCellInChunkX = gridData.NumCellInChunkX;
            NumCellX = gridData.NumCellXY.x;
            NumChunkX = gridData.NumChunkXY.x;
            UnsortedArray = unsortedArray;
            SortedArray = sortedArray;
        }
        
        public void Execute(int index)
        {
            int2 cellCoord = index.GetXY2(NumCellX);
            
            float ratio = CellSize / (float)ChunkSize; //CAREFULL! NOT ChunkCellWidth but Cell compare to Chunk!
            int2 chunkCoord = (int2)floor((float2)cellCoord * ratio);
            int2 coordInChunk = cellCoord - (chunkCoord * NumCellInChunkX);

            int indexCellInChunk = coordInChunk.GetIndex(NumCellInChunkX);
            int chunkIndex = chunkCoord.GetIndex(NumChunkX);
            int totalCellInChunk = NumCellInChunkX * NumCellInChunkX;
            
            SortedArray[mad(chunkIndex,totalCellInChunk,indexCellInChunk)] = UnsortedArray[index];
        }
    }

    public struct JConvertGrid<T> : IJobFor
    where T : struct
    {
        [ReadOnly] private readonly GridData OldGridData;
        [ReadOnly] private readonly GridData NewGridData;

        [ReadOnly, NativeDisableParallelForRestriction]  private NativeArray<T> GridToConvert; //Smaller length
        [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<T> GridConverted; //Length * (OldCell/NewCell)^2
        
        public JConvertGrid(in GridData oldGridData, in GridData newGridData, NativeArray<T> gridToConvert, NativeArray<T> gridConverted)
        {
            OldGridData = oldGridData;
            NewGridData = newGridData;
            GridToConvert = gridToConvert;
            GridConverted = gridConverted;
        }
        
        public void Execute(int index) // index = chunk index(GridToConvert)
        {
            int numIteration = Sq(NewGridData.CellSize / OldGridData.CellSize);
            for (int i = 0; i < numIteration; i++)
            {
                int2 coordInChunk = i.GetXY2(NewGridData.NumCellInChunkX);
                int indexInChunk = coordInChunk.GetIndex(NewGridData.NumCellInChunkX);
                int gridIndex = index.GetGridCellIndexFromChunkCellIndex(OldGridData, indexInChunk);

                GridConverted[gridIndex] = GridToConvert[index];
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
}
