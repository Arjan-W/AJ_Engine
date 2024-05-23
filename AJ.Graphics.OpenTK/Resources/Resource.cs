using AJ.Logging.Interfaces;
using AJ.TaskManagement.Interfaces;

namespace AJ.Graphics.OpenTK.Resources
{
    internal abstract class Resource : ITask
    {
        public bool IsLoaded => _isLoaded;
        public bool IsInitialized => _isInit;
        public bool IsDeleted => _isDeleted;
        public bool IsReady => _isLoaded && _isInit && !_isDeleted;

        private bool _isLoaded;
        private bool _isInit;
        private bool _isDeleted;

        protected Resource() {
            _isLoaded = false;
            _isInit = false;
            _isDeleted = false;
        }

        internal void LoadResource(ILogger logger) {
            _isLoaded = OnLoadResource(logger);
            if(_isLoaded) {
                GraphicsContext.InternalResourceManager.QueueForInit(this);
            }
        }

        protected virtual bool OnLoadResource(ILogger logger) { return true; }

        internal void InitResource(ILogger logger) {
            _isInit = OnInitResource(logger);
        }

        protected virtual bool OnInitResource(ILogger logger) { return true; }

        internal void DeleteResource(ILogger logger) {
            if (!_isDeleted && _isInit) {
                _isLoaded = false;
                _isDeleted = false;
                _isDeleted = true;
                OnDeleteResource(logger);
            }
        }

        protected virtual void OnDeleteResource(ILogger logger) { }
    }
}