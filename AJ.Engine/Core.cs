using AJ.Engine.FileManagement;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.SceneManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.ModuleManagement;
using AJ.Engine.SceneManagement;
using AJ.Engine.TimeManagement;
using AJ.Logging.Interfaces;
using CurrentLogger = AJ.LoggingV2.Installer;
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

        public static IModuleProvider ModuleProvider => _instance._moduleManager;

        private readonly Application _application;
        private readonly ModuleManager _moduleManager;
        private bool _isRunning;

        private Core(Application application) {
            _application = application;
            _moduleManager = new ModuleManager();
            _isRunning = true;
        }

        private void Initialize() {
            _moduleManager.Install<IGameTime, GameTime>(new GameTime());
            CurrentLogger.Install(_moduleManager, _moduleManager, _application);
            _moduleManager.Install<IFileManager, FileManager>(new FileManager(_moduleManager));
            TaskManagement.Installer.Install(_moduleManager, _moduleManager, _application);
            AJ.Graphics.OpenTK.Installer.Install(_moduleManager, _moduleManager, _application);
            _moduleManager.Install<ISceneManager, SceneManager>(new SceneManager());
        }

        private void GameLoop() {
            var logger = _moduleManager.Get<ILogger>();
            while (_isRunning) {
                _moduleManager.Update();
                logger.LogInfo("test", "testline");
            }
        }

        private void Deinitialize() {
            AJ.Graphics.OpenTK.Installer.Uninstall(_moduleManager);
            TaskManagement.Installer.Uninstall(_moduleManager);
            CurrentLogger.Uninstall(_moduleManager);
        }
    }
}