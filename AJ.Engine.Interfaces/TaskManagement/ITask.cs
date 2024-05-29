namespace AJ.Engine.Interfaces.TaskManagement
{
    public interface ITask
    {
        virtual void OnStart() { }
        bool OnRunTask();
        virtual void OnFinished(bool completed) { }
    }
}