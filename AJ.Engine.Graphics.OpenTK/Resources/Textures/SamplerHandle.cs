using AJ.Engine.Graphics.Interfaces.Resources;
using AJ.Engine.Graphics.Interfaces.Resources.Textures;
using AJ.Engine.Logging.Interfaces;

namespace AJ.Engine.Graphics.OpenTK.Resources.Textures
{
    internal class SamplerHandle : Resource, ISamplerHandle, IResource
    {
        private const string LOG_TITLE = "SamplerHandle";

        private readonly ILogger _logger;
        private int _samplerId;

        internal SamplerHandle() : base(false)
        {
            _logger = GraphicsContext.Logger;
            _samplerId = INVALID_ID;
        }

        protected override bool OnInitialize()
        {
            GL.CreateSamplers(1, out int samplerId);
            if (GL.IsSampler(samplerId))
            {
                _samplerId = samplerId;
                _logger.LogDebug(LOG_TITLE, $"Created samplerHandle_{_samplerId}!");
                return true;
            }
            return false;
        }

        public void SetMinificationFilter(MinificationFilter filter)
        {
            if (IsReady)
            {
                int textureMinFilter = GetTextureMinFilter(filter);
                if (textureMinFilter != INVALID_ID)
                {
                    GL.SamplerParameter(_samplerId, SamplerParameterName.TextureMinFilter, textureMinFilter);
                }
            }
        }

        private int GetTextureMinFilter(MinificationFilter filter) => filter switch
        {
            MinificationFilter.NEAREST => (int)TextureMinFilter.Nearest,
            MinificationFilter.LINEAR => (int)TextureMinFilter.Linear,
            MinificationFilter.NEAREST_MIPMAP_NEAREST => (int)TextureMinFilter.NearestMipmapNearest,
            MinificationFilter.NEAREST_MIPMAP_LINEAR => (int)TextureMinFilter.NearestMipmapLinear,
            MinificationFilter.LINEAR_MIPMAP_NEAREST => (int)TextureMinFilter.LinearMipmapNearest,
            MinificationFilter.LINEAR_MIPMAP_LINEAR => (int)TextureMinFilter.LinearMipmapLinear,
            _ => INVALID_ID
        };

        public void SetMagnificationFilter(MagnificationFilter filter)
        {
            if (IsReady)
            {
                int textureMagFilter = GetTextureMagFilter(filter);
                if (textureMagFilter != INVALID_ID)
                {
                    GL.SamplerParameter(_samplerId, SamplerParameterName.TextureMagFilter, textureMagFilter);
                }
            }
        }

        private int GetTextureMagFilter(MagnificationFilter filter) => filter switch
        {
            MagnificationFilter.NEAREST => (int)TextureMagFilter.Nearest,
            MagnificationFilter.LINEAR => (int)TextureMagFilter.Linear,
            _ => INVALID_ID
        };

        public void SetTextureHorizontalWrap(WrapMode wrapMode)
        {
            if (IsReady)
            {
                int textureWrapMode = GetTextureWrap(wrapMode);
                if (textureWrapMode != INVALID_ID)
                {
                    GL.SamplerParameter(_samplerId, SamplerParameterName.TextureWrapS, textureWrapMode);
                }
            }
        }

        public void SetTextureVerticalWrap(WrapMode wrapMode)
        {
            if (IsReady)
            {
                int textureWrapMode = GetTextureWrap(wrapMode);
                if (textureWrapMode != INVALID_ID)
                {
                    GL.SamplerParameter(_samplerId, SamplerParameterName.TextureWrapT, textureWrapMode);
                }
            }
        }

        private int GetTextureWrap(WrapMode wrapMode) => wrapMode switch
        {
            WrapMode.REPEAT => (int)TextureWrapMode.Repeat,
            WrapMode.MIRRORED_REPEAT => (int)TextureWrapMode.MirroredRepeat,
            WrapMode.CLAMP_TO_BORDER => (int)TextureWrapMode.ClampToBorder,
            WrapMode.CLAMP_TO_EDGE => (int)TextureWrapMode.ClampToEdge,
            _ => INVALID_ID
        };

        protected override void OnDispose()
        {
            GL.DeleteSampler(_samplerId);
            _logger.LogDebug(LOG_TITLE, $"Deleted samplerHandle_{_samplerId}!");
        }
    }
}