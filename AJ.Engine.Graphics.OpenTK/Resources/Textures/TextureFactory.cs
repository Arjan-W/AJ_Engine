using AJ.Engine.Graphics.Interfaces.Resources.Textures;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Logging.Interfaces;

namespace AJ.Engine.Graphics.OpenTK.Resources.Textures
{
    internal class TextureFactory : ITextureFactory
    {
        private const string LOG_TITLE = "TextureFactory";

        private readonly ILogger _logger;
        private readonly IFileManager _fileManager;
        private readonly ITaskManager _taskManager;

        internal TextureFactory()
        {
            _logger = GraphicsContext.Logger;
            _fileManager = GraphicsContext.FileManager;
            _taskManager = GraphicsContext.TaskManager;
        }

        public ITextureHandle CreateTexture(string filePath, bool flipHorizontal = false, bool flipVertical = false)
        {
            IFileHandle fileHandle = null;
            TextureHandle textureHandle = null;

            if (string.IsNullOrWhiteSpace(filePath))
                _logger.LogWarning(LOG_TITLE, "Given texture filepath is null or exclusively white space!");
            else if ((fileHandle = _fileManager.LoadFile(filePath)) == null)
                _logger.LogWarning(LOG_TITLE, $"Cannot find a file with filePath: {filePath}!");
            else
            {
                textureHandle = new TextureHandle(fileHandle, flipHorizontal, flipVertical);
                _taskManager.EnqueueTask(textureHandle);
            }

            return textureHandle;
        }

        public void DisposeTexture(ITextureHandle texture)
        {
            TextureHandle th = (TextureHandle)texture;
            if (th != null && th.IsReady)
                th.Dispose();
        }

        public ISamplerHandle CreateSampler()
        {
            SamplerHandle sh = new SamplerHandle();
            sh.Initialize();
            return sh;
        }

        public void DisposeSampler(ISamplerHandle sampler)
        {
            SamplerHandle sh = (SamplerHandle)sampler;
            if (sh != null && sh.IsReady)
            {
                sh.Dispose();
            }
        }
    }
}
