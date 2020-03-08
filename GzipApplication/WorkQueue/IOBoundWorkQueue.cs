using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GzipApplication.WorkQueue
{
    public class IOBoundQueue
    {
        private readonly ConcurrentQueue<Function> _actions = new ConcurrentQueue<Function>();

        private readonly AutoResetEvent _newActionEnqueued = new AutoResetEvent(false);

        public void Enqueue(Function function)
        {
            _actions.Enqueue(function);
            _newActionEnqueued.Set();
        }

        public void Evaluate(EventWaitHandle eventWaitHandle)
        {
            do
            {
                _newActionEnqueued.WaitOne();

                while (_actions.TryDequeue(out Function function))
                {
                    function.Payload();
                }
            } while (!eventWaitHandle.WaitOne(TimeSpan.Zero));
        }
    }
}