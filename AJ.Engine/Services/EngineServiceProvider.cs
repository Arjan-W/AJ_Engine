using AJ.Engine.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AJ.Engine.Services
{
    public class EngineServiceProvider : IEngineServiceProvider
    {
        private Dictionary<Type, IEngineService> _services;

        internal EngineServiceProvider()
        {
            _services = new Dictionary<Type, IEngineService>();
        }

        internal T Add<X, T>(T service) where T : class, IEngineService
        {
            if(typeof(T).GetInterfaces().Contains(typeof(X)))
            {
                var type = typeof(X);
                if (!_services.ContainsKey(type))
                {
                    service.Start();
                    _services.Add(typeof(X), service);
                }

                return service;
            }

            return null;
        }

        public T Get<T>()
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
                return (T)_services[type];
            throw new Exception($"Service of type {type} not found!");
        }

        internal void Dispose<T>()
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                var service = _services[type];
                service.Dispose();
                _services.Remove(type);
            }
        }
    }
}