using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public sealed class GenericChunkedGrid<T> : AbstractGrid<T>
    where T : struct
    {
        private Dictionary<int, T[]> chunkDictionary;
        public GenericChunkedGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject)
        {
                
        }

        public T[] this[int index]
        {
            get => chunkDictionary[index];
            set
            {
                chunkDictionary[index] = value;
            }
        }
        

        public void UpdateChunk()
        {
            
        }
    }
}