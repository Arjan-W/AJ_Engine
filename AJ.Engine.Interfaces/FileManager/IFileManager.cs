namespace AJ.Engine.Interfaces.FileManager
{
    public interface IFileManager
    {
        void ScanInternalFiles();
        IFileHandle LoadFile(string path);
    }
}
