using AJ.Engine.SceneManagement;

namespace AJ.Engine.Interfaces.SceneManagement
{
    public interface ISceneManager
    {
        IScene CreateScene<S>(string name) where S : IScene;
        IScene AddScene<S>(S scene) where S : IScene;
        void RemoveScene(IScene scene);
        void RemoveScene(string name);
    }
}
