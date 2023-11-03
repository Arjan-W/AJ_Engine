﻿using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Util;
using AJ.Engine.Graphics.OKT_STG;
using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.Services;
using AJ.Engine.Services;
using AJ.Logging.Interfaces;
using AJ.Logging.SimpleLogger;
using System.Threading;

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

        public static IApplication Application => _instance._application;
        public static IEngineServiceProvider ServiceProvider => _instance._serviceProvicer;

        private Application _application;
        private bool _isRunning;

        private EngineServiceProvider _serviceProvicer;
        private ILogger _logger;
        private GraphicsContext _graphicsContext;
        
        private Core(Application application)
        {
            _application = application;
            _isRunning = true;
        }

        private void Initialize()
        {
            _serviceProvicer = new EngineServiceProvider();
            _logger = _serviceProvicer.Add<ILogger, Logger>(new Logger(_application));
            _graphicsContext = _serviceProvicer.Add<IGraphicsContext, GraphicsContext>(new GraphicsContext(_application));
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
            _serviceProvicer.Dispose<IGraphicsContext>();
            _serviceProvicer.Dispose<ILogger>();
        }
    }
}