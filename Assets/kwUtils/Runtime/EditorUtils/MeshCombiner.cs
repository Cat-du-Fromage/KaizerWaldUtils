using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace KWUtils
{
    
    public class MeshCombiner : MonoBehaviour
    {
        [SerializeField] private string MeshName = string.Empty;
        public bool optimizeMesh = false;
        [SerializeField] private List<MeshFilter> sourceMeshFilters;
        [SerializeField] private MeshFilter targetMeshFilter;

        [SerializeField] private Mesh newMesh = null;
    
        [ContextMenu("Combine Meshes")]
        private void CombineMeshes()
        {
            if (MeshName != String.Empty)
            {
                CombineInstance[] combine = new CombineInstance[sourceMeshFilters.Count];
    
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    combine[i].mesh = sourceMeshFilters[i].sharedMesh;
                    combine[i].transform = sourceMeshFilters[i].transform.localToWorldMatrix;
                }
    
                Mesh mesh = new Mesh();
                mesh.name = MeshName;
                mesh.CombineMeshes(combine);
                targetMeshFilter.mesh = mesh;
                newMesh = mesh;
            }
        }

        [ContextMenu("Save Mesh")]
        private void SaveMesh()
        {
            if (newMesh != null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/CustomMesh"))
                {
                    AssetDatabase.CreateFolder("Assets", "CustomMesh");
                }
                
                if (optimizeMesh)
                    MeshUtility.Optimize(newMesh);
                
                AssetDatabase.CreateAsset(newMesh, $"Assets/CustomMesh/{newMesh.name}.asset" );
                AssetDatabase.SaveAssets();
                newMesh = null;
            }
        }
    }
}
#endif
