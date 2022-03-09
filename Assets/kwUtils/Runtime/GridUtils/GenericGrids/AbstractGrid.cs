using Unity.Mathematics;

namespace KWUtils
{
    public interface IGridHandlerTest<T1, out T2>
    where T1 : struct
    where T2 : GenericGrid<T1>
    {
        public T2 Grid { get; }
    }
    
    public class GridSystemTest : IGridHandlerTest<bool, GenericGrid<bool>>
    {
        public GenericGrid<bool> Grid { get; }

        public void test()
        {
            
        }
    }
    
    public class GridSystemTest2 : IGridHandlerTest<bool, GenericChunkedGrid<bool>>
    {
        public GenericChunkedGrid<bool> Grid { get; }

        public void test()
        {
            
        }
    }
}