using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces;
using AJ.Graphics.Interfaces;

namespace AJ.Graphics.OpenTK
{
    public static class Installer
    {
        public static void Install(IModuleInstaller moduleInstaller, IModuleProvider moduleProvider, IApplication application) {
            moduleInstaller.Install<IGraphicsContext, GraphicsContext>(new GraphicsContext(moduleProvider, application));
        }

        public static void Uninstall(IModuleUninstaller moduleUninstaller) {
            moduleUninstaller.Uninstall<IGraphicsContext>();
        }
    }
}