using AJ.Logging.Interfaces;
using System;

namespace AJ.Engine.Interfaces
{
    public interface IApplication
    {
        public string Title { get; }
        public bool EnableGraphics {  get; }
        public bool CloseOnRequest { get; }
        public TimeSpan? DeltaTimeConstraint { get; }
        public LogTypes LogToConsole { get; }
        public LogTypes LogToFile { get; }
    }
}