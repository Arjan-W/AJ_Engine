using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;

namespace AJ.Engine.SceneManagement
{
    public abstract class Scene : IScene
    {
        public string Name => _name;

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