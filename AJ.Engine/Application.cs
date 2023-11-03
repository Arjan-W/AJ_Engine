using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Util;
using AJ.Engine.Interfaces;
using AJ.Logging.Interfaces;

namespace AJ.Engine
{
    public abstract class Application : IApplication
    {
        protected ILogger Logger
        {
            get;
            private set;
        }

        protected IGraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        protected IWindow Window
        {
            get;
            private set;
        }

        public string Title => _title;
        public LogTypes LogToConsole => _logToConsole;
        public LogTypes LogToFile => _logToFile;

        private string _title;
        private LogTypes _logToConsole;
        private LogTypes _logToFile;

        protected Application(string title) 
        {
            _title = title;
            _logToConsole = LogTypes.All;
            _logToFile = LogTypes.All;
        }

        protected void LogTypesToConsole(LogTypes logTypes)
        {
            _logToConsole = logTypes;
        }

        protected void LogTypesToFile(LogTypes logTypesToFile)
        {
            _logToFile = logTypesToFile;
        }

        internal void Initialize()
        {
            Logger = Core.ServiceProvider.Get<ILogger>();
            GraphicsContext = Core.ServiceProvider.Get<IGraphicsContext>();
            Window = GraphicsContext.Window;
            OnInitialize();
        }

        protected abstract void OnInitialize();

        internal void Deinitialize()
        {
            OnDeinitialize();
        }

        protected abstract void OnDeinitialize();
    }
}
