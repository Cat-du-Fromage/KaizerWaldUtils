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
namespace KWUtils
{
    public static class KwManagedContainerUtils
    {
        //DICTIONNARY
        public static T[] GetKeysArray<T, U>(this Dictionary<T, U> dictionary)
        {
            T[] array = new T[dictionary.Keys.Count];
            dictionary.Keys.CopyTo(array,0);
            return array;
        }
        
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
        public static T[] Reverse<T>(this T[] array)
        where T : struct
        {
            Array.Reverse(array);
            return array;
        }
        
        public static T[] Concat<T>(this T[] x, T[] y)
        where T : struct
        {
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
        
        public static T[] GetFromMerge<T>(this T[] x, T[] y, T[] z)
        where T : struct
        {
            int oldLen = x.Length;
            Array.Copy(y, 0, x, 0, y.Length);
            Array.Copy(z, 0, x, y.Length, z.Length);
            return x;
        }
        
        /*
        public static NativeArray<T> ToNativeArray<T>(this T[] array, Allocator a = Allocator.TempJob , NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : struct
        {
            NativeArray<T> nA = new NativeArray<T>(array, a);
            nA.CopyFrom(array);
            return nA;
        }
        */
        public static NativeArray<T> ToNativeArray<T>(this T[] array, Allocator a = Allocator.TempJob) 
        where T : struct
        {
            return new NativeArray<T>(array, a);
        }
        
        public static unsafe NativeArray<T> ToNativeArray<T>(T* ptr, int length) where T : unmanaged
        {
            NativeArray<T> arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.Invalid);
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
            #endif
            return arr;
        }
        
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

        
        /// <summary>
        /// Convert HashSet To Array
        /// </summary>
        /// <param name="hashSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ToArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            return arr;
        }
        
        public static NativeArray<T> ToNativeArray<T>(this HashSet<T> hashSet)
        where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            NativeArray<T> ntvAry = new NativeArray<T>(hashSet.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ntvAry.CopyFrom(arr);
            return ntvAry;
        }
        
        public static T[] RemoveDuplicates<T>(this T[] s) 
        where T : struct
        {
            HashSet<T> set = new HashSet<T>(s);
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
        
        public static U[] ReinterpretArray<T,U>(this T[] array) 
        where T : struct //from
        where U : struct //to
        {
            using NativeArray<T> temp = new NativeArray<T>(array.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            temp.CopyFrom(array);
            return temp.Reinterpret<U>().ToArray();
        }
        
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
        
 
        public static unsafe NativeArray<T> ToNativeArray<T>(Span<T> span) 
        where T : unmanaged
        {
            // assumes the GC is non-moving
            fixed (T* ptr = span)
            {
                return ToNativeArray(ptr, span.Length);
            }
        }
 
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