using AJ.Engine.Graphics.Interfaces.Resources.Textures;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Interfaces.Util;
using AJ.Engine.Logging.Interfaces;
using SkiaSharp;
using System;

namespace AJ.Engine.Graphics.OpenTK.Resources.Textures
{
    internal class TextureHandle : Resource, ITextureHandle, ITask
    {
        private const string LOG_TITLE = "TextureHandle";

        public int Width => _width;
        public int Height => _height;
        public bool FlippedHorizontal => _flippedHorizontal;
        public bool FlippedVertical => _flippedVertical;

        private readonly ILogger _logger;
        private readonly IFileHandle _fileHandle;
        private int _width;
        private int _height;
        private byte[] _pixels;
        private readonly bool _flippedHorizontal;
        private readonly bool _flippedVertical;
        private int _textureId;

        public TextureHandle(IFileHandle fileHandle, bool flipHorizontal, bool flipVertical) {
            _logger = GraphicsContext.Logger;
            _fileHandle = fileHandle;
            _width = 0;
            _height = 0;
            _pixels = null;
            _flippedHorizontal = flipHorizontal;
            _flippedVertical = flipVertical;
            _textureId = INVALID_ID;
        }

        public bool OnRunTask() => Load();

        protected override bool OnLoad() {
            try {
                using (SKBitmap bmp = SKBitmap.Decode(_fileHandle.OpenRead())) {
                    Span<byte> pixelBuffer = stackalloc byte[bmp.ByteCount];
                    bmp.Bytes.CopyTo(pixelBuffer);

                    if (_flippedHorizontal)
                        FlipHorizontal(ref pixelBuffer, bmp);
                    if (_flippedVertical)
                        FlipVertical(ref pixelBuffer, bmp);

                    _width = bmp.Width;
                    _height = bmp.Height;
                    _pixels = pixelBuffer.ToArray();
                }
                _logger.LogDebug(LOG_TITLE, $"Texture data loaded from {_fileHandle.AbsolutePath}!");
                return true;
            }
            catch (Exception e) {
                _logger.LogError(LOG_TITLE, $"Encountered a error while trying to load texture {_fileHandle.AbsolutePath}{Globals.NewLine}{e.ToString()}");
                return false;
            }
        }

        private void FlipHorizontal(ref Span<byte> data, SKBitmap bitmap) {
            int bytesPerPixel = bitmap.BytesPerPixel;
            int pixelsInRow = bitmap.Width;
            int rowSize = pixelsInRow * bytesPerPixel;
            int rows = bitmap.ByteCount / rowSize;
            Span<byte> rowBuffer = stackalloc byte[rowSize];

            for (int row = 0; row < rows; row++) {
                int offset = row * rowSize;
                data.Slice(offset, rowSize).CopyTo(rowBuffer);
                for (int pix = 0; pix < pixelsInRow; pix++) {
                    rowBuffer.Slice((pixelsInRow - (pix + 1)) * bytesPerPixel, bytesPerPixel).CopyTo(data.Slice(offset + pix * bytesPerPixel));
                }
            }

        }

        private void FlipVertical(ref Span<byte> data, SKBitmap bitmap) {
            int bytesPerPixel = bitmap.BytesPerPixel;
            int pixelsInRow = bitmap.Width;
            int rowSize = pixelsInRow * bytesPerPixel;
            int rows = bitmap.ByteCount / rowSize;
            int totalNumOfPixels = rows * rowSize;
            Span<byte> pixelBuffer = stackalloc byte[data.Length];

            for (int row = 0; row < rows; row++) {
                int offset = totalNumOfPixels - (row + 1) * rowSize;
                data.Slice(offset, rowSize).CopyTo(pixelBuffer.Slice(row * rowSize));
            }

            pixelBuffer.CopyTo(data);
        }

        protected override bool OnInitialize() {
            _textureId = GL.GenTexture();
            if (_textureId <= 0) {
                _logger.LogError(LOG_TITLE, $"Failed to create a texture id for {_fileHandle.AbsolutePath}!");
                return false;
            }

            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, _pixels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            _logger.LogDebug(LOG_TITLE, $"TextureHandle {_fileHandle.AbsolutePath} initialized!");
            return true;
        }

        protected override void OnDispose() {
            GL.DeleteTexture(_textureId);
            _textureId = INVALID_ID;
            _logger.LogDebug(LOG_TITLE, $"Deleted textureHandle {_fileHandle.AbsolutePath}!");
        }
    }
}