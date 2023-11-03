using System;

namespace AJ.Engine.Interfaces.Services
{
    public interface IEngineServiceProvider
    {
        T Get<T>();
    }
}