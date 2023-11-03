using AJ.Engine.Interfaces.Services;
using AJ.Logging.Interfaces;

namespace AJ.Engine.Logging
{
    internal class FauxLogger : IEngineService, ILogger
    {
        public void Dispose()
        {
            
        }

        public void LogDebug(string title, params string[] messages)
        {
            
        }

        public void LogError(string title, params string[] messages)
        {
            
        }

        public void LogFatal(string title, params string[] messages)
        {
            
        }

        public void LogInfo(string title, params string[] messages)
        {
            
        }

        public void LogWarning(string title, params string[] messages)
        {
            
        }
    }
}
