using System;

namespace AJ.Logging.Interfaces
{
    [Flags]
    public enum LogTypes : byte
    {
        None = 0,
        Info= 1,
        Warning = 2,
        Error = 4,
        Fatal = 8,
        Debug = 16,
        All = 255
    }

    public interface ILogger
    {
        void LogInfo(string title, params string[] messages);
        void LogWarning(string title, params string[] messages);
        void LogError(string title, params string[] messages);
        void LogFatal(string title, params string[] messages);
        void LogDebug(string title, params string[] messages);
    }
}
