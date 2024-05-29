using System.Runtime.InteropServices;

namespace AJ.Engine.Logging
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