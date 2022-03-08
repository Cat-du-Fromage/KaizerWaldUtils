using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static KWUtils.KwManagedContainerUtils;

namespace KWUtils
{
    public static class NativeCollectionExt
    {
        public static NativeArray<T> AllocNtvAry<T>(int size, Allocator a = Allocator.TempJob) 
        where T : struct
        {
            return new NativeArray<T>(size, a, NativeArrayOptions.UninitializedMemory);
        }
        
        public static NativeArray<T> AllocNtvAryOpt<T>(int size, NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
        where T : struct
        {
            return new NativeArray<T>(size, Allocator.TempJob, nao);
        }

        public static NativeArray<T> AllocFillNtvAry<T>(int size, T val) 
        where T : struct
        {
            NativeArray<T> a = new NativeArray<T>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < size; i++) { a[i] = val; }
            return a;
        }
        
        public static void Fill<T>(this NativeArray<T> array, T val)
        where T : struct
        {
            for (int i = 0; i < array.Length; i++) { array[i] = val; }
        }
        
        public static int NumValueNotEqualTo<T>(this NativeArray<T> array, T val) 
        where T : struct
        {
            int n = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(val)) continue;
                n++;
            }
            return n;
        }
        
        public static NativeArray<T> RemoveDuplicates<T>(this NativeArray<T> s, Allocator a = Allocator.TempJob, NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
        where T : unmanaged
        {
            HashSet<T> set = new HashSet<T>(s.ToArray());
            NativeArray<T> result = new NativeArray<T>(set.Count, a, nao);
            result.CopyFrom(set.ToArray());
            return result;
        }

        /// <summary>
        /// Conditional Add used in parallel Job system
        /// </summary>
        /// <param name="list"></param>
        /// <param name="flag"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddNoResizeIf<T>(this NativeList<T>.ParallelWriter list, bool flag, T obj)
        where T : unmanaged
        {
            if (flag) { list.AddNoResize(obj); }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void NativeAddRange<T>(this List<T> list, NativeArray<T> dynamicBuffer)
            where T : struct
        {
            NativeAddRange(list, dynamicBuffer.GetUnsafePtr(), dynamicBuffer.Length);
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void NativeAddRange<T>(List<T> list, void* arrayBuffer, int length)
            where T : struct
        {
            int index = list.Count;
            int newLength = index + length;
     
            // Resize our list if we require
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }
     
            T[] items = NoAllocHelpers.ExtractArrayFromListT(list);
            int size = UnsafeUtility.SizeOf<T>();
     
            // Get the pointer to the end of the list
            IntPtr bufferStart = (IntPtr) UnsafeUtility.AddressOf(ref items[0]);
            byte* buffer = (byte*)(bufferStart + (size * index));
     
            UnsafeUtility.MemCpy(buffer, arrayBuffer, length * (long) size);
     
            NoAllocHelpers.ResizeList(list, newLength);
        }


    }
}
