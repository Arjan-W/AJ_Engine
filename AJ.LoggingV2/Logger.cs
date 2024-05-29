using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.Interfaces.Util.Strings;
using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ITimer = AJ.Engine.Interfaces.TimeManagement.ITimer;

namespace AJ.LoggingV2
{
    internal class Logger : ILogger, IModule
    {
        private const string FILE_DATETIME = "yyyy-MM-dd-HH-mm-ss";

        private const byte CMD_SEND_MESSAGE = 253;
        private const byte CMD_FLUSH = 254;
        private const byte CMD_SHUTDOWN = 255;

        private const int MESSAGE_MAX_LENGTH = 600;

        private readonly int MESSASGE_DATA_SIZE;

        private const int BUFFER_SIZE = 65536;
        private const int BUFFER_FLUSH_TIME_MS = 166;

        private readonly IApplication _application;
        private readonly ITimer _flushTimer;
        private readonly LogTypes _logFilter;
        private readonly AnonymousPipeServerStream _pipeServerOut;
        private readonly AnonymousPipeClientStream _pipeClientIn;

        private readonly string _filePath;
        private bool _hasTimerElapsed;

        private Thread _logThread;
        private bool _isLogging;

        internal Logger(IApplication application, IGameTime gameTime)
        {
            MESSASGE_DATA_SIZE = Marshal.SizeOf<MessageData>();

            _application = application;
            _flushTimer = gameTime.CreateTimer(TimeSpan.FromMilliseconds(BUFFER_FLUSH_TIME_MS));
            _logFilter = application.LogToFile | application.LogToConsole;
            _pipeServerOut = new AnonymousPipeServerStream(PipeDirection.In);
            _pipeClientIn = new AnonymousPipeClientStream(PipeDirection.Out, _pipeServerOut.ClientSafePipeHandle);

            _filePath = CreateLogFile();
            _hasTimerElapsed = false;

            _isLogging = true;
        }

        private string CreateLogFile()
        {
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

        void IModule.Start()
        {
            using (var are = new AutoResetEvent(false))
            {
                _logThread = new Thread(() =>
                {
                    are.Set();
                    LogLoop();
                });
                _logThread.Start();
                are.WaitOne();
            }
        }

        private void LogLoop()
        {
            using (StreamWriter consoleWriter = CreateConsoleStreamWriter())
            using (StreamWriter fileWriter = CreateFileStreamWriter())
            {
                try
                {
                    while (_isLogging)
                    {
                        switch (_pipeServerOut.ReadByte())
                        {
                            case CMD_SEND_MESSAGE:
                                ReadMessage(consoleWriter, fileWriter);
                                break;
                            case CMD_FLUSH:
                                consoleWriter?.Flush();
                                fileWriter?.Flush();
                                break;
                            case CMD_SHUTDOWN:
                                _isLogging = false;
                                break;
                        }
                    }
                }
                finally
                {
                    consoleWriter?.Flush();
                    fileWriter?.Flush();
                }
            }
        }

        private StreamWriter CreateConsoleStreamWriter()
        {
            if (_application.LogToConsole == LogTypes.NONE)
                return null;
            var sw = new StreamWriter(Console.OpenStandardOutput(BUFFER_SIZE));
            sw.AutoFlush = false;
            return sw;
        }

        private StreamWriter CreateFileStreamWriter()
        {
            if (_application.LogToFile == LogTypes.NONE)
                return null;
            var sw = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write), Encoding.Unicode, BUFFER_SIZE, false);
            sw.AutoFlush = false;
            return sw;
        }

        private void ReadMessage(StreamWriter consoleWriter, StreamWriter fileWriter)
        {
            Span<MessageData> logData = stackalloc MessageData[1];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(logData));

