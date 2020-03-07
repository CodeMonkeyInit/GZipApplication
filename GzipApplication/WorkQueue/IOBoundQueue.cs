using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GzipApplication.ReaderWriter;

namespace GzipApplication.WorkQueue
{
    public class IOBoundQueue
    {
        private readonly ConcurrentQueue<Function> _actions = new ConcurrentQueue<Function>();

        private readonly HashSet<string> _incompletedFunctions = new HashSet<string>();
        
        private readonly AutoResetEvent _newActionEnqueued = new AutoResetEvent(false);

        public void Enqueue(Function function)
        {
            _actions.Enqueue(function);
            _newActionEnqueued.Set();
        }

        public void Evaluate() => Evaluate(CancellationToken.None);

        public void Evaluate(CancellationToken token)
        {
            do
            {
                _newActionEnqueued.WaitOne();

                if (token.IsCancellationRequested)
                    return;
                
                while (_actions.TryDequeue(out Function function))
                {
                    var isCompleted = function.Payload();

                    if (isCompleted)
                        _incompletedFunctions.Remove(function.Name);

                    else
                        _incompletedFunctions.Add(function.Name);
                }
            } while (_incompletedFunctions.Count > 0);
        }
    }
}