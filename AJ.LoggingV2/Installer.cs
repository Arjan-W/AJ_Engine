using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Logging.Interfaces;

namespace AJ.LoggingV2
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