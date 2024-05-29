using System.Runtime.InteropServices;

namespace AJ.LoggingV2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MessageData
    {
        public byte LogType;
        public long DateTime;
        public ushort TitleLength;
        public ushort MessageLength;
    }
}