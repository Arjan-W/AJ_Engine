using AJ.Engine.Logging.Interfaces;

namespace AJ.TaskManagement.Interfaces
{
    public interface ITask
    {
        virtual void OnRun(ILogger logger) { }
    }
}