using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Logging.Interfaces;
using System.Collections.Concurrent;

namespace AJ.Engine.TaskManagement
{
    internal class TaskManager : ITaskManager, IModule
    {
        private ILogger _logger;
        private readonly ConcurrentQueue<ITask> _taskQueue;
        private readonly ConcurrentQueue<TaskWorker> _taskWorkerQueue;

        private readonly TaskWorker[] _taskWorkers;

        internal TaskManager(int numOfTaskWorkers) {
            _logger = Core.ModuleProvider.Get<ILogger>();
            _taskQueue = new ConcurrentQueue<ITask>();
            _taskWorkerQueue = new ConcurrentQueue<TaskWorker>();

            _taskWorkers = new TaskWorker[numOfTaskWorkers];
            for (int i = 0; i < numOfTaskWorkers; i++) {
                _taskWorkers[i] = new TaskWorker(this, _taskQueue, i + 1);
            }

            _logger.LogInfo("TaskManager", $"{numOfTaskWorkers} workers initialized!");
        }

        internal void AddWorkerToInactivateWorkers(TaskWorker taskWorker) {
            _taskWorkerQueue.Enqueue(taskWorker);
        }

        public void EnqueueTask(ITask task) {
            if (task != null) {
                _taskQueue.Enqueue(task);
                if (_taskWorkerQueue.TryDequeue(out var taskWorker)) {
                    taskWorker.Wake();
                }
            }
        }

        void IModule.Stop() {
            foreach (var taskWorker in _taskWorkers) {
                taskWorker.Stop();
            }
            _logger.LogInfo("TaskManager", $"{_taskWorkers.Length} workers Deinitialized!");
        }
    }
}