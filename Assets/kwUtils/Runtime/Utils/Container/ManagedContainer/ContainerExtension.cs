using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;


namespace KWUtils
{
    public static class KwManagedContainerUtils
    {
        //DICTIONNARY
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetKeysArray<T, U>(this Dictionary<T, U> dictionary)
        {
            T[] array = new T[dictionary.Keys.Count];
            dictionary.Keys.CopyTo(array,0);
            return array;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U[] GetValuesArray<T, U>(this Dictionary<T, U> dictionary)
        {
            U[] array = new U[dictionary.Values.Count];
            dictionary.Values.CopyTo(array,0);
            return array;
        }
        
        //==============================================================================================================
        //GENERIC ARRAY
        //==============================================================================================================
        
        // C# method converted to Extension
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Reverse<T>(this T[] array)
        where T : struct
        {
            Array.Reverse(array);
            return array;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Concat<T>(this T[] x, T[] y)
        where T : struct
        {
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetFromMerge<T>(this T[] x, T[] y, T[] z)
        where T : struct
        {
            int oldLen = x.Length;
            Array.Copy(y, 0, x, 0, y.Length);
            Array.Copy(z, 0, x, y.Length, z.Length);
            return x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(this T[] array, Allocator allocator = TempJob) 
        where T : struct
        {
            return new NativeArray<T>(array, allocator);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(this ArraySegment<T> array, Allocator allocator = TempJob) 
        where T : struct
        {
            return array.ToArray().ToNativeArray(allocator);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> ToNativeArray<T>(T* ptr, int length) where T : unmanaged
        {
            NativeArray<T> arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.Invalid);
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
            #endif
            return arr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> CopyAllData<T>(this T[] array, Allocator allocator = Allocator.TempJob) 
        where T : unmanaged
        {
            NativeArray<T> dst = new NativeArray<T>(array.Length, allocator, NativeArrayOptions.UninitializedMemory);
            fixed (T* srcPtr = array)
            {
                void* dstPtr = dst.GetUnsafePtr();
                UnsafeUtility.MemCpy(dstPtr,srcPtr, sizeof(T) * array.Length);
            }
            return dst;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> CopyData<T>(this T[] array, int count, int offset = 0, Allocator allocator = Allocator.TempJob) 
        where T : unmanaged
        {
            NativeArray<T> dst = new NativeArray<T>(count, allocator);
            fixed (T* srcPtr = array)
            {
                void* dstPtr = dst.GetUnsafePtr();
                UnsafeUtility.MemCpy(dstPtr,srcPtr + offset, sizeof(T) * count);
            }
            return dst;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this T[] array, T[] items)
        {
            int size = array.Length;
            Array.Resize(ref array, array.Length + items.Length);
            for (int i = 0; i < items.Length; i++)
                array[size + i] = items[i];
        }
        
        /// <summary>
        /// Convert HashSet To Array
        /// </summary>
        /// <param name="hashSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            return arr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> ToNativeArray<T>(this HashSet<T> hashSet)
        where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            NativeArray<T> ntvAry = new (hashSet.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ntvAry.CopyFrom(arr);
            return ntvAry;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] RemoveDuplicates<T>(this T[] s) 
        where T : struct
        {
            HashSet<T> set = new HashSet<T>(s);
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U[] ReinterpretArray<T,U>(this T[] array) 
        where T : struct //from
        where U : struct //to
        {
            using NativeArray<T> temp = new NativeArray<T>(array.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            temp.CopyFrom(array);
            return temp.Reinterpret<U>().ToArray();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ArraySegment<T> array)
        {
            return array == null || array.Count == 0;
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> ToNativeArray<T>(Span<T> span) 
        where T : unmanaged
        {
            // assumes the GC is non-moving
            fixed (T* ptr = span)
            {
                return ToNativeArray(ptr, span.Length);
            }
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeArray<T> ToNativeArray<T>(ReadOnlySpan<T> span) 
        where T : unmanaged
        {
            // assumes the GC is non-moving
            fixed (T* ptr = span)
            {
                return ToNativeArray(ptr, span.Length);
            }
        }
    }
}