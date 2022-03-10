using System.Collections.Generic;
using Unity.Mathematics;

namespace KWUtils.KWGenericGrid
{
    public class GridSystemTest : IGridHandler<bool, GenericGrid<bool>>
    {
        public GenericGrid<bool> Grid { get; }

        public void test()
        {
            
        }


        public void InitializeGrid(int2 terrainBounds)
        {
            
        }
    }
    
    public class GridSystemTest2 : IGridHandler<bool, GenericChunkedGrid<bool>>
    {
        public GenericChunkedGrid<bool> Grid { get; }

        public void test()
        {
            
        }

        public void InitializeGrid(int2 terrainBounds)
        {
            
        }
    }
}