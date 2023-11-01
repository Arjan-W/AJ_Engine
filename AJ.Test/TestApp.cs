using AJ.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJ.Test
{
    internal class TestApp : Application
    {
        public TestApp() : base("AJ test app")
        {

        }

        protected override void OnInitialize()
        {
            Core.Window.OnCloseWindowRequest += () =>
            {
                Core.Stop();
            };

            Core.Window.OnResize += (size) =>
            {
                Console.WriteLine("Resize " + size.ToString());
            };

            Core.Window.OnResizeFinished += (size) =>
            {
                Console.WriteLine("Resize finished " + size.ToString());
            };

            Core.Window.OnFocusChanged += (isFocused) =>
            {
                Console.WriteLine($"window focused: {isFocused}");
            };
        }

        protected override void OnDeinitialize()
        {

        }
    }
}
