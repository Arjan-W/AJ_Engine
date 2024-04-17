using System;

namespace AJ.Engine.Interfaces.TimeManagement
{
    public interface IGameTime {
        public TimeSpan ElapsedTime { get; }
        public TimeSpan DeltaTime { get; }

        public float ElapsedTimeF { get; }
        public float DeltaTimeF { get; }

        public double ElapsedTimeD { get; }
        public double DeltaTimeD { get; }
    }
}