using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;

namespace AJ.Engine
{
    public abstract class Application : IApplication
    {
        public string Title => _title;
        public bool EnableGraphics => _enableGraphics;
        public bool CloseOnRequest => _closeOnRequest;

        private readonly string _title;
        private readonly bool _enableGraphics;
        private readonly bool _closeOnRequest;

        public Application(AppSettings appSettings) {
            _title = appSettings.Title;
            _enableGraphics = appSettings.EnableGraphics;
            _closeOnRequest = appSettings.CloseOnRequest;
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