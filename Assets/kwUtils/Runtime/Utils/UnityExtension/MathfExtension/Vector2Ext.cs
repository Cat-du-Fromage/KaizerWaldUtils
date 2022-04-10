using UnityEngine;

namespace KWUtils
{
    public static class Vector2Ext
    {
        public static bool IsLeft(this Vector2 centerPoint, Vector2 otherPoint) => centerPoint.x < otherPoint.x;
        public static bool IsAbove(this Vector2 centerPoint, Vector2 otherPoint) => centerPoint.y > otherPoint.y;

        public static Vector3 ToVector3Flat(this Vector2 origin) => new Vector3(origin.x, 0, origin.y);
    }
}