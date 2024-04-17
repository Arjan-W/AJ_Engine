using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using static AJ.Logging.LoggingBufferUtils;

namespace AJ.Logging
{
    internal class Logger : ILogger, IModule
    {
        private const int MESSAGE_DATA_DEFAULT_SIZE = 1 + 8 + 4;
        private const string MESSAGE_HEADER = "[{0}][{1}][{2:ddd HH:mm:ss}]";
        private const string FILE_DATETIME = "yyyy-MM-dd-HH-mm-ss";

        private readonly IApplication _application;
        private readonly LogTypes _logFilter;

        private readonly AnonymousPipeServerStream _pipeServerOut;
        private readonly AnonymousPipeClientStream _pipeClientIn;
        private readonly string _filePath;

        private readonly StringBuilder _titleBuffer;
        private readonly StringBuilder _titleTimeStampBuffer;
        private readonly StringBuilder _messageBuffer;

        private Thread _logThread;
        private bool _isRunning;

        internal Logger(IApplication application) {
            _application = application;
            _logFilter = application.LogToConsole | application.LogToFile;

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
            using (StreamWriter consoleWriter = new StreamWriter(Console.OpenStandardOutput()))
            using (StreamWriter fileWriter = CreateStreamWriter()) {
                consoleWriter.AutoFlush = true;
                byte commandByte;
                while (_isRunning) {


                    commandByte = (byte)_pipeServerOut.ReadByte();
                    switch (commandByte) {
                        default:
                            ReadMessage((LogTypes)commandByte, fileWriter, consoleWriter);
                            break;
                        case 255:
                            _isRunning = false;
                            break;
                    }
                }
            }
        }

        private StreamWriter CreateStreamWriter() {
            if (_application.LogToFile == LogTypes.NONE)
                return null;
            return new StreamWriter(File.OpenWrite(_filePath));
        }

        private void ReadMessage(LogTypes logType, StreamWriter fileWriter, StreamWriter consoleWriter) {
            DateTime dateTime = DateTime.FromBinary(ReadLong(_pipeServerOut));
            int numOfStrings = ReadInt(_pipeServerOut);

            _titleBuffer.Clear();
            _titleTimeStampBuffer.Clear();
            _messageBuffer.Clear();

            ReadString(_pipeServerOut, _titleBuffer, false);
            CreateTitle(logType, dateTime);

            for (int i = 0; i < numOfStrings; i++) {
                ReadString(_pipeServerOut, _messageBuffer);
            }

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
                readOnlyData.ToString(),
                dateTime);
        }

        private void WriteToConsole(LogTypes logType, StreamWriter consoleWriter) {
            switch (logType) {
                case LogTypes.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogTypes.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogTypes.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogTypes.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }
            WriteStringBufferToStream(_titleTimeStampBuffer, consoleWriter);
            Console.ForegroundColor = ConsoleColor.White;
            WriteStringBufferToStream(_messageBuffer, consoleWriter);
        }

        private void WriteToFile(StreamWriter sw) {
            WriteStringBufferToStream(_titleTimeStampBuffer, sw);
            WriteStringBufferToStream(_messageBuffer, sw);
        }

        private void WriteEndByte() {
            _pipeClientIn.WriteByte(255);
        }

        void IModule.Stop() {
            WriteEndByte();
            _logThread.Join();
            _pipeServerOut.Close();
            _pipeClientIn.Close();
        }
    }
}