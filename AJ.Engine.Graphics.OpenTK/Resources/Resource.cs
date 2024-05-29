using AJ.Engine.Graphics.Interfaces.Resources;

namespace AJ.Engine.Graphics.OpenTK.Resources;

internal abstract class Resource : IResource
{
    public bool IsLoaded => _isLoaded;
    public bool IsInitialized => _isInitialized;
    public bool IsDisposed => _isDisposed;
    public bool IsReady => _isLoaded && _isInitialized && !_isDisposed;

    public event OnInitialized OnInitialized;

    private readonly ResourceManager _resourceManager;
    private bool _isLoaded;
    private bool _isInitialized;
    private bool _isDisposed;

    internal Resource()
    {
        _resourceManager = GraphicsContext.ResourceManager;
        _isLoaded = false;
        _isInitialized = false;
        _isDisposed = false;
    }

    internal bool Load()
    {
        _isLoaded = OnLoad();
        if (_isLoaded)
            _resourceManager.QueueForInitialization(this);
        else
            Dispose();
        return _isLoaded;
    }

    protected abstract bool OnLoad();

    internal void Initialize()
    {
        _isInitialized = OnInitialize();
        if (_isInitialized)
            OnInitialized?.Invoke();
        else
            Dispose();
    }

    protected abstract bool OnInitialize();

    internal void Dispose()
    {
        if (!_isDisposed)
        {
            OnDispose();
            _isDisposed = true;
        }
    }

    protected abstract void OnDispose();
}