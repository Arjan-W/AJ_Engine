using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using static AJ.Logging.LoggingBufferUtils;
using Timer = AJ.Engine.Interfaces.TimeManagement.Timer;

namespace AJ.Logging
{
    internal class Logger : ILogger, IModule
    {
        private const byte FLUSH_COMMAND = 128;
        private const byte SHUTDOWN_COMMAND = 255;
        private const int BUFFER_SIZE = 65536;
        private const int FLUSH_TIMER_INTERVAL_MILLIS = 500;
        private const int MESSAGE_DATA_DEFAULT_SIZE = 1 + 8 + 4;

        private const string MESSAGE_HEADER = "[{0}][{1}][{2:ddd HH:mm:ss}]";
        private const string FILE_DATETIME = "yyyy-MM-dd-HH-mm-ss";

        private readonly IApplication _application;
        private readonly IGameTime _gameTime;
        private readonly LogTypes _logFilter;

        private readonly Timer _timer;
        private readonly AnonymousPipeServerStream _pipeServerOut;
        private readonly AnonymousPipeClientStream _pipeClientIn;
        private readonly string _filePath;

        private readonly StringBuilder _titleBuffer;
        private readonly StringBuilder _titleTimeStampBuffer;
        private readonly StringBuilder _messageBuffer;

        private Thread _logThread;
        private bool _isRunning;

        internal Logger(IModuleProvider moduleProvider, IApplication application) {
            _application = application;
            _gameTime = moduleProvider.Get<IGameTime>();
            _logFilter = application.LogToConsole | application.LogToFile;

            _timer = new Timer(TimeSpan.FromMilliseconds(FLUSH_TIMER_INTERVAL_MILLIS));
            _pipeServerOut = new AnonymousPipeServerStream(PipeDirection.In);
            _pipeClientIn = new AnonymousPipeClientStream(PipeDirection.Out, _pipeServerOut.ClientSafePipeHandle);

            _filePath = CreateLogFile();

            _titleTimeStampBuffer = new StringBuilder();
            _titleBuffer = new StringBuilder();
            _messageBuffer = new StringBuilder();

            _isRunning = true;
        }

        private string CreateLogFile() {
            if (_application.LogToFile == LogTypes.NONE)
                return null;

            string directory = string.Join(Path.DirectorySeparatorChar, Directory.GetCurrentDirectory(), "Logs");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string file = $"Log_{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now.ToString(FILE_DATETIME)}.log";
            string filePath = string.Join(Path.DirectorySeparatorChar, directory, file);

            if (!File.Exists(filePath))
                File.Create(filePath).Close();

            return filePath;
        }

        void IModule.Start() {
            using AutoResetEvent are = new AutoResetEvent(false);
            _logThread = new Thread(() => {
                are.Set();
                LogLoop();
            });
            _logThread.Start();
            are.WaitOne();
        }

        public void LogInfo(string title, params string[] messages) {
            WriteMessage(LogTypes.INFO, title, messages);
        }

        public void LogWarning(string title, params string[] messages) {
            WriteMessage(LogTypes.WARNING, title, messages);
        }

        public void LogError(string title, params string[] messages) {
            WriteMessage(LogTypes.ERROR, title, messages);
        }

        public void LogDebug(string title, params string[] messages) {
            WriteMessage(LogTypes.DEBUG, title, messages);
        }

        private void WriteMessage(LogTypes logType, string title, string[] messages) {
            if (!_logFilter.HasFlag(logType))
                return;

            if (string.IsNullOrWhiteSpace(title))
                return;

            int dataPackageSize = MESSAGE_DATA_DEFAULT_SIZE;
            int allowableStrings = 0;

            dataPackageSize += CalculateNeededBytesForString(title.AsSpan());
            foreach (var message in messages) {
                if (!string.IsNullOrWhiteSpace(message)) {
                    dataPackageSize += CalculateNeededBytesForString(message.AsSpan());
                    allowableStrings++;
                }
            }

            //allocate data on stack
            Span<byte> dataPackage = stackalloc byte[dataPackageSize];

            BufferOffsetCounter bufferOffsetCounter = new BufferOffsetCounter();
            bufferOffsetCounter.WriteByte(ref dataPackage, (byte)logType);
            bufferOffsetCounter.WriteLong(ref dataPackage, DateTime.Now.ToBinary());
            bufferOffsetCounter.WriteInt(ref dataPackage, allowableStrings);
            bufferOffsetCounter.WriteString(ref dataPackage, title.AsSpan());

            foreach (var message in messages) {
                if (!string.IsNullOrWhiteSpace(message)) {
                    bufferOffsetCounter.WriteString(ref dataPackage, message.AsSpan());
                }
            }

            //write package
            _pipeClientIn.Write(dataPackage);
        }

