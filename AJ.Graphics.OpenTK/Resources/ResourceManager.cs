using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Graphics.Interfaces.Resources;
using AJ.Logging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AJ.Graphics.OpenTK.Resources
{
    internal class ResourceManager : IResourceManager
    {
        private readonly ILogger _logger;
        private Dictionary<Type, IResourceLoader> _resourceLoaders;
        private ConcurrentQueue<Resource> _resourcesQueuedForInit;

        internal ResourceManager(IModuleProvider _moduleProvider) {
            _logger = GraphicsContext.InternalLogger;
            _resourceLoaders = new Dictionary<Type, IResourceLoader>();
            _resourcesQueuedForInit = new ConcurrentQueue<Resource>();
        }

        internal void AddResourceLoader<T>() where T : Resource{
            if(!_resourceLoaders.ContainsKey(typeof(T))) {
                _resourceLoaders.Add(typeof(T), new ResourceLoader<T>());
            }
        }

        public T Load<T>(string resource) {
            if (_resourceLoaders.TryGetValue(typeof(T), out var rescLoader)) 
                return rescLoader.Load<T>(resource);
                    else
                return default(T);
        }

        public void Unload<T>(string resource) {
            if (_resourceLoaders.TryGetValue(typeof(T), out var rescLoader))
                rescLoader.Unload<T>(resource);
        }

        internal void QueueForInit(Resource resource) {
            _resourcesQueuedForInit.Enqueue(resource);
        }

        internal void Update() {
            int numOfResources = _resourcesQueuedForInit.Count;
            for(int i = 0; i < numOfResources; i++) {
                if(_resourcesQueuedForInit.TryDequeue(out var res)){
                    res.InitResource(_logger);
                }
            }
        }
    }
}