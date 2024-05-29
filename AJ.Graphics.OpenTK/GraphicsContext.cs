using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Graphics.Interfaces.Resources;
using AJ.Graphics.Interfaces.Windowing;
using AJ.Graphics.OpenTK.Resources;
using AJ.Graphics.OpenTK.Windowing;
using AJ.Logging.Interfaces;
using AJ.TaskManagement.Interfaces;
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
        public IResourceManager ResourceManager => _resourceManager;

        private static GraphicsContext _instance;
        public static ResourceManager InternalResourceManager => _instance._resourceManager;
        public static ILogger InternalLogger => _instance._logger;
        public static IFileManager InternalFileManager => _instance._fileManager;
        public static ITaskManager InternalTaskManager => _instance._taskManager;
        
        private readonly ILogger _logger;
        private readonly IFileManager _fileManager;
        private readonly ITaskManager _taskManager;

        private readonly NativeWindow _nativeWindow;
        private readonly Window _window;
        private readonly ResourceManager _resourceManager;

        internal GraphicsContext(IModuleProvider moduleProvider, IApplication application) {
            _instance = this;

            _logger = moduleProvider.Get<ILogger>();
            _fileManager = moduleProvider.Get<IFileManager>();
            _taskManager = moduleProvider.Get<ITaskManager>();

            NativeWindowSettings nws = new NativeWindowSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 5),
                Profile = ContextProfile.Core,
                AutoLoadBindings = true,
                IsEventDriven = false,
                ClientSize = new Vector2i(1920, 1080),
                Title = application.Title,
                StartVisible = true,
                WindowBorder = WindowBorder.Resizable
            };

            _nativeWindow = new NativeWindow(nws);
            _window = new Window(_nativeWindow);
            _resourceManager = new ResourceManager(moduleProvider);
            _logger.LogInfo(
                "GraphicsContext initialized!",
                $"{GL.GetString(StringName.Vendor)}" +
                $"{GL.GetString(StringName.Version)}" +
                $"{GL.GetString(StringName.Renderer)}"
                );
        }

        void IModule.Update() {
            _nativeWindow.NewInputFrame();
            NativeWindow.ProcessWindowEvents(false);
            _resourceManager.Update();
            _window.Update();
            _nativeWindow.Context.SwapBuffers();
        }

        void IModule.Stop() {
            _nativeWindow.Close();
            _nativeWindow.Dispose();
        }
    }
}