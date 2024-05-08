namespace AJ.TaskManagement.Interfaces
{
    public interface ITaskManager
    {
        void EnqueueTasks(params ITask[] tasks);
    }
}