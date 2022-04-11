using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

using static Unity.Mathematics.math;
using static KWUtils.NativeCollectionExt;

namespace KWUtils.KwTerrain
{
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
    
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point, //just fill the triangles / trillinear/Billinear = blend with surrounding cube
                wrapMode = TextureWrapMode.Clamp // what happen when we go outside of the limits? (in this case stretch value continu as if it was part of the range)
            };
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(MapSettings mapSettings, TerrainType[] terrains, float[] heightMap)
        {
            //====================================
            //COLOR IS DEFINED HERE
            //====================================
            using NativeArray<Color> colorMap = AllocNtvAry<Color>(mapSettings.totalMapPoints);
            (NativeArray<float> terrainsHeights, NativeArray<Color> terrainsColor) = GetArraysTerrains(terrains);
            using NativeArray<float> noiseMap = heightMap.ToNativeArray();
            
            Texture2DJob textureJob = new Texture2DJob(terrainsHeights,terrainsColor, noiseMap, colorMap);
            JobHandle jobHandle = textureJob.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            
            //====================================
            terrainsHeights.Dispose();
            terrainsColor.Dispose();
            return TextureFromColourMap(colorMap.ToArray(), mapSettings.mapPointPerAxis, mapSettings.mapPointPerAxis);
            
        }

        private static (NativeArray<float>, NativeArray<Color>) GetArraysTerrains(TerrainType[] terrains)
        {
            NativeArray<float> terrainHeights = AllocNtvAry<float>(terrains.Length);
            NativeArray<Color> terrainColor = AllocNtvAry<Color>(terrains.Length);
            for (int i = 0; i < terrains.Length; i++)
            {
                terrainHeights[i] = terrains[i].height;
                terrainColor[i] = terrains[i].colour;
            }

            return (terrainHeights, terrainColor);
        }
        
        // FALLOFF
        //==============================================================================================================
        [BurstCompile(CompileSynchronously = true)]
        private struct Texture2DJob : IJobFor
        {
            [ReadOnly] private NativeArray<float> jTerrainsHeight;
            [ReadOnly] private NativeArray<Color> jTerrainsColor;
            [ReadOnly] private NativeArray<float> jNoiseMap;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<Color> jColorMap;

            public Texture2DJob(NativeArray<float> terrainsHeight, NativeArray<Color> terrainsColor, NativeArray<float> noiseMap, NativeArray<Color> colorMap)
            {
                jTerrainsHeight = terrainsHeight;
                jTerrainsColor = terrainsColor;
                jNoiseMap = noiseMap;
                jColorMap = colorMap;
            }
            
            public void Execute(int index)
            {
                for(int i = 0; i < jTerrainsHeight.Length; i++)
                {
                    if(jNoiseMap[index] <= jTerrainsHeight[i])
                    {
                        jColorMap[index] = jTerrainsColor[i];
                        break;
                    }
                }
            }
        }
    }
}