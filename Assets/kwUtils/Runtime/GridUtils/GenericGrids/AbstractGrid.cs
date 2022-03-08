using Unity.Mathematics;

namespace KWUtils
{
    public interface IGridHandlerTest<T>
    {
        public T Grid { get; }
    }
    
    public class GridSystemTest : IGridHandlerTest<GenericGrid<bool>>
    {
        public GenericGrid<bool> Grid { get; }

        public void test()
        {
            
        }
    }
    
    public class GridSystemTest2 : IGridHandlerTest<GenericChunkedGrid<bool>>
    {
        public GenericChunkedGrid<bool> Grid { get; }

        public void test()
        {
            
        }
    }
    
    public abstract class AbstractGrid<T>
    {
        protected int CellSize;
        protected int GridWidth;
        protected int GridHeight;
        
        protected int2 MapWidthHeight;
        protected int2 GridBounds;
        
        public T[] GridArray;
    }
}