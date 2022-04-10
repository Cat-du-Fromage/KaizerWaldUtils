using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils.ProceduralMeshes
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralMesh : MonoBehaviour
    {
        [SerializeField, Range(1, 10)]
        private int resolution = 1;
        
        private Mesh mesh;

        private void Awake() 
        {
            mesh = new Mesh 
            {
                name = "Procedural Mesh"
            };
            //GenerateMesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }
        
        private void OnValidate()
        {
            enabled = true;
        }
        
        private void Update() 
        {
            GenerateMesh();
            enabled = false;
        }

        private void GenerateMesh()
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            MeshJob<SquareGrid, MultiStream>.ScheduleParallel(mesh, meshData, resolution, default).Complete();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

    }
}
