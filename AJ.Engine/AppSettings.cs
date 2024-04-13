using System.ComponentModel;
using System.Reflection;

namespace AJ.Engine
{
    public class AppSettings
    {
        public string Title {
            get => _title;
            set => _title = value;
        }

        public bool EnableGraphics {
            get => _enableGraphics;
            set => _enableGraphics = value;
        }

        public bool CloseOnRequest {
            get => _closeOnRequest && _enableGraphics;
            set => _closeOnRequest = value;
        }

        private string _title;
        private bool _enableGraphics;
        private bool _closeOnRequest;

        public AppSettings() {
            _title = Assembly.GetExecutingAssembly()?.FullName ?? "defaultTitle";
            _enableGraphics = true;
            _closeOnRequest = false;
        }
    }
}