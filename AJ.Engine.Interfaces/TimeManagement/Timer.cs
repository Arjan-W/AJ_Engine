using System;

namespace AJ.Engine.Interfaces.TimeManagement
{
    public class Timer
    {
        public bool HasElapsed => _hasElapsed;

        private readonly TimeSpan _interval;
        private TimeSpan _timer;
        private bool _repeat;
        private bool _hasElapsed;

        public Timer(TimeSpan interval, bool repeat = true) {
            _interval = interval;
            _timer = TimeSpan.Zero;
            _repeat = repeat;
            _hasElapsed = false;
        }

        public bool Update(IGameTime gameTime) {
            if (_hasElapsed && !_repeat)
                return _hasElapsed;

            _hasElapsed = false;
            _timer += gameTime.DeltaTime;

            if (_timer >= _interval) {
                _timer -= _interval;
                _hasElapsed = true;
            }

            return _hasElapsed;
        }
    }
}