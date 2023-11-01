using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Util;
using AJ.Engine.Graphics.OKT_STG.Util;
using AJ.Engine.Interfaces;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace AJ.Engine.Graphics.OKT_STG
{
    public class GraphicsContext : IGraphicsContext
    {
        public IWindow Window => _window;

        private NativeWindow _nativeWindow;
        private Window _window;

        public GraphicsContext(IApplication application)
        {
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
                Title = application.Title
            };
            _nativeWindow = new NativeWindow(nws);
            _window = new Window(_nativeWindow);
        }

        public void Update()
        {
            NativeWindow.ProcessWindowEvents(false);
            _nativeWindow.Context.SwapBuffers();
            _window.Update();
        }

    }
}