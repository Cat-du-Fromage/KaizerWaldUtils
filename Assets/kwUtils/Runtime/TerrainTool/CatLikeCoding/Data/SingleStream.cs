using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace KWUtils.ProceduralMeshes
{
    public struct SingleStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Stream0 
        {
            public float3 position;
            public float3 normal;
            public float4 tangent;
            public float2 texCoord0;

            public Stream0(in float3 position, in float3 normal, in float4 tangent, in float2 texCoord0)
            {
                this.position = position;
                this.normal = normal;
                this.tangent = tangent;
                this.texCoord0 = texCoord0;
            }
        }
        
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Stream0> stream0;
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<TriangleUInt16> triangles;
        
        public void Setup (Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) 
        {
            NativeArray<VertexAttributeDescriptor> descriptor = new (4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            descriptor[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
            descriptor[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4);
            descriptor[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);
            meshData.SetVertexBufferParams(vertexCount, descriptor);
            descriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh
            (
                0, new SubMeshDescriptor(0, indexCount) 
                {
                    bounds = bounds,
                    vertexCount = vertexCount
                },
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
            );

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex (int index, in Vertex vertex) => stream0[index] = new Stream0 
        {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0
        };

        public void SetTriangle (int index, in int3 triangle)
        {
            triangles[index] = triangle;
        }
    }
}
