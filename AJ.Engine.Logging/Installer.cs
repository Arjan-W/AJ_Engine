using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.Logging.Interfaces;

namespace AJ.Engine.Logging
{
    public static class Installer
    {
        public static void Install(IModuleInstaller moduleInstaller, IModuleProvider moduleProvider, IApplication application)
        {
            moduleInstaller.Install<ILogger, Logger>(new Logger(application, moduleProvider.Get<IGameTime>()));
        }

        public static void Uninstall(IModuleUninstaller moduleUnInstaller)
        {
            moduleUnInstaller.Uninstall<ILogger>();
        }
    }
}