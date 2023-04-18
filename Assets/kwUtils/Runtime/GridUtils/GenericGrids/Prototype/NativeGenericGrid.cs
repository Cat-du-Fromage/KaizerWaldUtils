using System;
using Unity.Collections;
using Unity.Mathematics;

using static System.Array;
using static Unity.Mathematics.math;
using static KWUtils.KWmath;
using static KWUtils.KWGrid;
//using static KWUtils.KWChunk;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2; 

namespace KWUtils
{
    public class NativeGenericGrid<T> : IDisposable
    where T : struct
    {
        protected readonly bool IsCentered;
        protected readonly int2 NumCellXY;
        
        protected NativeArray<T> GridArray;
        public event Action OnGridChange;
        
        protected float2 Offset => IsCentered ? (float2)NumCellXY / 2f : float2.zero;

        //==============================================================================================================
        //CONSTRUCTOR
        //==============================================================================================================
        public NativeGenericGrid(in int2 numCellXY, bool isCentered = false)
        {
            IsCentered = isCentered;
            NumCellXY = ceilpow2(numCellXY);
            GridArray = new NativeArray<T>(NumCellXY.x * NumCellXY.y, Persistent);
        }
        
        public NativeGenericGrid(in int2 numCellXY, T[] source, bool isCentered = false) : this(numCellXY, isCentered)
        {
            GridArray.CopyFrom(source);
        }
        
        public NativeGenericGrid(in int2 numCellXY, NativeArray<T> source, bool isCentered = false) : this(numCellXY, isCentered)
        {
            GridArray.CopyFrom(source);
        }
        
        public NativeGenericGrid(in int2 numCellXY, Func<T[]> providerFunction, bool isCentered = false) : this(numCellXY, isCentered)
        {
            providerFunction.Invoke().CopyTo(GridArray);
        }
        
        public NativeGenericGrid(in int2 numCellXY, Func<NativeArray<T>> providerFunction, bool isCentered = false) : this(numCellXY, isCentered)
        {
            providerFunction.Invoke().CopyTo(GridArray);
        }
        
        //==============================================================================================================
        //CELLS INFORMATION
        //==============================================================================================================

        public virtual float3 GetCellCenter(int index)
        {
            float2 cellCoord = GetXY2(index,NumCellXY.x) + float2(0.5f) - Offset;
            return float3(cellCoord.x,0,cellCoord.y);
        }
        
        //==============================================================================================================
        // ARRAY MANIPULATION
        //==============================================================================================================
        public virtual void CopyFrom(T[] otherArray) => GridArray.CopyFrom(otherArray);
        public virtual void CopyFrom(NativeArray<T> otherArray) => GridArray.CopyFrom(otherArray);

        //==============================================================================================================
        //ACCESSOR
        //==============================================================================================================
        public virtual T ElementAt(int index) => GridArray[index];
        public virtual T this[int cellIndex]
        {
            get => GridArray[cellIndex];
            set => SetValue(cellIndex, value);
        }
        public virtual void SetValue(int index, T value)
        {
            GridArray[index] = value;
            OnGridChange?.Invoke();
        }
        
        //==============================================================================================================
        // Operation from World Position
        //==============================================================================================================
        public virtual int IndexFromPosition(in float3 position)
        {
            float2 pos2D = position.xz;
            return IsCentered ? GetIndexFromPositionOffset(pos2D, NumCellXY) : GetIndexFromPosition(pos2D, NumCellXY);
        }

        //==============================================================================================================
        // DESTRUCTOR
        //==============================================================================================================
        //Clear Events
        public virtual void ClearEvents()
        {
            if (OnGridChange == null) return;
            ForEach(OnGridChange.GetInvocationList(),action => OnGridChange -= (Action)action);
            //foreach (Delegate action in OnGridChange.GetInvocationList()) { OnGridChange -= (Action)action; }
        }
        
        public void Dispose()
        {
            ClearEvents();
            if(GridArray.IsCreated) GridArray.Dispose();
        }
        
        ~NativeGenericGrid() => Dispose();
    }
}
