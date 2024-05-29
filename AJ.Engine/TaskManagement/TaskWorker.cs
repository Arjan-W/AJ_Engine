using AJ.Engine.Interfaces.TaskManagement;
using System.Collections.Concurrent;
using System.Threading;

namespace AJ.Engine.TaskManagement
{
    internal class TaskWorker
    {
        private const int DEQUEUE_ATTEMPTS = 100;

        private readonly int _id;
        private readonly TaskManager _taskManager;
        private readonly ConcurrentQueue<ITask> _taskQueue;
        private readonly AutoResetEvent _wakeEvent;
        private readonly Thread _thread;
        private bool _isRunning;

        internal TaskWorker(TaskManager taskManager, ConcurrentQueue<ITask> taskQueue, int id)
        {
            _taskManager = taskManager;
            _taskQueue = taskQueue;
            _id = id;
            _wakeEvent = new AutoResetEvent(false);
            _isRunning = true;
            using (AutoResetEvent are = new AutoResetEvent(false))
            {
                _thread = new Thread(() =>
                {
                    are.Set();
                    WorkerLoop();
                });
                _thread.Start();
                are.WaitOne();
            }
        }

        private void WorkerLoop()
        {
            int dequeueTries = 0;
            while (_isRunning)
            {
                if (_taskQueue.TryDequeue(out var task))
                {
                    task.OnStart();
                    task.OnFinished(task.OnRunTask());
                }
                else if (dequeueTries == DEQUEUE_ATTEMPTS)
                {
                    _taskManager.AddWorkerToInactivateWorkers(this);
                    _wakeEvent.WaitOne();
                    _wakeEvent.Reset();
                    dequeueTries = 0;
                }
                else
                {
                    dequeueTries++;
                }
            }
        }

        internal void Wake()
        {
            _wakeEvent.Set();
        }

        internal void Stop()
        {
            Volatile.Write(ref _isRunning, false);
            Wake();
            _thread.Join();
        }
    }
}