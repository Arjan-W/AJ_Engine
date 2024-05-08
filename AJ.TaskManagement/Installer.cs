using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.TaskManagement.Interfaces;

namespace AJ.TaskManagement
{
    public static class Installer
    {
        public static void Install(IModuleInstaller moduleInstaller, IModuleProvider moduleProvider, IApplication application) {
            moduleInstaller.Install<ITaskManager, TaskManager>(new TaskManager(moduleProvider, application.NumOfTaskWorkers));
        }

        public static void Uninstall(IModuleUninstaller moduleUninstaller) {
            moduleUninstaller.Uninstall<ITaskManager>();
        }
    }
}
