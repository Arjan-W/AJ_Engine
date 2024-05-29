using AJ.Engine.FileManagement;
using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.Interfaces.Util;
using AJ.Engine.Logging.Interfaces;
using AJ.Engine.ModuleManagement;
using AJ.Engine.TaskManagement;
using AJ.Engine.TimeManagement;
using System;

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
            Logging.Installer.Install(_moduleManager, _moduleManager, _application);
            _moduleManager.Install<IFileManager, FileManager>(new FileManager(_moduleManager));
            _moduleManager.Install<ITaskManager, TaskManager>(new TaskManager(_application.NumOfTaskWorkers));
            Graphics.OpenTK.Installer.Install(_moduleManager, _moduleManager, _application);
            //last step
            ModuleProvider.Get<IFileManager>().ScanInternalFiles();
        }

        private void GameLoop() {
            try {
                IShaderHandle sh = _moduleManager.Get<IGraphicsContext>().ShaderFactory.CreateShader("AJ.Engine.Graphics.Interfaces.Assets.Shaders.DefaultShader"); //<= we just want a default shader the graphics implementation is responsible for the file extension
                while (_isRunning) {
                    _moduleManager.Update();
                }
                _moduleManager.Get<IGraphicsContext>().ShaderFactory.DisposeShader(sh);
            }
            catch(Exception e) {
                Stop();
                ModuleProvider.Get<ILogger>().LogFatal(
                    "FatalError",
                    $"Fatal error encountered!{Globals.NewLine}" +
                    $"{e}"
                    );
            }
        }

        private void Deinitialize() {
            Graphics.OpenTK.Installer.Uninstall(_moduleManager);
            _moduleManager.Uninstall<ITaskManager>();
            Logging.Installer.Uninstall(_moduleManager);
        }
    }
}