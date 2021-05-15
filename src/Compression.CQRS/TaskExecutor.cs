using System;
using System.Collections.Generic;
using System.Threading;

namespace Compression.CQRS
{
    public sealed class TaskExecutor : IDisposable
    {
        private readonly IList<Thread> _threadPool;

        private readonly Queue<Action> _actions = new Queue<Action>();

        private volatile bool _isDisposed;

        private volatile bool _isExecuting;
        private readonly int _maxThreadsCount = Environment.ProcessorCount;

        private readonly object _lock = new object();
        private int _totalTasks;

        private event EventHandler<Thread> TaskDone;

        public TaskExecutor()
        {
            _threadPool = new List<Thread>(_maxThreadsCount);
        }

        ~TaskExecutor()
        {
            if (!_isDisposed)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _isExecuting = false;

            _isDisposed = true;

            _actions.Clear();

            foreach (var _thread in _threadPool)
                _thread.Join();
        }

        public void AddTask(Action task)
        {
            _actions.Enqueue(task);
        }

        public void Start()
        {
            _isExecuting = true;

            while (true)
            {
                if (!_isExecuting || (_actions.Count == 0 && _threadPool.Count == 0)) // Stop execution or all tasks done.
                    break;

                if (_threadPool.Count == _maxThreadsCount) // The whole pool is busy with threads.
                    continue;

                Action task;

                if (!_actions.TryDequeue(out task)) // All tasks from the queue are pulled out.
                    continue;

                TaskDone = (sender, args) => { lock (_lock) _threadPool.Remove(args); };
                _totalTasks++;

                var thread = new Thread(() => { task(); TaskDone?.Invoke(this, Thread.CurrentThread); }) { Name = "GZipTask" + _totalTasks, Priority = ThreadPriority.AboveNormal };
                lock (_lock) _threadPool.Add(thread);
                thread.Start();
            }

            _isExecuting = false;
            _totalTasks = 0;
        }
    }
}