            Span<char> titleChars = stackalloc char[logData[0].TitleLength / 2];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(titleChars));

            Span<char> messageChars = stackalloc char[logData[0].MessageLength / 2];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(messageChars));

            LogTypes logType = (LogTypes)logData[0].LogType;
            int titleHeaderLength = TitleFormatter.GetLengthInBytes(logType, logData[0].TitleLength);

            Span<char> completeMessage = stackalloc char[titleHeaderLength + messageChars.Length + messageChars.Length];
            TitleFormatter.SetTitle(completeMessage, logType, titleChars, DateTime.FromBinary(logData[0].DateTime));
            messageChars.CopyTo(completeMessage.Slice(titleHeaderLength));
            NewLine.CopyTo(completeMessage.Slice(titleHeaderLength + messageChars.Length));

            if (_application.LogToConsole.HasFlag(logType))
                consoleWriter?.Write(completeMessage);

            if (_application.LogToFile.HasFlag(logType))
                fileWriter?.Write(completeMessage);
        }

        public void LogInfo(string title, string message)
        {
            WriteMessage(LogTypes.INFO, title, message);
        }

        public void LogWarning(string title, string message)
        {
            WriteMessage(LogTypes.WARNING, title, message);
        }

        public void LogError(string title, string message)
        {
            WriteMessage(LogTypes.ERROR, title, message);
        }

        public void LogFatal(string title, string message)
        {
            WriteMessage(LogTypes.FATAL, title, message);
        }

        public void LogDebug(string title, string message)
        {
            WriteMessage(LogTypes.DEBUG, title, message);
        }

        private void WriteMessage(LogTypes logType, string title, string message)
        {
            if (!_logFilter.HasFlag(logType))
                return;

            if (string.IsNullOrEmpty(title))
                return;

            int titleSizeInBytes = title.Length * 2;
            int messageSizeInBytes = (2 + message?.Length ?? 0) * 2;

            MessageData logData = new MessageData();
            logData.LogType = (byte)logType;
            logData.DateTime = DateTime.Now.ToBinary();
            logData.TitleLength = (ushort)titleSizeInBytes;
            logData.MessageLength = (ushort)messageSizeInBytes;

            int offset = 0;
            Span<byte> data = stackalloc byte[1 + MESSASGE_DATA_SIZE + titleSizeInBytes + messageSizeInBytes];

            SetCommandByte(ref data, ref offset, CMD_SEND_MESSAGE);
            SetMessageData(ref data, ref offset, ref logData);
            SetTextData(ref data, ref offset, title);
            SetTextData(ref data, ref offset, message, true);

            _pipeClientIn.Write(data);
            _flushTimer.Reset();
            _hasTimerElapsed = false;
        }

        private void SetCommandByte(ref Span<byte> data, ref int offset, byte command)
        {
            data[offset++] = command;
        }

        private void SetMessageData(ref Span<byte> data, ref int offset, ref MessageData messageData)
        {
            ref MessageData messageDataBuffer = ref MemoryMarshal.Cast<byte, MessageData>(data.Slice(offset))[0];
            messageDataBuffer = messageData;
            offset += MESSASGE_DATA_SIZE;
        }

        private void SetTextData(ref Span<byte> data, ref int offset, string text, bool newLine = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            Span<char> textBuffer = MemoryMarshal.Cast<byte, char>(data.Slice(offset));
            text.CopyTo(textBuffer);
            offset += text.Length * 2;

            if (newLine)
            {
                NewLine.CopyTo(textBuffer.Slice(text.Length));
                offset += NewLine.Length * 2;
            }
        }

        void IModule.Update()
        {
            if (_flushTimer.HasElapsed() && !_hasTimerElapsed)
            {
                _pipeClientIn.WriteByte(CMD_FLUSH);
                _hasTimerElapsed = true;
            }
        }

        void IModule.Stop()
        {
            _pipeClientIn.WriteByte(CMD_FLUSH);
            _pipeClientIn.WriteByte(CMD_SHUTDOWN);
            _logThread.Join();
            _pipeClientIn.Dispose();
            _pipeServerOut.Dispose();
        }
    }
}