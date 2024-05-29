using System;

namespace AJ.Engine.Interfaces.TimeManagement
{
    public interface ITimer
    {
        bool HasElapsed();
        void Reset();
    }
}