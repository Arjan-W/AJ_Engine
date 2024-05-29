namespace AJ.Engine.Interfaces.TaskManagement
{
    public interface ITaskManager
    {
        void EnqueueTask(ITask task);
    }
}