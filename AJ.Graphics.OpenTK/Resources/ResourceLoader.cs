namespace AJ.Graphics.OpenTK.Resources
{
    internal interface IResourceLoader {
        T Load<T>(string path);
        void Unload<T>(string path);
    }

    internal class ResourceLoader<T> : IResourceLoader where T : Resource
    {
        public T1 Load<T1>(string path) {
            throw new System.NotImplementedException();
        }

        public void Unload<T1>(string path) {
            throw new System.NotImplementedException();
        }
    }
}
