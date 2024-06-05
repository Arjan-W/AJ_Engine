using AJ.Engine.FileManagement;
using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Graphics.Interfaces.Resources.Textures;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.Interfaces.Util;
using AJ.Engine.Logging.Interfaces;
using AJ.Engine.ModuleManagement;
using AJ.Engine.TaskManagement;
using AJ.Engine.TimeManagement;
using OpenTK.Mathematics;
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
                IGraphicsContext gc = _moduleManager.Get<IGraphicsContext>();
                IShaderHandle sh = gc.ShaderFactory.CreateShader("AJ.Engine.Graphics.Interfaces.Assets.Shaders.DefaultShader"); //<= we just want a default shader the graphics implementation is responsible for the file extension
                ITextureHandle th1 = gc.TextureFactory.CreateTexture("AJ.Engine.Graphics.Interfaces.Assets.Textures.horiFlipTest.png", true, false);
                ITextureHandle th2 = gc.TextureFactory.CreateTexture("AJ.Engine.Graphics.Interfaces.Assets.Textures.vertFlipTest.png", false, true);

                ISamplerHandle sah = gc.TextureFactory.CreateSampler();
                sah.SetTextureVerticalWrap(WrapMode.CLAMP_TO_EDGE);
                sah.SetTextureHorizontalWrap(WrapMode.CLAMP_TO_EDGE);
                sah.SetMagnificationFilter(MagnificationFilter.LINEAR);
                sah.SetMinificationFilter(MinificationFilter.LINEAR);

                gc.SetClearColor(Color4.CornflowerBlue);

                while (_isRunning) {
                    _moduleManager.Update();
                    gc.Clear();
                }

                gc.ShaderFactory.DisposeShader(sh);
                gc.TextureFactory.DisposeTexture(th1);
                gc.TextureFactory.DisposeTexture(th2);
                gc.TextureFactory.DisposeSampler(sah);
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