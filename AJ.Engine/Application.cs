using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Windowing;
using AJ.Engine.Interfaces;
using AJ.Engine.Logging.Interfaces;
using System;

namespace AJ.Engine
{
    public abstract class Application : IApplication
    {
        public string Title => _title;
        public bool EnableGraphics => _enableGraphics;
        public bool CloseOnRequest => _closeOnRequest;
        public TimeSpan? DeltaTimeConstraint => _deltaTimeConstraint;
        public LogTypes LogToConsole => _logToConsole;
        public LogTypes LogToFile => _logToFile;
        public int NumOfTaskWorkers => _numOftaskWorkers;

        private readonly string _title;
        private readonly bool _enableGraphics;
        private readonly bool _closeOnRequest;
        private readonly TimeSpan? _deltaTimeConstraint;
        private readonly LogTypes _logToConsole;
        private readonly LogTypes _logToFile;
        private readonly int _numOftaskWorkers;

        public Application(AppSettings appSettings) {
            _title = appSettings.Title;
            _enableGraphics = appSettings.EnableGraphics;
            _closeOnRequest = appSettings.CloseOnRequest;
            _deltaTimeConstraint = appSettings.DeltaTimeConstraint;
            _logToConsole = appSettings.LogToConsole;
            _logToFile = appSettings.LogToFile;
            _numOftaskWorkers = appSettings.NumOfTaskWorkers;
        }

        internal void Initialize() {
            if (_closeOnRequest) {
                IDisplay window = Core.ModuleProvider.Get<IGraphicsContext>().Display;
                window.OnCloseRequest += () => Core.Stop();
            }
            OnInitialize();
        }

        protected abstract void OnInitialize();

        internal void Deinitialize() {
            OnDeinitialize();
        }

        protected abstract void OnDeinitialize();
    }
}