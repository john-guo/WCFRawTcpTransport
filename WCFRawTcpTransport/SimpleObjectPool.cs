using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WCFRawTcpTransport
{
    class SimpleObjectPool<T> where T : class, new()
    {
        private ConcurrentBag<T> _pool;
        private bool _allowNew;

        public SimpleObjectPool(int poolSize, bool allowNew = false)
        {
            _allowNew = allowNew;
            _pool = new ConcurrentBag<T>();
            Allocate(poolSize);
        }

        private void Allocate(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                _pool.Add(new T());
            }
        }

        public T Take()
        {
            T item;
            if (!_pool.TryTake(out item))
            {
                if (_allowNew)
                    return new T();

                throw new OutOfMemoryException();
            }

            return item;
        }

        public void Return(T item)
        {
            _pool.Add(item);
        }
    }
}
