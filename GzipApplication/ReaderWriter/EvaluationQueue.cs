using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GzipApplication.ReaderWriter
{
    public class EvaluationQueue
    {
        private readonly ConcurrentQueue<Function> _actions = new ConcurrentQueue<Function>();

        private readonly HashSet<string> incompletedFunctions = new HashSet<string>();

        public void Enqueue(Function function)
        {
            _actions.Enqueue(function);
        }

        public void Evaluate() => Evaluate(CancellationToken.None);

        void Evaluate(CancellationToken token)
        {
            do
            {
                if (token.IsCancellationRequested)
                    return;
                

                while (_actions.TryDequeue(out Function function))
                {
                    var isCompleted = function.Payload();

                    if (isCompleted)
                        incompletedFunctions.Remove(function.Name);

                    else
                        incompletedFunctions.Add(function.Name);
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            } while (incompletedFunctions.Count > 0);
        }
    }
}