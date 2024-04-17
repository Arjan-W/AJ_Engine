namespace AJ.Engine.Interfaces.FileManager
{
    public interface IFileManager
    {
        IFileHandle LoadFile(string path);
    }
}
