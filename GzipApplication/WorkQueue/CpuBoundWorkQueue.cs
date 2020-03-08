using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GzipApplication.WorkQueue
{
    public class CpuBoundWorkQueue
    {
        public static readonly int ParallelWorkMax = Environment.ProcessorCount;

        public static CpuBoundWorkQueue Instance => _instance.Value;
        
        
        private readonly ConcurrentQueue<Action> _concurrentQueue = new ConcurrentQueue<Action>();
        private readonly SemaphoreSlim _workQueuedSemaphore = new SemaphoreSlim(0);
        
        private readonly Thread[] _threads;
        
        private static readonly Lazy<CpuBoundWorkQueue> _instance  = new Lazy<CpuBoundWorkQueue>(
            () => new CpuBoundWorkQueue());

        
        public bool QueueWork(Action someWork)
        {
            _concurrentQueue.Enqueue(someWork);

            _workQueuedSemaphore.Release();

            return true;
        }

        private void Work()
        {
            while (true)
            {
                _workQueuedSemaphore.Wait();

                if (_concurrentQueue.TryDequeue(out Action? workToDo))
                {
                    workToDo();
                }
            }
        }
        
        private CpuBoundWorkQueue()
        {
            _threads = new Thread[ParallelWorkMax];

            for (int i = 0; i < _threads.Length; i++)
            {
                var newThread = new Thread(Work)
                {
                    IsBackground = true
                };

                newThread.Start();
            }
        }
    }
}