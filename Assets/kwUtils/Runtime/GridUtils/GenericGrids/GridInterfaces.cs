using System;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KWUtils
{
    public interface IGridBehaviour<E>
    where E : Enum
    {
        public IGridSystem<E> GridSystem { get; set; }
        public void SetGridSystem(IGridSystem<E> system) => GridSystem = system;
        public void InitializeGrid(int2 terrainBounds);
    }
    

    public interface IGridSystem<in E>
    where E : Enum
    {
        public TerrainData MapData { get; set; }

        public int2 MapBounds { get; set; }
        
        public void SubscribeToGrid(E gridType, Action action);

        //ALL REQUEST TYPE
        //public T RequestBoolGrid<T>(E gridType) where T : GenericGrid<bool>;
        
        
        public T2 RequestGrid<T1, T2>(E gridType) 
        where T1 : struct
        where T2 : GenericGrid<T1>;

        public T1[] RequestGridArray<T1>(E gridType) where T1 : struct;

        /// <summary>
        /// Gather terrain Datas
        /// </summary>
        public void InitializeTerrain()
        {
            MapData = Object.FindObjectOfType<Terrain>().terrainData;
            MapBounds = (int2)MapData.size.XZ();
        }
        
        /// <summary>
        /// Tie all GridHandlers to the GridSystem(According to the Enum constrained)
        /// Initialize all of them into
        /// </summary>
        public void InitializeAllGrids()
        {
            IGridBehaviour<E>[] gridBehaviours = GameObjectExtension.FindObjectsOfInterface<IGridBehaviour<E>>().ToArray();
            for (int i = 0; i < gridBehaviours.Length; i++)
            {
                gridBehaviours[i].SetGridSystem(this);
                gridBehaviours[i].InitializeGrid(MapBounds);
            }
        }

        public void Initialize()
        {
            InitializeTerrain();
            InitializeAllGrids();
        }
    }

    public interface IGridHandler<E, T1, out T2> : IGridBehaviour<E>
    where E : Enum
    where T1 : struct
    where T2 : GenericGrid<T1>
    {
        public T2 Grid { get; }
    }
}
