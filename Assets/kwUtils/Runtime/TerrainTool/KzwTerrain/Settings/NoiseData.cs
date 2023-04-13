using System;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KWUtils
{
    [Serializable]
    public struct NoiseData
    {
        public uint   Seed;
        public int    Octaves;
        public float  Lacunarity;
        public float  Persistence;
        public float  Scale;
        public float  HeightMultiplier;
        public float2 Offset;
        
        
        public NoiseData(in NoiseData noise)
        {
            Seed             = max(1, noise.Seed);
            Octaves          = max(1, noise.Octaves);
            Lacunarity       = max(1f, noise.Lacunarity);
            Persistence      = noise.Persistence;
            Scale            = max(0.0001f, noise.Scale);
            HeightMultiplier = max(1f, noise.HeightMultiplier);
            Offset           = noise.Offset;
        }
    }
}
