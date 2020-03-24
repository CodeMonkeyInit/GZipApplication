using System;
using System.Threading;
using GzipApplication.Constants;
using GzipApplication.Data;

namespace GzipApplication.WorkQueue
{
    /// <summary>
    ///     Queue for IO bound operations like reading/writing from disk.
    /// </summary>
    public class IOBoundQueue
    {
        private readonly IThreadSafeQueue<Function> _actions =
            new ThreadSafeQueue<Function>(ApplicationConstants.BufferSlots * 1000);

        private readonly AutoResetEvent _newActionEnqueued = new AutoResetEvent(false);

        public void Enqueue(Function function)
        {
            _actions.Enqueue(function);
            _newActionEnqueued.Set();
        }

        /// <summary>
        ///     Evaluates members of queue until eventWaitHandle is set.
        /// 
        ///     <remarks>
        ///         All members will be executed on calling thread.
        ///     </remarks> 
        /// </summary>
        /// <param name="eventWaitHandle">Handle that signals that evaluation should end.</param>
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