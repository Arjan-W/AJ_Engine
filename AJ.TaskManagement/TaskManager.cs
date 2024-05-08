using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;
using AJ.TaskManagement.Interfaces;
using System.Collections.Concurrent;

namespace AJ.TaskManagement
{
    internal class TaskManager : ITaskManager, IModule
    {
        private ILogger _logger;
        private readonly int _numOfTaskWorkers;
        private readonly ConcurrentQueue<ITask> _taskQueue;
        private TaskWorker[] _workers;

        internal TaskManager(IModuleProvider moduleProvider, int numOfTaskWorkers) {
            _logger = moduleProvider.Get<ILogger>();
            _numOfTaskWorkers = numOfTaskWorkers;
            _workers = new TaskWorker[numOfTaskWorkers];
            _taskQueue = new ConcurrentQueue<ITask>();
        }

        void IModule.Start() {
            for (int i = 0; i < _numOfTaskWorkers; i++) {
                _workers[i] = new TaskWorker(_logger, i, _taskQueue);
            }
        }

        void ITaskManager.EnqueueTasks(params ITask[] tasks) {
            foreach (ITask t in tasks) {
                _taskQueue.Enqueue(t);
            }
            foreach (TaskWorker tw in _workers) {
                tw.Wake();
            }
        }

        void IModule.Stop() {
            for (int i = 0; i < _numOfTaskWorkers; i++) {
                _workers[i].Stop();
            }
        }
    }
}