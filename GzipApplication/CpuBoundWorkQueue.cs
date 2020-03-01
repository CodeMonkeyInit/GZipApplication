using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GzipApplication
{
    public static class CpuBoundWorkQueue
    {
        private static readonly Thread[] Threads;

        public static readonly int ParallelWorkMax = 1;

        static CpuBoundWorkQueue()
        {
            Threads = new Thread[ParallelWorkMax];

            for (int i = 0; i < Threads.Length; i++)
            {
                var newThread = new Thread(DoSomeWork)
                {
                    IsBackground = true
                };

                newThread.Start();
            }
        }

        private static readonly ConcurrentQueue<Action> ConcurrentQueue = new ConcurrentQueue<Action>();

        private static readonly SemaphoreSlim WorkQueuedSemaphore = new SemaphoreSlim(0);

        public static bool QueueWork(Action someWork)
        {
            ConcurrentQueue.Enqueue(someWork);

            WorkQueuedSemaphore.Release();

            return true;
        }

        private static void DoSomeWork()
        {
            while (true)
            {
                WorkQueuedSemaphore.Wait();

                if (ConcurrentQueue.TryDequeue(out Action workToDo))
                {
                    workToDo();
                }
            }
        }
    }
}