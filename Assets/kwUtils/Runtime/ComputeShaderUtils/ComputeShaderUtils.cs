using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaizerWaldCode.Utils
{
    public static class ShaderUtils
    {
        /// <summary>
        /// concat "computeBuffer.SetData(Array[])" and "computeShader.SetBuffer(int, string, ComputeBuffer)" into one function
        /// </summary>
        /// <param name="computeShader"></param>
        /// <param name="kernel"></param>
        /// <param name="CSdata"></param>
        /// <param name="computeBuffer"></param>
        /// <param name="array"></param>
        public static void SetBuffer(ref ComputeBuffer computeBuffer, ComputeShader computeShader, string name, Array array, int kernel = 0)
        {
            computeBuffer.SetData(array);
            computeShader.SetBuffer(kernel, name, computeBuffer);
        }
        /// <summary>
        /// Release buffers passed in parameter
        /// </summary>
        /// <param name="buffers"></param>
        public static void Release(params ComputeBuffer[] buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                if (buffers[i] != null)
                {
                    buffers[i].Release();
                }
            }
        }

        #region Dispatch
        /// Convenience method for dispatching a compute shader.
        /// It calculates the number of thread groups based on the number of iterations needed.
        public static void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
        {
            int3 threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
            int numGroupsX = (int)math.ceil(numIterationsX / (float)threadGroupSizes.x);
            int numGroupsY = (int)math.ceil(numIterationsY / (float)threadGroupSizes.y);
            int numGroupsZ = (int)math.ceil(numIterationsZ / (float)threadGroupSizes.z);
            cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
        }
        public static int3 GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
        {
            compute.GetKernelThreadGroupSizes(kernelIndex, out uint x, out uint y, out uint z);
            return new int3((int)x, (int)y, (int)z);
        }
        #endregion Dispatch

        /// <summary>
        /// By Sebastian Lague
        /// </summary>
        #region BufferCreation
        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            bool createNewBuffer = buffer == null || !buffer.IsValid() || buffer.count != count || buffer.stride != stride;
            if (createNewBuffer)
            {
                Release(buffer);
                buffer = new ComputeBuffer(count, stride);
            }
        }

        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data)
        {
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
        }

        public static ComputeBuffer CreateAndSetBuffer<T>(T[] data, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            ComputeBuffer buffer = null;
            CreateAndSetBuffer<T>(ref buffer, data, cs, nameID, kernelIndex);
            return buffer;
        }

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, T[] data, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }

        public static ComputeBuffer CreateAndSetBuffer<T>(int length, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            ComputeBuffer buffer = null;
            CreateAndSetBuffer<T>(ref buffer, length, cs, nameID, kernelIndex);
            return buffer;
        }

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, int length, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            CreateStructuredBuffer<T>(ref buffer, length);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }
        #endregion BufferCreation

        #region GPU Request Async
        public static async Task<T[]> AsyncGpuRequest<T>(ComputeShader computeShader, int3 iteration, ComputeBuffer computeBuffer, int kernel = 0) 
            where T : struct
        {
            Dispatch(computeShader, iteration.x, iteration.y, iteration.z, kernel);
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(computeBuffer);
            while (!request.done && !request.hasError)
            {
                await Task.Yield();
            }
            NativeArray<T> result = request.GetData<T>();
            return result.ToArray();
        }

        public static async Task<NativeArray<T>> AsyncGpuRequestNative<T>(ComputeShader computeShader, int3 iteration, ComputeBuffer computeBuffer, int kernel = 0) 
            where T : struct
        {
            Dispatch(computeShader, iteration.x, iteration.y, iteration.z, kernel);
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(computeBuffer);
            while (!request.done && !request.hasError)
            {
                await Task.Yield();
            }
            //NativeArray<T> result = request.GetData<T>();
            return request.GetData<T>();
        }

        public static async Task<(NativeArray<T1>, NativeArray<T2>)> AsyncGpuRequestNative<T1, T2>(ComputeShader computeShader, int3 iteration, ComputeBuffer cB1, ComputeBuffer cB2, int kernel = 0) 
            where T1 : struct
            where T2 : struct
        {
            Dispatch(computeShader, iteration.x, iteration.y, iteration.z, kernel);
            AsyncGPUReadbackRequest request1 = AsyncGPUReadback.Request(cB1);
            AsyncGPUReadbackRequest request2 = AsyncGPUReadback.Request(cB2);
            while (!request1.done && !request1.hasError && !request2.done && !request2.hasError)
            {
                await Task.Yield();
            }
            return (request1.GetData<T1>(), request2.GetData<T2>());
        }

        public static async Task<(T1[], T2[])> AsyncGpuRequest<T1, T2>(ComputeShader computeShader, int3 iteration, ComputeBuffer cB1, ComputeBuffer cB2, int kernel = 0)
            where T1 : struct
            where T2 : struct
        {
            Dispatch(computeShader, iteration.x, iteration.y, iteration.z, kernel);
            AsyncGPUReadbackRequest request1 = AsyncGPUReadback.Request(cB1);
            AsyncGPUReadbackRequest request2 = AsyncGPUReadback.Request(cB2);
            while (!request1.done && !request1.hasError && !request2.done && !request2.hasError)
            {
                await Task.Yield();
            }
            return (request1.GetData<T1>().ToArray(), request2.GetData<T2>().ToArray());
        }
        #endregion GPU Request Async
    }
}
