using System.Collections;
using System.Collections.Generic;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils.KWGenericGrid
{
    public interface IGridBehaviour
    {
        public void InitializeGrid(int2 terrainBounds);
    }

    public interface IGridSystem
    {
        public TerrainData MapData { get; }

        public void InitializeAllGrids()
        {
            IGridBehaviour[] gridBehaviours = GameObjectExtension.FindObjectsOfInterface<IGridBehaviour>().ToArray();
            int2 mapSize = (int2)MapData.size.XZ();
            for (int i = 0; i < gridBehaviours.Length; i++)
            {
                gridBehaviours[i].InitializeGrid(mapSize);
            }
        }
    }
    
    public interface IGridHandler<T1, out T2> : IGridBehaviour
        where T1 : struct
        where T2 : GenericGrid<T1>
    {
        public T2 Grid { get; }
    }
}
