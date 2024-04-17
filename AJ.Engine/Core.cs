using AJ.Engine.FileManagement;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.ModuleManagement;
using AJ.Engine.TimeManagement;
using AJ.Logging.Interfaces;

namespace AJ.Engine
{
    public class Core
    {
        private static Core _instance;

        public static void Run(Application application) {
            if (_instance == null) {
                _instance = new Core(application);
                _instance.Initialize();
                application.Initialize();
                _instance.GameLoop();
                application.Deinitialize();
                _instance.Deinitialize();
            }
        }

        public static void Stop() {
            if (_instance != null) {
                _instance._isRunning = false;
            }
        }

        private static IModuleProvider ModuleProvider => _instance._moduleManager;

        private readonly Application _application;
        private readonly ModuleManager _moduleManager;
        private bool _isRunning;

        private Core(Application application) {
            _application = application;
            _moduleManager = new ModuleManager();
            _isRunning = true;
        }

        private void Initialize() {
            Logging.Installer.Install(_moduleManager, _application);
            _moduleManager.Install<IGameTime, GameTime>(new GameTime());
            _moduleManager.Install<IFileManager, FileManager>(new FileManager(_moduleManager));
        }

        private void GameLoop() {
            while (_isRunning) {
                _moduleManager.Update();
            }
        }

        private void Deinitialize() {
            Logging.Installer.Uninstall(_moduleManager);
        }
    }
}