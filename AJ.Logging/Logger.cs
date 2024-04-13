using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using System;
using System.Threading;

namespace AJ.Logging
{
    internal class Logger : IModule
    {
        private readonly IApplication _application;
        private Thread _thread;
        private bool _isRunning;

        internal Logger(IApplication application) {
            _application = application;
            _isRunning = true;
        }

        void IModule.Start() {
            using (AutoResetEvent are = new AutoResetEvent(false)) {
                _thread = new Thread(() => {
                    are.Set();
                    LogLoop();
                });
                _thread.Start();
                are.WaitOne();
            }
        }

        private void LogLoop() {
            while (_isRunning) {
                Console.WriteLine(_application.Title);
            }
        }

        void IModule.Stop() {
            Volatile.Write(ref _isRunning, false);
            _thread.Join();
        }
    }
}