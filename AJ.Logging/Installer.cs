using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;

namespace AJ.Logging
{
    public static class Installer
    {
        public static void Install(IModuleInstaller moduleInstaller, IApplication application) {
            moduleInstaller.Install<ILogger, Logger>(new Logger(application));
        }

        public static void Uninstall(IModuleUninstaller moduleUnInstaller) {
            moduleUnInstaller.Uninstall<ILogger>();
        }
    }
}
