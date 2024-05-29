using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Logging.Interfaces;
using AJ.TaskManagement.Interfaces;
using System;
using System.Threading;

namespace AJ.TaskManagement
{
    public static class TestClass
    {

        public static void Test(IModuleProvider moduleProvider) {
            var taskManager = moduleProvider.Get<ITaskManager>();
            var r = new Random();
            taskManager.EnqueueTasks(new TestB(0, r, taskManager));
        }

        public class TestA : ITask {
            private int _id;
            private int _timeOut;
            private int _count;

            public TestA(int id, int timeOut, int count) {
                _id = id;
                _timeOut = Math.Clamp(timeOut, 250, 1000);
                _count = count;
            }

            void ITask.OnRun(ILogger logger) {
                Thread.Sleep(_timeOut);
                logger.LogDebug($"Task[{_id}-{_count}]", $"Completed after {_timeOut}ms!");
            }
        }

        public class TestB : ITask
        {
            private ITaskManager _taskManager;
            private Random _r;
            private int _count;

            public TestB(int count, Random r, ITaskManager taskManager) {
                _taskManager = taskManager;
                _r = r;
                _count = count;
                if(_count <= 10) {
                    for (int i = 0; i < 400; i++)
                        taskManager.EnqueueTasks(new TestA(i, r.Next(1000), _count));
                }
                _count++;
            }

            void ITask.OnRun(ILogger logger) {
                Thread.Sleep(2000);
                var tb = new TestB(_count, _r, _taskManager);
                _taskManager.EnqueueTasks(tb);
            }
        }
    }
}