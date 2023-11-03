using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Util;
using AJ.Engine.Graphics.OKT_STG.Util;
using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.Services;
using AJ.Logging.Interfaces;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace AJ.Engine.Graphics.OKT_STG
{
    public class GraphicsContext : IEngineService, IGraphicsContext
    {
        public IWindow Window => _window;

        private IApplication _application;
        private ILogger _logger;
        private NativeWindow _nativeWindow;
        private Window _window;

        public GraphicsContext(ICore core)
        {
            _application = core.Application;
            _logger = core.ServiceProvider.Get<ILogger>();
            NativeWindowSettings nws = new NativeWindowSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(4, 5),
                IsEventDriven = false,
                AutoLoadBindings = true,
                Size = new Vector2i(1280, 720),
                StartFocused = true,
                StartVisible = true,
                Profile = OpenTK.Windowing.Common.ContextProfile.Core,
                Title = _application.Title
            };
            _nativeWindow = new NativeWindow(nws);
            _window = new Window(_nativeWindow);
            _logger.LogInfo("GraphicsContext", "Started!");
        }

        public void Update()
        {
            NativeWindow.ProcessWindowEvents(false);
            _nativeWindow.Context.SwapBuffers();
            _window.Update();
        }

        public void Dispose()
        {
            _logger.LogInfo("GraphicsContext", "Stopped!");
        }
    }
}