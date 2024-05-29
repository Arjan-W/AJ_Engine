using AJ.Engine.Graphics.Interfaces;
using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Graphics.Interfaces.Windowing;
using AJ.Engine.Graphics.OpenTK.Resources;
using AJ.Engine.Graphics.OpenTK.Resources.Shaders;
using AJ.Engine.Graphics.OpenTK.Windowing;
using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Interfaces.Util;
using AJ.Engine.Logging.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using IGraphicsContext = AJ.Engine.Graphics.Interfaces.IGraphicsContext;

namespace AJ.Engine.Graphics.OpenTK
{
    internal class GraphicsContext : IGraphicsContext, IModule
    {
        private static GraphicsContext _instance;
        internal static ILogger Logger => _instance._logger;
        internal static IFileManager FileManager => _instance._fileManager;
        internal static ITaskManager TaskManager => _instance._taskManager;
        internal static Display Display => _instance._display;
        internal static ResourceManager ResourceManager => _instance._resourceManager;
        internal static ShaderFactory ShaderFactory => _instance._shaderFactory;

        IDisplay IGraphicsContext.Display => _display;
        IShaderFactory IGraphicsContext.ShaderFactory => _shaderFactory;

        private readonly ILogger _logger;
        private readonly IFileManager _fileManager;
        private readonly ITaskManager _taskManager;

        private readonly NativeWindow _nativeWindow;
        private readonly Display _display;
        private readonly ResourceManager _resourceManager;
        private readonly ShaderFactory _shaderFactory;

        internal GraphicsContext(IModuleProvider moduleProvider, IApplication application)
        {
            _instance = this;

            _logger = moduleProvider.Get<ILogger>();
            _fileManager = moduleProvider.Get<IFileManager>();
            _taskManager = moduleProvider.Get<ITaskManager>();

            _nativeWindow = InitializeOpenTK(application);
            _display = new Display(_nativeWindow);
            _resourceManager = new ResourceManager();
            _shaderFactory = new ShaderFactory();
        }

        private NativeWindow InitializeOpenTK(IApplication application)
        {
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

            NativeWindow nw = new NativeWindow(nws);

            _logger.LogInfo(
                "GraphicsContext",
                $"GraphicsContext Initialized!{Globals.NewLine}" +
                $"Vendor := {GL.GetString(StringName.Vendor)}{Globals.NewLine}" +
                $"GPU := {GL.GetString(StringName.Renderer)}{Globals.NewLine}" +
                $"Version := {GL.GetString(StringName.Version)}");

            return nw;
        }

        void IModule.Update()
        {
            _nativeWindow.NewInputFrame();
            NativeWindow.ProcessWindowEvents(false);
            _resourceManager.Update();
            _display.Update();
            _nativeWindow.Context.SwapBuffers();
        }

        void IModule.Stop()
        {
            _nativeWindow.Close();
            _nativeWindow.Dispose();
            _logger.LogInfo("GraphicsContext", "GraphicsContext Deinitialized!");
        }
    }
}