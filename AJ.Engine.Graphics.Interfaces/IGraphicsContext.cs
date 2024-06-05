using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Graphics.Interfaces.Resources.Textures;
using AJ.Engine.Graphics.Interfaces.Windowing;
using OpenTK.Mathematics;

namespace AJ.Engine.Graphics.Interfaces
{
    public interface IGraphicsContext
    {
        IDisplay Display { get; }
        IShaderFactory ShaderFactory { get; }
        ITextureFactory TextureFactory { get; }

        void SetClearColor(Color4 color);
        void Clear();
    }
}