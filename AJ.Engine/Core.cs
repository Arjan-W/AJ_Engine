using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Util;
using AJ.Engine.Graphics.OKT_STG;

namespace AJ.Engine
{
    public class Core
    {
        private static Core _instance;

        public static void Run(Application application)
        {
            if (_instance == null)
            {
                _instance = new Core(application);
                _instance.Initialize();
                _instance.GameLoop();
                _instance.Deinitialize();
            }
        }

        public static void Stop()
        {
            Volatile.Write(ref _instance._isRunning, false);
        }

        public static IGraphicsContext GraphicsContext => _instance._graphicsContext;
        public static IWindow Window => _instance._graphicsContext.Window;

        private Application _application;
        private bool _isRunning;

        private GraphicsContext _graphicsContext;
        

        private Core(Application application)
        {
            _application = application;
            _isRunning = true;
        }

        private void Initialize()
        {
            _graphicsContext = new GraphicsContext(_application);
            _application.Initialize();
        }

        private void GameLoop()
        {
            while(_isRunning)
            {
                _graphicsContext.Update();
            }
        }

        private void Deinitialize()
        {
            _application.Deinitialize();
        }
    }
}