using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Graphics.Interfaces.Windowing;
using AJ.Graphics.OpenTK.Windowing;
using AJ.Logging.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using IGraphicsContext = AJ.Graphics.Interfaces.IGraphicsContext;

namespace AJ.Graphics.OpenTK
{
    internal class GraphicsContext : IGraphicsContext, IModule
    {
        public IWindow Window => _window;

        private readonly ILogger _logger;

        private readonly NativeWindow _nativeWindow;
        private readonly Window _window;

        internal GraphicsContext(IModuleProvider moduleProvider, IApplication application) {
            _logger = moduleProvider.Get<ILogger>();

            NativeWindowSettings nws = new NativeWindowSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 5),
                AutoLoadBindings = true,
                IsEventDriven = false,
                ClientSize = new Vector2i(1920, 1080),
                Title = application.Title,
                StartVisible = true,
                WindowBorder = WindowBorder.Resizable
            };

            _nativeWindow = new NativeWindow(nws);
            _window = new Window(_nativeWindow);

            _logger.LogInfo(
                "GraphicsContext initialized!",
                GL.GetString(StringName.Version),
                GL.GetString(StringName.Vendor),
                GL.GetString(StringName.Renderer)
                );
        }

        void IModule.Update() {
            _nativeWindow.NewInputFrame();
            NativeWindow.ProcessWindowEvents(false);
            _window.Update();
            _nativeWindow.Context.SwapBuffers();
        }

        void IModule.Stop() {
            _nativeWindow.Close();
            _nativeWindow.Dispose();
        }
    }
}