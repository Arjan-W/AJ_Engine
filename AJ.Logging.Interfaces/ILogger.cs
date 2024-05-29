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
        void LogInfo(string title, string message);
        void LogWarning(string title, string message);
        void LogError(string title, string message);
        void LogFatal(string title, string message);
        void LogDebug(string title, string message);
    }
}