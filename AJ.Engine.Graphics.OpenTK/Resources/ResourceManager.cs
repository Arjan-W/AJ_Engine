using System.Collections.Concurrent;

namespace AJ.Engine.Graphics.OpenTK.Resources
{
    internal class ResourceManager
    {
        private ConcurrentQueue<Resource> _resourcesQueuedForInit;

        internal ResourceManager()
        {
            _resourcesQueuedForInit = new ConcurrentQueue<Resource>();
        }

        internal void QueueForInitialization(Resource resource)
        {
            _resourcesQueuedForInit.Enqueue(resource);
        }

        internal void Update()
        {
            int resourceCount = _resourcesQueuedForInit.Count;
            for (int i = 0; i < resourceCount; i++)
            {
                if (_resourcesQueuedForInit.TryDequeue(out var resource))
                    resource.Initialize();
            }
        }
    }
}
