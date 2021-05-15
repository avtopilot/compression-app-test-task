using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Compression.Utils.Task
{
    public sealed class MultiThreadTaskExecutor : IDisposable
    {
        private readonly IList<Thread> _threadPool;

        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private volatile bool _isExecuting;
        private readonly int _maxThreadsSize;

        private readonly object _lock = new object();
        private int _totalTasks;

        private event EventHandler<Thread> _taskFinished;

        public MultiThreadTaskExecutor(int maxTrheadsSize)
        {
            _maxThreadsSize = maxTrheadsSize;
            _threadPool = new List<Thread>(maxTrheadsSize);
        }

        ~MultiThreadTaskExecutor()
        {
            Dispose();
        }

        public void Dispose()
        {
            _isExecuting = false;

            foreach (var _thread in _threadPool)
            {
                _thread.Join();
            }

            _actionQueue.Clear();
        }

        public void AddTask(Action task)
        {
            _actionQueue.Enqueue(task);
        }

        public void Start()
        {
            _isExecuting = true;

            while (true)
            {
                // stopped execution
                // or all tasks are done
                if (!_isExecuting || _actionQueue.Count == 0 && _threadPool.Count == 0)
                    break;

                // thread pool is busy
                if (_threadPool.Count == _maxThreadsSize)
                    continue;

                Action task;

                // all tasks from the queue are pulled out.
                if (!_actionQueue.TryDequeue(out task))
                    continue;

                _taskFinished = (sender, args) => { lock (_lock) _threadPool.Remove(args); };
                _totalTasks++;

                var thread = new Thread(() => { task(); _taskFinished?.Invoke(this, Thread.CurrentThread); });
                lock (_lock) _threadPool.Add(thread);
                thread.Start();
            }

            _isExecuting = false;
            _totalTasks = 0;
        }
    }
}
