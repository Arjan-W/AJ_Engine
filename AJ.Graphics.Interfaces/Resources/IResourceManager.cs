namespace AJ.Graphics.Interfaces.Resources
{
    public interface IResourceManager
    {
        T Load<T>(string resource);
        void Unload<T>(string resource);
    }
}