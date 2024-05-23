namespace AJ.Engine.SceneManagement
{
    public interface IScene
    {
        public string Name { get; }
        public bool IsLoaded { get; }
        public bool IsInit {  get; }
        public bool IsReady { get; }
        public bool IsRemoved { get; }

    }
}