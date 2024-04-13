namespace AJ.Engine.Interfaces.ModuleManagement
{
    public interface IModule
    {
        virtual void Start() { }
        virtual void Update() { }
        virtual void Stop() { }
    }
}