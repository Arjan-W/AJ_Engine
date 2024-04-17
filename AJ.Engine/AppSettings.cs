using AJ.Logging.Interfaces;
using System;
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

        public TimeSpan? DeltaTimeConstraint {
            get => _deltaTimeConstraint;
            set => _deltaTimeConstraint = value;
        }

        public LogTypes LogToConsole {
            get => _logToConsole;
            set => _logToConsole = value;
        }

        public LogTypes LogToFile {
            get => _logToFile;
            set => _logToFile = value;
        }

        private string _title;
        private bool _enableGraphics;
        private bool _closeOnRequest;
        private TimeSpan? _deltaTimeConstraint;
        private LogTypes _logToConsole;
        private LogTypes _logToFile;

        public AppSettings() {
            _title = Assembly.GetExecutingAssembly()?.FullName ?? "defaultTitle";
            _enableGraphics = true;
            _closeOnRequest = false;
            _deltaTimeConstraint = null;
            _logToConsole = LogTypes.ALL;
            _logToFile = LogTypes.ALL;
        }
    }
}