        private void LogLoop() {
            using (StreamWriter consoleWriter = CreateConsoleStreamWriter())
            using (StreamWriter fileWriter = CreateFileStreamWriter()) {
                try {
                    byte commandByte;
                    while (_isRunning) {
                        commandByte = (byte)_pipeServerOut.ReadByte();
                        switch (commandByte) {
                            case SHUTDOWN_COMMAND:
                                _isRunning = false;
                                break;
                            case FLUSH_COMMAND:
                                    consoleWriter?.Flush();
                                    fileWriter?.Flush();
                                break;
                            default:
                                ReadMessage((LogTypes)commandByte, fileWriter, consoleWriter);
                                break;
                        }
                    }
                }
                finally {
                    consoleWriter?.Flush();
                    fileWriter?.Flush();
                }
            }
        }
        private StreamWriter CreateConsoleStreamWriter() {
            if (_application.LogToConsole == LogTypes.NONE)
                return null;
            var sw = new StreamWriter(Console.OpenStandardOutput(BUFFER_SIZE), null, BUFFER_SIZE, false);
            sw.AutoFlush = false;
            return sw;
        }

        private StreamWriter CreateFileStreamWriter() {
            if (_application.LogToFile == LogTypes.NONE)
                return null;
            var sw = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write), Encoding.Unicode, BUFFER_SIZE, false);
            sw.AutoFlush = false;
            return sw;
        }


        private void ReadMessage(LogTypes logType, StreamWriter fileWriter, StreamWriter consoleWriter) {
            DateTime dateTime = DateTime.FromBinary(ReadLong(_pipeServerOut));
            int numOfStrings = ReadInt(_pipeServerOut);

            _titleBuffer.Clear();
            _titleTimeStampBuffer.Clear();
            _messageBuffer.Clear();

            ReadString(_pipeServerOut, _titleBuffer, false);
            CreateTitle(logType, dateTime);
            _titleTimeStampBuffer.AppendLine();

            for (int i = 0; i < numOfStrings; i++) {
                ReadString(_pipeServerOut, _messageBuffer);
            }

            _messageBuffer.AppendLine();

            if (_application.LogToConsole.HasFlag(logType))
                WriteToConsole(logType, consoleWriter);

            if (_application.LogToFile.HasFlag(logType) && fileWriter != null)
                WriteToFile(fileWriter);
        }

        private void CreateTitle(LogTypes logType, DateTime dateTime) {
            Span<char> data = stackalloc char[_titleBuffer.Length];
            _titleBuffer.CopyTo(0, data, data.Length);
            ReadOnlySpan<char> readOnlyData = data;

            _titleTimeStampBuffer.AppendFormat(MESSAGE_HEADER,
                GetLogTypeString(logType),
                readOnlyData.ToString(),//alloc
                dateTime);
        }

        private void AppendMessage() {

        }

        private void WriteToConsole(LogTypes logType, StreamWriter consoleWriter) {
            WriteStringBufferToStream(_titleTimeStampBuffer, consoleWriter);
            WriteStringBufferToStream(_messageBuffer, consoleWriter);
        }

        private void WriteToFile(StreamWriter sw) {
            WriteStringBufferToStream(_titleTimeStampBuffer, sw);
            WriteStringBufferToStream(_messageBuffer, sw);
        }

        private void WriteEndByte() {
            _pipeClientIn.WriteByte(SHUTDOWN_COMMAND);
        }

        void IModule.Update() {
            if (_timer.Update(_gameTime))
                _pipeClientIn.WriteByte(FLUSH_COMMAND);
        }

        void IModule.Stop() {
            WriteEndByte();
            _logThread.Join();
            _pipeServerOut.Close();
            _pipeClientIn.Close();
        }
    }
}