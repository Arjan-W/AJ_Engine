using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Graphics.Interfaces.Windowing;

namespace AJ.Engine.Graphics.Interfaces
{
    public interface IGraphicsContext
    {
        IDisplay Display { get; }
        IShaderFactory ShaderFactory { get; }
    }
}