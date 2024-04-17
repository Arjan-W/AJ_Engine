using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using System;
using System.Diagnostics;

namespace AJ.Engine.TimeManagement
{
    internal class GameTime : IGameTime, IModule
    {
        public TimeSpan ElapsedTime => _elapsedTime;
        public TimeSpan DeltaTime => _deltaTime;

        public float ElapsedTimeF => (float)_elapsedTime.TotalSeconds;
        public float DeltaTimeF => (float)_deltaTime.TotalSeconds;

        public double ElapsedTimeD => _elapsedTime.TotalSeconds;
        public double DeltaTimeD => _deltaTime.TotalSeconds;

        private readonly Stopwatch _stopwatch;
        private TimeSpan _deltaTime;
        private TimeSpan _elapsedTime;
        private readonly TimeSpan? _deltaTimeConstraint;

        internal GameTime(TimeSpan? deltaTimeConstraint = null)
        {
            _stopwatch = Stopwatch.StartNew();
            _deltaTime = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;
            _deltaTimeConstraint = deltaTimeConstraint;
        }

        public void Update()
        {
            TimeSpan elapsedTimeSpan = _stopwatch.Elapsed;
            _deltaTime = elapsedTimeSpan - _elapsedTime;
            _elapsedTime = elapsedTimeSpan;

            if (_deltaTimeConstraint.HasValue)
            {
                if (_deltaTime > _deltaTimeConstraint)
                    _deltaTime = _deltaTimeConstraint.Value;
            }
        }
    }
}