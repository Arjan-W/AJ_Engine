using System;
using System.Runtime.InteropServices;

namespace AJ.Logging.SimpleLogger
{
    public struct BufferUtil
    {
        private int _offset;

        public BufferUtil()
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

        public static int CalculateNeededBytesForString(ReadOnlySpan<char> data)
        {
            return MemoryMarshal.AsBytes(data).Length + 4;
        }

    }
}
