using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

using static UnityEngine.Rendering.VertexAttribute;
using static UnityEngine.Rendering.VertexAttributeFormat;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;


using VertexAttribute = UnityEngine.Rendering.VertexAttribute;
using static UnityEngine.Mesh;
using Mesh = UnityEngine.Mesh;

namespace KWUtils
{
    public static class KWmesh
    {
        public static NativeArray<VertexAttributeDescriptor> InitializeVertexAttribute()
       {
           NativeArray<VertexAttributeDescriptor> vertexAttributes = new(4, Temp, UninitializedMemory);
           vertexAttributes[0] = new VertexAttributeDescriptor(Position, Float32, dimension: 3, stream: 0);
           vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, Float32, dimension: 3, stream: 1);
           vertexAttributes[2] = new VertexAttributeDescriptor(Tangent, Float16, dimension: 4, stream: 2);
           vertexAttributes[3] = new VertexAttributeDescriptor(TexCoord0, Float16, dimension: 2, stream: 3);
           return vertexAttributes;
       }
       
       //NEW MESH API
       public static MeshUpdateFlags NoRecalculations = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontRecalculateBounds;
       
       
       /// <summary>
       /// UPDATE ONLY VERTICES!! tangents and normals are not!
       /// </summary>
       /// <param name="mesh"></param>
       /// <param name="newVertices"></param>
       public static void UpdateVertices (ref Mesh mesh, Vector3[] newVertices)
       {
           int[] cubeVertices = new int[36] { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };
           int numVertices = mesh.vertices.Length;
           VertexAttributeDescriptor layout = new VertexAttributeDescriptor(VertexAttribute.Position, stream:0);
           mesh.SetVertexBufferParams(numVertices, layout);
           
           NativeArray<Vector3> verticesPos = new NativeArray<Vector3>(numVertices, Allocator.Temp);
           verticesPos.CopyFrom(newVertices);
           
           mesh.SetIndexBufferParams(mesh.vertices.Length, IndexFormat.UInt32);
           mesh.SetIndexBufferData(cubeVertices, 0, 0, numVertices, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
           
           mesh.SetVertexBufferData(verticesPos, 0, 0, numVertices, 0, MeshUpdateFlags.DontRecalculateBounds);
       }

    }
    
    
}