using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KWUtils.KWGenericGrid;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KWUtils.KWGenericGrid
{
    public abstract class GridSystemBehaviour<E> : MonoBehaviour
    where E : Enum
    {
        private TerrainData MapData { get; set; }
        private int2 MapBounds { get; set; }

        protected virtual void Awake() //DONT FORGET base.Awake();
        {
            InitializeTerrain();
            InitializeAllGrids();
        }

        public abstract void SubscribeToGrid(E gridType, Action action);

        public abstract T1[] RequestGrid<T1>(E gridType) where T1 : struct;
        
        private void InitializeTerrain()
        {
            MapData = Object.FindObjectOfType<Terrain>().terrainData;
            MapBounds = (int2)MapData.size.XZ();
        }
        
        private void InitializeAllGrids()
        {
            IGridBehaviour<GridType>[] gridBehaviours = GameObjectExtension.FindObjectsOfInterface<IGridBehaviour<GridType>>().ToArray();
            for (int i = 0; i < gridBehaviours.Length; i++)
            {
                //gridBehaviours[i].SetGridSystem(this);
                gridBehaviours[i].InitializeGrid(MapBounds);
            }
        }
    }
}
