using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Timer = AJ.Engine.Interfaces.TimeManagement.Timer;

namespace AJ.Logging
{
    internal class Logger : ILogger, IModule
    {
        private const byte CMD_SEND_MESSAGE = 253;
        private const byte CMD_FLUSH_BUFFER = 254;
        private const byte CMD_SHUTDOWN = 255;

        private const int BUFFER_SIZE = 65536;
        private const int FLUSH_TIMER_INTERVAL_MILLIS = 500;

        private const string MESSAGE_HEADER = "[{0}][{1}][xxxx/xx/xx xx:xx:xx]";
        private const string FILE_DATETIME = "yyyy-MM-dd-HH-mm-ss";

        private static readonly char[] NEWLINE = Environment.NewLine.ToCharArray();
        private static readonly int MESSAG_HEADER_SIZE = Marshal.SizeOf<MessageHeader>();

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

            bool hasMessages = messages.Any(m => !string.IsNullOrWhiteSpace(m));
            int titleSizeInBytes = CalculateStringSize(title);
            int allMessageSizeInBytes = 0;

            if (hasMessages) {
                foreach (var msg in messages) {
                    if (!string.IsNullOrEmpty(msg)) {
                        allMessageSizeInBytes += CalculateStringSize(msg, true);
                    }
                }
            }

            allMessageSizeInBytes += CalculateStringSize("", true);

            //add one for command byte
            Span<byte> dataPackage = stackalloc byte[1 + MESSAG_HEADER_SIZE + titleSizeInBytes + allMessageSizeInBytes];
            int offset = 0;

            dataPackage[offset++] = CMD_SEND_MESSAGE;
            SetMessageHeader(ref dataPackage, ref offset, logType, DateTime.Now, (short)titleSizeInBytes, (short)allMessageSizeInBytes);
            SetText(ref dataPackage, ref offset, title);

            if (hasMessages) {
                foreach (var msg in messages) {
                    if (!string.IsNullOrEmpty(msg)) {
                        SetText(ref dataPackage, ref offset, msg, true);
                    }
                }
            }

            SetText(ref dataPackage, ref offset, "", true);

            //write package
            _pipeClientIn.Write(dataPackage);
        }

        private int CalculateStringSize(string text, bool newLine = false) {
            return (text.Length + (newLine ? NEWLINE.Length : 0)) * 2;
        }

        private void SetMessageHeader(ref Span<byte> data, ref int offset, LogTypes logType, DateTime dateTime, short titleLength, short messageLength) {
            ref MessageHeader messageHeader = ref MemoryMarshal.Cast<byte, MessageHeader>(data.Slice(offset))[0];
            messageHeader.LogTypes = (byte)logType;
            messageHeader.DateTime = dateTime.ToBinary();
            messageHeader.TitleLength = titleLength;
            messageHeader.MessageLength = messageLength;
            offset += MESSAG_HEADER_SIZE;
        }

        private void SetText(ref Span<byte> data, ref int offset, string text, bool newLine = false) {
            Span<char> characters = MemoryMarshal.Cast<byte, char>(data.Slice(offset));
            text.CopyTo(characters);
            offset += text.Length * 2;
            if (newLine) {
                NEWLINE.CopyTo(characters.Slice(text.Length));
                offset += NEWLINE.Length * 2;
            }
        }

        private void LogLoop() {
            using (StreamWriter consoleWriter = CreateConsoleStreamWriter())
            using (StreamWriter fileWriter = CreateFileStreamWriter()) {
                try {
                    byte commandByte;
                    while (_isRunning) {
                        commandByte = (byte)_pipeServerOut.ReadByte();
                        switch (commandByte) {
                            case CMD_SEND_MESSAGE:
                                ReadMessage(consoleWriter, fileWriter);
                                break;
                            case CMD_FLUSH_BUFFER:
                                consoleWriter?.Flush();
                                fileWriter?.Flush();
                                break;
                            case CMD_SHUTDOWN:
                                _isRunning = false;
                                break;
                            default:
                                throw new DataMisalignedException("First byte must be a command byte!");
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
            var sw = new StreamWriter(Console.OpenStandardOutput(BUFFER_SIZE));
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

        private void ReadMessage(StreamWriter consoleWriter, StreamWriter fileWriter) {
            Span<MessageHeader> messageHeaderSpan = stackalloc MessageHeader[1];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(messageHeaderSpan));
            ref MessageHeader messageHeader = ref messageHeaderSpan[0];

            Span<byte> title = stackalloc byte[messageHeader.TitleLength];
            Span<byte> message = stackalloc byte[messageHeader.MessageLength];

            _pipeServerOut.Read(title);
            _pipeServerOut.Read(message);

            LogTypes logType = (LogTypes)messageHeader.LogTypes;
            int headerSize = TitleFormatter.GetLengthInBytes(logType, messageHeader.TitleLength);

            Span<byte> data = stackalloc byte[headerSize + messageHeader.MessageLength];

            ReadOnlySpan<char> titleAsChar = MemoryMarshal.Cast<byte, char>(title);

            Span<char> dataAsChar = MemoryMarshal.Cast<byte, char>(data);
            TitleFormatter.SetTitle(dataAsChar, logType, titleAsChar, DateTime.FromBinary(messageHeader.DateTime));
            message.CopyTo(data.Slice(headerSize));

            if (_application.LogToConsole.HasFlag(logType) && consoleWriter != null)
                consoleWriter.Write(dataAsChar);

            if (_application.LogToFile.HasFlag(logType) && fileWriter != null)
                fileWriter.Write(dataAsChar);
        }

        void IModule.Update() {
            if (_timer.Update(_gameTime))
                _pipeClientIn.WriteByte(CMD_FLUSH_BUFFER);
        }

        void IModule.Stop() {
            _pipeClientIn.WriteByte(CMD_SHUTDOWN);
            _logThread.Join();
            _pipeServerOut.Close();
            _pipeClientIn.Close();
        }
    }
}