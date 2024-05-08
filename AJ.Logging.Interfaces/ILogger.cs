using System;

namespace AJ.Logging.Interfaces
{
    [Flags]
    public enum LogTypes : byte
    {
        NONE = 0,
        INFO = 1,
        WARNING = 2,
        ERROR = 4,
        FATAL = 8,
        DEBUG = 16,
        ALL = 255
    }

    public interface ILogger
    {
        void LogInfo(string title, params string[] messages);
        void LogWarning(string title, params string[] messages);
        void LogError(string title, params string[] messages);
        void LogDebug(string title, params string[] messages);
    }
}