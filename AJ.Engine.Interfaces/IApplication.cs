using AJ.Logging.Interfaces;

namespace AJ.Engine.Interfaces
{
    public interface IApplication
    {
        public string Title { get; }
        public LogTypes LogToConsole { get; }
        public LogTypes LogToFile { get; }
    }
}