using System.Threading;

namespace GzipApplication.Data
{
    public class ThreadSafeQueue<T> : IThreadSafeQueue<T>
    {
        private readonly int _capacity;
        private volatile int _count = 0;

        private volatile T[] _buffer;

        private volatile int _head = -1;
        private volatile int _tail = -1;

        public ThreadSafeQueue(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public bool Enqueue(T value)
        {
            int tail = Interlocked.Increment(ref _tail);
            int count = Interlocked.Increment(ref _count);

            if (count == _capacity)
                return false;

            _buffer[tail % _capacity] = value;

            return true;
        }

        public bool TryDequeue(out T value)
        {
            var localCount = Interlocked.Decrement(ref _count);

            //This means that multiple threads tried to decrement count so we should return it to its original state 
            if (localCount < 0)
            {
                Interlocked.Increment(ref _count);
                value = default;
                return false;
            }

            var head = Interlocked.Increment(ref _head);

            var index = head % _capacity;

            value = _buffer[index];

            _buffer[index] = default;

            return true;
        }
    }
}