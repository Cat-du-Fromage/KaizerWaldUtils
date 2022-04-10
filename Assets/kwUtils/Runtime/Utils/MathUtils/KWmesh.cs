using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace KWUtils
{
    public static class KWmesh
    {
       public static readonly int[] CubeVertices = new int[] { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };

       public static readonly Vector3[] CubeBottom = new Vector3[4]
       {
            new Vector3(-1,0,1),
            new Vector3(1,0,1),
            new Vector3(1,0,-1),
            new Vector3(-1,0,-1),
       };
       
       public static readonly Vector3[] CubeTop = new Vector3[4]
       {
           new Vector3(-1,2,1),
           new Vector3(1,2,1),
           new Vector3(1,2,-1),
           new Vector3(-1,2,-1),
       };

       public static readonly Vector3[] BasicCube = new Vector3[8]
       {
           new Vector3(-1,0,1),
           new Vector3(1,0,1),
           new Vector3(1,0,-1),
           new Vector3(-1,0,-1),
           new Vector3(-1,2,1),
           new Vector3(1,2,1),
           new Vector3(1,2,-1),
           new Vector3(-1,2,-1),
       };

       
       //NEW MESH API

       public static MeshUpdateFlags NoRecalculations = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontRecalculateBounds;
       
       
       /// <summary>
       /// UPDATE ONLY VERTICES!! tangents and normals are not!
       /// </summary>
       /// <param name="mesh"></param>
       /// <param name="newVertices"></param>
       public static void UpdateVertices (ref Mesh mesh, Vector3[] newVertices)
       {
           int numVertices = mesh.vertices.Length;
           VertexAttributeDescriptor layout = new VertexAttributeDescriptor(VertexAttribute.Position, stream:0);
           mesh.SetVertexBufferParams(numVertices, layout);
           
           NativeArray<Vector3> verticesPos = new NativeArray<Vector3>(numVertices, Allocator.Temp);
           verticesPos.CopyFrom(newVertices);
           
           mesh.SetIndexBufferParams(mesh.vertices.Length, IndexFormat.UInt32);
           mesh.SetIndexBufferData(CubeVertices, 0, 0, numVertices, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
           
           mesh.SetVertexBufferData(verticesPos, 0, 0, numVertices, 0, MeshUpdateFlags.DontRecalculateBounds);
       }

    }
    
    
}