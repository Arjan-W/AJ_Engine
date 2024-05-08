using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;

namespace AJ.Logging
{
    public static class Installer
    {
        public static void Install(IModuleInstaller moduleInstaller, IModuleProvider moduleProvider, IApplication application) {
            moduleInstaller.Install<ILogger, Logger>(new Logger(moduleProvider, application));
        }

        public static void Uninstall(IModuleUninstaller moduleUnInstaller) {
            moduleUnInstaller.Uninstall<ILogger>();
        }
    }
}
