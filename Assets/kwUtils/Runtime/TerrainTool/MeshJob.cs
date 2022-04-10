using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace KWUtils.ProceduralMeshes
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<G, S> : IJobFor
    where G : struct, IMeshGenerator
    where S : struct, IMeshStreams
    {
        public G Generator;
        [WriteOnly] public S Streams;
        public void Execute(int index)
        {
            Generator.Execute(index, Streams);
        }
        
        public static JobHandle ScheduleParallel(Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency) 
        {
            MeshJob<G, S> job = new MeshJob<G, S>();
            job.Generator.Resolution = resolution;
            job.Streams.Setup
            (
                meshData,
                mesh.bounds = job.Generator.Bounds,
                job.Generator.VertexCount,
                job.Generator.IndexCount
            );
            return job.ScheduleParallel(job.Generator.JobLength, 1, dependency);
        }
    }
}
