using System.IO;

namespace AJ.Engine.Interfaces.FileManager
{
    public delegate void OnFileChange(IFileHandle handle);

    public interface IFileHandle
    {
        public string Path { get; }

        public string AbsolutePath { get; }

        public bool IsExternal { get; }

        public event OnFileChange OnFileChange;

        public Stream OpenRead();

        public Stream OpenWrite();
    }
}
