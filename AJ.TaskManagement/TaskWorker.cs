using AJ.Engine.Logging.Interfaces;
using AJ.TaskManagement.Interfaces;
using System.Collections.Concurrent;
using System.Threading;

namespace AJ.TaskManagement
{
    internal class TaskWorker
    {
        private const int TASK_DEQUEUE_ATTEMPTS = 10;

        private readonly ILogger _logger;
        private readonly int _id;
        private readonly ConcurrentQueue<ITask> _taskQueue;
        private readonly Thread _thread;
        private readonly AutoResetEvent _wakeEvent = new AutoResetEvent(false);
        private bool _isRunning;

        internal TaskWorker(ILogger logger, int id, ConcurrentQueue<ITask> taskQueue) {
            _logger = logger;
            _id = id;
            _taskQueue = taskQueue;
            _isRunning = true;
            using (var are = new AutoResetEvent(false)) {
                _thread = new Thread(() => {
                    are.Set();
                    WorkerLoop();
                });
                _thread.Start();
                are.WaitOne();
            }
        }

        private void WorkerLoop() {
            _logger.LogDebug($"ThreadWorker[{_id}]", "Started!");
            int dequeueAttemtps = 0;
            while (_isRunning) {
                if (_taskQueue.TryDequeue(out var task)) {
                    dequeueAttemtps = 0;
                    task.OnRun(_logger);
                }
                else {
                    dequeueAttemtps++;
                    if (dequeueAttemtps >= TASK_DEQUEUE_ATTEMPTS) {
                        _logger.LogDebug($"ThreadWorker[{_id}]", "Sleeping!");
                        _wakeEvent.WaitOne();
                        _logger.LogDebug($"ThreadWorker[{_id}]", "Waking!");
                        dequeueAttemtps = 0;
                    }
                }
            }
            _logger.LogDebug($"ThreadWorker[{_id}]", "Stopped!");
        }

        internal void Wake() {
            _wakeEvent.Set();
        }

        internal void Stop() {
            Volatile.Write(ref _isRunning, false);
            _wakeEvent.Set();
            _thread.Join();
            _wakeEvent.Dispose();
        }
    }
}