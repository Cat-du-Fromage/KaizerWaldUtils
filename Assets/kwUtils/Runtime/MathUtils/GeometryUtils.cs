using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class GeometryUtils
    {
        public static bool PointInTriangle(this Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s1 = c.y - a.y;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = p.y - a.y;

            float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x-a.x) * s1);
            float w2 = (s4- w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        public static bool PointInTriangle(this Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            float s1 = c.z - a.z;
            float s2 = c.x - a.x;
            float s3 = b.z - a.z;
            float s4 = p.z - a.z;

            float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x-a.x) * s1);
            float w2 = (s4- w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        public static bool PointInTriangle2(this Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 d = b - a;
            Vector3 e = c - a;
            
            e.y = Mathf.Approximately(e.y, 0) ? 0.0001f : e.y;//safety
            
            float w1 = (e.x * (a.y - p.y) + e.y * (p.x - a.x)) / (d.x * e.y - d.y * e.x);
            float w2 = (p.y - a.y - w1 * d.y) / e.y;
            return (w1 >= 0f) && (w2 >= 0f) && ((w1 + w2) <= 1f);
        } 
    }
}
