using AJ.Engine.Interfaces.Services;

namespace AJ.Engine.Interfaces
{
    public interface ICore
    {
        IApplication Application { get; }
        IEngineServiceProvider ServiceProvider { get; }
        void Stop();
    }
}