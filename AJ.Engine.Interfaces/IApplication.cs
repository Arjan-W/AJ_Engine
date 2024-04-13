namespace AJ.Engine.Interfaces
{
    public interface IApplication
    {
        public string Title { get; }
        public bool EnableGraphics {  get; }
        public bool CloseOnRequest { get; }
    }
}