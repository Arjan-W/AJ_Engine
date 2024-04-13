namespace AJ.Engine.Interfaces.ModuleManagement
{
    public interface IModuleInstaller
    {
        void Install<MI, MC>(MC module) where MC : IModule;
    }
}