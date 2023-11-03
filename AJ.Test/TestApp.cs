using AJ.Engine;

namespace AJ.Test
{
    internal class TestApp : Application
    {
        public TestApp() : base("AJ test app")
        {

        }

        protected override void OnInitialize()
        {
            Window.OnCloseWindowRequest += () =>
            {                
                Logger.LogDebug("Window Event",
                    "Window close requested!");
                Core.Stop();
            };

            Window.OnResize += (newSize) =>
            {
                Logger.LogDebug("Window Event",
                   $"Window resized to {newSize}!");
            };

            Window.OnResizeFinished += (finalSize) =>
            {
                Logger.LogDebug("Window Event",
                   $"Window final resize to {finalSize}!");
            };

            Window.OnFocusChanged += (focused) =>
            {
                Logger.LogDebug("Window Event",
                   $"Window focused: {focused}!");
            };
        }

        protected override void OnDeinitialize()
        {

        }
    }
}
