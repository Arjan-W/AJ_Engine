using AJ.Logging.Interfaces;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AJ.Logging
{
    internal class LoggingBufferUtils
    {
        private const string STR_INFO = "Info";
        private const string STR_WARNING = "Warning";
        private const string STR_ERROR = "Error";
        private const string STR_DEBUG = "Debug";

        internal struct BufferOffsetCounter
        {
            private int _offset;

            public BufferOffsetCounter()
            {
                _offset = 0;
            }

            public void WriteByte(ref Span<byte> buffer, byte data)
            {
                buffer[_offset] = data;
                _offset += 1;
            }

            public void WriteInt(ref Span<byte> buffer, int data)
            {
                MemoryMarshal.Cast<byte, int>(buffer.Slice(_offset))[0] = data;
                _offset += 4;
            }

            public void WriteLong(ref Span<byte> buffer, long data)
            {
                MemoryMarshal.Cast<byte, long>(buffer.Slice(_offset))[0] = data;
                _offset += 8;
            }

            public void WriteString(ref Span<byte> buffer, ReadOnlySpan<char> data)
            {
                var byteData = MemoryMarshal.AsBytes(data);
                WriteInt(ref buffer, byteData.Length);
                byteData.CopyTo(buffer.Slice(_offset));
                _offset += byteData.Length;
            }
        }

        public static string GetLogTypeString(LogTypes logType)
        {
            switch (logType)
            {
                case LogTypes.INFO:
                    return STR_INFO;
                case LogTypes.WARNING:
                    return STR_WARNING;
                case LogTypes.ERROR:
                    return STR_ERROR;
                default:
                case LogTypes.DEBUG:
                    return STR_DEBUG;
            }
        }

        public static int CalculateNeededBytesForString(ReadOnlySpan<char> data)
        {
            return MemoryMarshal.AsBytes(data).Length + 4;
        }

        public static int ReadInt(Stream stream)
        {
            Span<int> data = stackalloc int[1];
            stream.Read(MemoryMarshal.AsBytes(data));
            return data[0];
        }

        public static long ReadLong(Stream stream)
        {
            Span<long> data = stackalloc long[1];
            stream.Read(MemoryMarshal.AsBytes(data));
            return data[0];
        }

        public static void ReadString(Stream stream, StringBuilder stringBuffer, bool appendLine = true)
        {
            Span<byte> data = stackalloc byte[ReadInt(stream)];
            stream.Read(data);

            if (appendLine)
                stringBuffer.AppendLine(MemoryMarshal.Cast<byte, char>(data).ToString());//alloc
            else
                stringBuffer.Append(MemoryMarshal.Cast<byte, char>(data).ToString());//alloc
        }

        public static void WriteStringBufferToStream(StringBuilder stringBuffer, BinaryWriter stream)
        {
            Span<char> data = stackalloc char[stringBuffer.Length];
            stringBuffer.CopyTo(0, data, data.Length);
            ReadOnlySpan<byte> readOnlyData = MemoryMarshal.Cast<char, byte>(data);
            stream.Write(readOnlyData);
        }
        public static void WriteStringBufferToStream(StringBuilder stringBuffer, StreamWriter stream) {
            Span<char> data = stackalloc char[stringBuffer.Length];
            stringBuffer.CopyTo(0, data, data.Length);
            stream.Write(data);
        }

    }
}