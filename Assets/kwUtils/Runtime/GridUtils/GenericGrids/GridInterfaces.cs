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

    public interface IGridBehaviour
    {
        public IGridSystem GridSystem { get; set; }
        public void SetGridSystem(IGridSystem system) => GridSystem = system;
        public void InitializeGrid(int2 terrainBounds);
    }
    

    public interface IGridSystem
    {
        public TerrainData MapData { get; set; }

        public int2 MapBounds { get; set; }
        
        public void SubscribeToGrid<T>(T gridType, Action action) where T : Enum;

        public T1[] RequestGrid<T1, T2>(T2 gridType) where T1 : struct where T2 : Enum;
        
        public void InitializeTerrain()
        {
            MapData = Object.FindObjectOfType<Terrain>().terrainData;
            MapBounds = (int2)MapData.size.XZ();
        }
        
        public void InitializeAllGrids()
        {
            IGridBehaviour[] gridBehaviours = GameObjectExtension.FindObjectsOfInterface<IGridBehaviour>().ToArray();
            for (int i = 0; i < gridBehaviours.Length; i++)
            {
                gridBehaviours[i].SetGridSystem(this);
                gridBehaviours[i].InitializeGrid(MapBounds);
            }
        }
/*
        public Dictionary<GridType, MonoBehaviour> test
        {
            get;
            set;
        }

        public void GetGridsByType<T1,T2,T3>()
        where T1 : struct
        where T2 : GenericGrid<T1>
        where T3 : Enum
        {
            //AbstractGridHandler<T1, T2, T3>[] grids = GameObject.FindObjectsOfType<AbstractGridHandler<T1, T2, T3>>();
            AbstractGridHandler<T1,T2,T3>[] gridBehaviours = GameObject.FindObjectsOfType<AbstractGridHandler<T1,T2,T3>>();
            test = new Dictionary<T3, MonoBehaviour>(gridBehaviours.Length);
            for (int i = 0; i < gridBehaviours.Length; i++)
            {
                test.Add(gridBehaviours[i].GridType, gridBehaviours[i]);
                //gridBehaviours[i].SetGridSystem(this);
                //gridBehaviours[i].InitializeGrid(MapBounds);
            }
        }
*/
    }
    
    public interface IGridHandler<T1, out T2> : IGridBehaviour
    where T1 : struct
    where T2 : GenericGrid<T1>
    {
        public T2 Grid { get; }
    }
}
