namespace AJ.Graphics.OpenTK.Resources
{
    internal interface IResourceLoader {
        T Load<T>(string path);
        void Unload<T>(string path);
    }

    internal class ResourceLoader<T> : IResourceLoader where T : Resource
    {
    }
}
