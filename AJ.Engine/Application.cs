using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;
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

        private readonly string _title;
        private readonly bool _enableGraphics;
        private readonly bool _closeOnRequest;
        private readonly TimeSpan? _deltaTimeConstraint;
        private readonly LogTypes _logToConsole;
        private readonly LogTypes _logToFile;

        public Application(AppSettings appSettings) {
            _title = appSettings.Title;
            _enableGraphics = appSettings.EnableGraphics;
            _closeOnRequest = appSettings.CloseOnRequest;
            _deltaTimeConstraint = appSettings.DeltaTimeConstraint;
            _logToConsole = appSettings.LogToConsole;
            _logToFile = appSettings.LogToFile;
        }

        internal void Initialize() {
            OnInitialize();
        }

        protected abstract void OnInitialize();

        internal void Deinitialize() {
            OnDeinitialize();
        }

        protected abstract void OnDeinitialize();
    }
}