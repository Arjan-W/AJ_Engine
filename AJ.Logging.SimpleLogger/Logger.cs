using AJ.Engine.Interfaces;
using AJ.Engine.Interfaces.Services;
using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AJ.Logging.SimpleLogger
{
    public class Logger : IEngineService, ILogger
    {
        private const int MESSAGE_DATA_DEFAULT_SIZE = 1 + 8 + 4;
        private const string MESSAGE_HEADER = "[{0}][{1}][{2}]";
        private const string MESSAGE_DATETIME = "ddd HH:mm:ss";
        private const string FILE_DATETIME = "yyyy-MM-dd-HH-mm-ss";

        private readonly LogTypes _logToConsole;
        private readonly LogTypes _logToFile;
        private readonly LogTypes _logFilter;

        private readonly AnonymousPipeServerStream _pipeServerOut;
        private readonly AnonymousPipeClientStream _pipeClientIn;

        private readonly string _filePath;

        private readonly StringBuilder _titleBuffer;
        private readonly StringBuilder _messageBuffer;

        private Thread _thread;
        private bool _isRunning;

        public Logger(IApplication application)
        {
            _logToConsole = application.LogToConsole;
            _logToFile = application.LogToFile;
            _logFilter = _logToConsole | _logToFile;

            _pipeServerOut = new AnonymousPipeServerStream(PipeDirection.In);
            _pipeClientIn = new AnonymousPipeClientStream(PipeDirection.Out, _pipeServerOut.ClientSafePipeHandle);

            string directory = string.Join(Path.DirectorySeparatorChar, Directory.GetCurrentDirectory(), "Logs");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string file = $"Log_{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now.ToString(FILE_DATETIME)}.log";
            _filePath = string.Join(Path.DirectorySeparatorChar, directory, file);

            if (!File.Exists(_filePath))
                File.Create(_filePath).Close();

            _titleBuffer = new StringBuilder();
            _messageBuffer = new StringBuilder();

            _isRunning = true;
        }

        void IEngineService.Start()
        {
            using (var are = new AutoResetEvent(false))
            {
                _thread = new Thread(() =>
                {
                    are.Set();
                    LogLoop();
                });
                _thread.Start();
                are.WaitOne();
            }
        }

        public void LogInfo(string title, params string[] messages)
        {
            WriteMessage(LogTypes.Info, title, messages);
        }

        public void LogWarning(string title, params string[] messages)
        {
            WriteMessage(LogTypes.Warning, title, messages);
        }
        public void LogError(string title, params string[] messages)
        {
            WriteMessage(LogTypes.Error, title, messages);
        }

        public void LogFatal(string title, params string[] messages)
        {
            WriteMessage(LogTypes.Fatal, title, messages);
        }

        public void LogDebug(string title, params string[] messages)
        {
            WriteMessage(LogTypes.Debug, title, messages);
        }

        private void WriteMessage(LogTypes logType, string title, string[] messages)
        {
            if (!_logFilter.HasFlag(logType))
                return;

            if (string.IsNullOrWhiteSpace(title))
                return;

            string messageTitle = string.Format(MESSAGE_HEADER, logType, title, DateTime.Now.ToString(MESSAGE_DATETIME));

            int dataPackageSize = MESSAGE_DATA_DEFAULT_SIZE;
            int allowableStrings = 0;

            dataPackageSize += BufferUtil.CalculateNeededBytesForString(title.AsSpan());
            foreach (var message in messages)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    dataPackageSize += BufferUtil.CalculateNeededBytesForString(message.AsSpan());
                    allowableStrings++;
                }
            }

            //allocate data on stack
            Span<byte> dataPackage = stackalloc byte[dataPackageSize];

            BufferUtil bufferUtil = new BufferUtil();
            bufferUtil.WriteByte(ref dataPackage, (byte)logType);
            bufferUtil.WriteLong(ref dataPackage, DateTime.Now.ToBinary());
            bufferUtil.WriteInt(ref dataPackage, allowableStrings);
            bufferUtil.WriteString(ref dataPackage, title.AsSpan());
            foreach (var message in messages)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    bufferUtil.WriteString(ref dataPackage, message.AsSpan());
                }
            }

            //write package
            _pipeClientIn.Write(dataPackage);
        }



        private void LogLoop()
        {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(_filePath)))
            {
                bool endByteRecieved = false;
                byte commandByte;
                while (_isRunning || !endByteRecieved)
                {
                    commandByte = (byte)_pipeServerOut.ReadByte();
                    switch (commandByte)
                    {
                        default:
                            ReadMessage((LogTypes)commandByte, sw);
                            break;
                        case 255:
                            endByteRecieved = true;
                            break;
                    }
                }
            }
        }

        private void ReadMessage(LogTypes logType, StreamWriter sw)
        {
            DateTime dateTime = DateTime.FromBinary(ReadLong());
            int numOfStrings = ReadInt();
            _titleBuffer.Clear();
            _messageBuffer.Clear();
            Readtitle(logType, dateTime);
            for (int i = 0; i < numOfStrings; i++)
            {
                ReadString();
            }

            if (_logToConsole.HasFlag(logType))
                WriteToConsole(logType);

            if (_logToFile.HasFlag(logType))
                WriteToFile(sw);
        }

        private void WriteToConsole(LogTypes logType)
        {
            switch(logType)
            {
                case LogTypes.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogTypes.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogTypes.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogTypes.Fatal:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogTypes.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }
            Console.WriteLine(_titleBuffer.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(_messageBuffer.ToString());
        }

        private void WriteToFile(StreamWriter sw)
        {
            sw.WriteLine(_titleBuffer.ToString());
            sw.WriteLine(_messageBuffer.ToString());
        }

        private int ReadInt()
        {
            Span<int> data = stackalloc int[1];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(data));
            return data[0];
        }

        private long ReadLong()
        {
            Span<long> data = stackalloc long[1];
            _pipeServerOut.Read(MemoryMarshal.AsBytes(data));
            return data[0];
        }
        private void Readtitle(LogTypes type, DateTime dateTime)
        {
            Span<byte> data = stackalloc byte[ReadInt()];
            _pipeServerOut.Read(data);
            _titleBuffer.AppendFormat(
                MESSAGE_HEADER,
                type.ToString(),
                MemoryMarshal.Cast<byte, char>(data).ToString(),
                dateTime.ToString(MESSAGE_DATETIME)
                );
        }

        private void ReadString()
        {
            Span<byte> data = stackalloc byte[ReadInt()];
            _pipeServerOut.Read(data);
            _messageBuffer.AppendLine(MemoryMarshal.Cast<byte, char>(data).ToString());
        }

        private void WriteEndByte()
        {
            _pipeClientIn.WriteByte(255);
        }

        public void Dispose()
        {
            WriteEndByte();
            Volatile.Write(ref _isRunning, false);
            _thread.Join();
            _pipeServerOut.Close();
            _pipeClientIn.Close();
        }
    }
}