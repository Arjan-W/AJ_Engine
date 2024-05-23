using AJ.Engine.Interfaces.ModuleManagement;

namespace AJ.Engine.SceneManagement
{
    internal class DefaultScene : Scene, IScene
    {

        private string _name;

        public DefaultScene(string name, IModuleProvider moduleProvider) : base(name, moduleProvider) {
            _name = name; //Chech if name is unique with other scenes
        }


    }
}
