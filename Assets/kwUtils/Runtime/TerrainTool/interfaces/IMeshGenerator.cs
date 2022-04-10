using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//MAY WANT TO MOVE TO "PROCEDURAL MESH" LIBRARY
namespace KWUtils.ProceduralMeshes
{
    public interface IMeshGenerator
    {
        public int Resolution { get; set; }
        public int VertexCount { get; }
        public int IndexCount { get; }
        public int JobLength { get; }
        public Bounds Bounds { get; }
        
        public void Execute<S> (int i, S streams) where S : struct, IMeshStreams;
    }
}
