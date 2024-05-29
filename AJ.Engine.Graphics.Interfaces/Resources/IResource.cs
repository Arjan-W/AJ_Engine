namespace AJ.Engine.Graphics.Interfaces.Resources;

public delegate void OnInitialized();

public interface IResource
{
    public bool IsLoaded { get; }
    public bool IsInitialized { get; }
    public bool IsDisposed { get; }
    public bool IsReady { get; }

    public event OnInitialized OnInitialized;
}