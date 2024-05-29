using AJ.Engine.Interfaces.TimeManagement;
using System;

namespace AJ.Engine.TimeManagement
{
    internal class Timer : ITimer
    {
        private readonly IGameTime _gameTime;
        private readonly TimeSpan _interval;
        private readonly bool _repeat;

        private TimeSpan _lastIntervalCheck;
        private TimeSpan _elapsedTime;
        private bool _hasElapsed;

        internal Timer(IGameTime gameTime, TimeSpan interval, bool repeat = true) {
            _gameTime = gameTime;
            _interval = interval;
            _repeat = repeat;

            _lastIntervalCheck = TimeSpan.Zero;
            _hasElapsed = false;
        }

        public bool HasElapsed() {
            if (_hasElapsed && !_repeat) {
                return true;
            }
            else if (_lastIntervalCheck != _gameTime.DeltaTime) {
                _lastIntervalCheck = _gameTime.DeltaTime;
                _elapsedTime += _gameTime.DeltaTime;
                _hasElapsed = false;
                if (_elapsedTime >= _interval) {
                    _elapsedTime -= _interval;
                    _hasElapsed = true;
                }
            }

            return _hasElapsed;
        }

        public void Reset() {
            _elapsedTime = TimeSpan.Zero;
            _hasElapsed = false;
        }
    }
}
