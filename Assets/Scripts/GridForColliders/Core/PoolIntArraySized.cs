using System.Collections.Generic;

namespace GridForColliders.Core
{
    // todo NOTE: NOT-THREAD-SAFE !
    // todo NOTE: GC optimization
    public class PoolIntArraySized
    {
        private readonly int _arraySize;
        private readonly List<int[]> _pool;

        public PoolIntArraySized(int arraySize, int capacity = 1024)
        {
            _arraySize = arraySize;
            _pool = new List<int[]>(capacity);
        }

        public int[] Get()
        {
            if (_pool.Count > 0)
            {
                int[] array = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
                return array;
            }
            return new int[_arraySize];
        }

        public void ReturnToPool(int[] array)
        {
            _pool.Add(array);
        }
    }
}