namespace AJ.Core
{
    public class Core
    {
        private static Core _instance;

        public static void Run(Application application)
        {
            if (_instance == null)
            {
                _instance = new Core(application);
                _instance.Initialize();
                _instance.GameLoop();
                _instance.Deinitialize();
            }
        }

        public static void Stop()
        {
            Volatile.Write(ref _instance._isRunning, false);
        }

        private Application _application;
        private bool _isRunning;

        private Core(Application application)
        {
            _application = application;
            _isRunning = true;
        }

        private void Initialize()
        {
            _application.Initialize();
        }

        private void GameLoop()
        {
            while(_isRunning)
            {

            }
        }

        private void Deinitialize()
        {
            _application.Deinitialize();
        }
    }
}