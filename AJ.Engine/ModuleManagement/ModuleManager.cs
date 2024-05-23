using AJ.Engine.Interfaces.ModuleManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AJ.Engine.ModuleManagement
{
    internal class ModuleManager : IModuleInstaller, IModuleProvider, IModuleUninstaller
    {
        private List<IModule> _moduleUpdateList;
        private Dictionary<Type, IModule> _modules;

        internal ModuleManager() {
            _moduleUpdateList = new List<IModule>();
            _modules = new Dictionary<Type, IModule>();
        }

        public void Install<MI, MC>(MC module) where MC : IModule {
            Type moduleInterfaceType = typeof(MI);
            Type moduleClassType = typeof(MC);

            if (moduleClassType.GetInterfaces().Contains(moduleInterfaceType)) {
                if (!_modules.ContainsKey(moduleInterfaceType)) {
                    _modules.Add(moduleInterfaceType, module);
                    _moduleUpdateList.Add(module);
                    module.Start();
                }
            }
        }

        public MI Get<MI>() {
            if (_modules.TryGetValue(typeof(MI), out IModule module)) {
                return (MI)module;
            }
            return default(MI);
        }

        internal void Update() {
            foreach(var module in _moduleUpdateList)
                module.Update();
        }

        public void Uninstall<MI>() {
            if (_modules.Remove(typeof(MI), out IModule module))
                module.Stop();
        }
    }
}