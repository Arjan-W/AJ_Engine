using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Logging.Interfaces;

namespace AJ.Engine.Graphics.OpenTK.Resources.Shaders;

internal class ShaderFactory : IShaderFactory
{
    private const string LOG_TITLE = "ShaderFactory";
    private const string SHADER_FILE_EXTENSION = ".glsl";

    private readonly ILogger _logger;
    private readonly IFileManager _fileManager;
    private readonly ITaskManager _taskManager;

    internal ShaderFactory()
    {
        _logger = GraphicsContext.Logger;
        _fileManager = GraphicsContext.FileManager;
        _taskManager = GraphicsContext.TaskManager;
    }

    public IShaderHandle CreateShader(string filePath)
    {
        IFileHandle fileHandle = null;
        ShaderHandle shaderHandle = null;

        if (string.IsNullOrWhiteSpace(filePath)) {
            _logger.LogWarning(LOG_TITLE, "Given shader filepath is null or exclusively white space!");
            return null;
        }

        filePath += SHADER_FILE_EXTENSION;

        if (!filePath.EndsWith(SHADER_FILE_EXTENSION))
            _logger.LogWarning(LOG_TITLE, $"Only {SHADER_FILE_EXTENSION} supported!");
        else if ((fileHandle = _fileManager.LoadFile(filePath)) == null)
            _logger.LogWarning(LOG_TITLE, $"Cannot find a file with filePath: {filePath}!");
        else
        {
            shaderHandle = new ShaderHandle(fileHandle);
            _taskManager.EnqueueTask(shaderHandle);
        }

        return shaderHandle;
    }

    public void DisposeShader(IShaderHandle shader)
    {
        ShaderHandle sh = (ShaderHandle)shader;
        if (sh != null && sh.IsReady)
            sh.Dispose();
    }
}