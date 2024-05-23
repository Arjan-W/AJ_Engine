using System.Runtime.InteropServices;

namespace AJ.Logging
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MessageHeader
    {
        public byte LogTypes { get; set; }
        public long DateTime { get; set; }
        public short TitleLength { get; set; }
        public short MessageLength { get; set; }
    }
}