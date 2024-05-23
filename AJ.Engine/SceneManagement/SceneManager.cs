using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.SceneManagement;
using AJ.Logging.Interfaces;
using System.Collections.Generic;

namespace AJ.Engine.SceneManagement
{
    internal class SceneManager : ISceneManager, IModule
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, Scene> _scenes;

        internal SceneManager() {
            _logger = Core.ModuleProvider.Get<ILogger>();
            _scenes = new Dictionary<string, Scene>();
        }

        public IScene CreateScene<S>(string name) where S : IScene { 
            if (_scenes.ContainsKey(name)) {
                _logger.LogError("SceneManager", $"Scenename {name} already exists!");
            }
            return AddScene(new DefaultScene(name, Core.ModuleProvider));
        }

        public IScene AddScene<S>(S scene) where S : IScene {
            var sceneType = typeof(S);
            if (sceneType.BaseType != typeof(Scene) || scene == null) {
                _logger.LogError("SceneManager",
                    $"Cannot add scene because it doesnt implement the scene class!");
                return default(S);
            }
            else if (_scenes.ContainsKey(scene.Name))
                return _scenes[scene.Name];
            else {
                _scenes.Add(scene.Name, scene as Scene);
                return scene;
            }
        }

        public void RemoveScene(IScene scene) {
            RemoveScene(scene?.Name);
        }

        public void RemoveScene(string sceneName) {
            if(_scenes.Remove(sceneName, out var _scene)) {
                _scene.Deinitialize();
            } else {
                _logger.LogWarning("SceneManager", $"Tried to remove {sceneName} but no such Scene exists.");
            }
        }
    }
}