using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static KWUtils.KWmath;
using static KWUtils.KWGrid;

using static Unity.Mathematics.math;

namespace KWUtils
{
    public struct GridCell
    {
        public bool IsBlocked;
        public int Index;
        public int2 Coord;
        public float3 Center;
        public FixedList64Bytes<float3> Vertices;
        
        public GridCell(TerrainSettings settings, int x, int y, NativeArray<float3> cellVertex)
        {
            IsBlocked = false;
            Coord = new int2(x, y);
            Index = GetIndex(Coord, settings.NumQuadsAxis.x);
            Vertices = new FixedList64Bytes<float3>();
            unsafe { Vertices.AddRange(cellVertex.GetUnsafeReadOnlyPtr(),cellVertex.Length); }
            float2 coord2D = Coord - (float2)settings.NumQuadsAxis / 2f + float2(0.5f);
            float height = (cellVertex[1] + normalize(cellVertex[2] - cellVertex[1]) * (SQRT2/2f)).y;
            Center = new float3(coord2D.x, height, coord2D.y);
        }
        
        public GridCell(TerrainSettings settings, int2 coord, NativeArray<float3> cellVertex)
        {
            IsBlocked = false;
            Coord = coord;
            Index = GetIndex(Coord, settings.NumQuadsAxis.x);
            Vertices = new FixedList64Bytes<float3>();
            unsafe { Vertices.AddRange(cellVertex.GetUnsafeReadOnlyPtr(), cellVertex.Length); }
            float2 coord2D =coord - (float2)settings.NumQuadsAxis / 2f + float2(0.5f);
            float height = (cellVertex[1] + normalize(cellVertex[2] - cellVertex[1]) * (SQRT2/2f)).y;
            Center = new float3(coord2D.x, height, coord2D.y);
        }
        
        // Highest Vertex on Cell
        public float HighestPoint => ceil(cmax(float4(Vertices[0].y, Vertices[1].y, Vertices[2].y, Vertices[3].y)));

        // Triangles Normal
        public float3 NormalTriangleLeft => normalizesafe(cross(Vertices[2] - Vertices[0], Vertices[1] - Vertices[0]));
        public float3 NormalTriangleRight => normalizesafe(cross(Vertices[1] - Vertices[3], Vertices[2] - Vertices[3]));
        
        // Triangles
        public NativeSlice<float3> LeftTriangle => Vertices.ToNativeArray(Allocator.Temp).Slice(0, 3);
        public NativeSlice<float3> RightTriangle => Vertices.ToNativeArray(Allocator.Temp).Slice(1, 3);
        
        public float3 Get3DTranslatedPosition(float2 position2D)
        {
            bool isLeftTri = IsPointInTriangle(LeftTriangle, position2D);
            //Ray origin
            float3 rayOrigin = new float3(position2D.x, HighestPoint, position2D.y);
            //NORMAL
            float3 triangleNormal = isLeftTri ? NormalTriangleLeft : NormalTriangleRight;
            //Point A : start
            float3 a = isLeftTri ? LeftTriangle[0] : RightTriangle[0];
            float t = dot(a - rayOrigin, triangleNormal) / dot(down(), triangleNormal);
            return mad(t,down(), rayOrigin);
        }
        
        public bool IsPointInTriangle(NativeSlice<float3> triangle, float2 position2D)
        {
            float2 triA = triangle[0].xz;
            float2 triB = triangle[1].xz;
            float2 triC = triangle[2].xz;
            
            bool isAEqualC = Mathf.Approximately(triC.y, triA.y);
            float2 a = select(triA, triB, isAEqualC);
            float2 b = select(triB, triA, isAEqualC);
            
            float s1 = triC.y - a.y;
            float s2 = triC.x - a.x;

            float s3 = b.y - a.y;
            float s4 = position2D.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - position2D.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        public readonly override string ToString()
        {
            return $"Coord: {Coord}; Center: {Center}";
        }
    }
}
