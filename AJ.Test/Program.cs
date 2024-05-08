using AJ.Engine;
using System;

namespace AJ.Test
{
    internal class Program
    {
        static void Main(string[] args) {
            AppSettings appSettings = new AppSettings
            {
                Title = "TestApp",
                CloseOnRequest = true
            };
            Core.Run(new TestApp(appSettings));
        }
    }
}
