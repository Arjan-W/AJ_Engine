using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;

namespace AJ.Engine.SceneManagement
{
    public abstract class Scene : IScene
    {
        public string Name => _name;

        public bool IsLoaded => throw new System.NotImplementedException();

        public bool IsInit => throw new System.NotImplementedException();

        public bool IsReady => throw new System.NotImplementedException();

        public bool IsRemoved => throw new System.NotImplementedException();

        private readonly ILogger _logger;

        private readonly string _name;
        private bool _isLoaded;
        private bool _isInit;
        private bool _isRemoved;

        protected Scene(string name, IModuleProvider moduleProvider) {
            _logger = moduleProvider.Get<ILogger>();
            _name = name;
            _isLoaded = true;
        }

        internal void Deinitialize() {
            
        }
    }
}