using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils.KwTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainGenerator : MonoBehaviour
    {
        //Data
        public int2 MapSize;
        private int numVertices;
        
        private Mesh mesh;
        
        private void Awake() 
        {
            MapSize = ceilpow2(MapSize);
            numVertices = MapSize.x * MapSize.y;
            
            mesh = new Mesh 
            {
                name = "Procedural Mesh",
                bounds = new Bounds(Vector3.zero, new Vector3(MapSize.x,0,MapSize.y)),
            };
            
            GetComponent<MeshFilter>().mesh = mesh;
        }
        
        //Setup(Num Vertices)
        private void GenerateMesh()
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            
            //Actual Job
            
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }
    }
}
