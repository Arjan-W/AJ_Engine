using AJ.Graphics.Interfaces.Resources;
using AJ.Graphics.Interfaces.Windowing;

namespace AJ.Graphics.Interfaces
{
    public interface IGraphicsContext
    {
        IWindow Window { get; }
        IResourceManager ResourceManager { get; }
    }
}