using UnityEngine;
using Unity.Mathematics;

namespace KWUtils.ProceduralMeshes
{
    public interface IMeshStreams
    {
        public void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount);

        public void SetVertex(int index, in Vertex data);
        
        public void SetTriangle(int index, in int3 triangle);
    }
}
