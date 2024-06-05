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
    private readonly bool _loadMultithreaded;
    private bool _isLoaded;
    private bool _isInitialized;
    private bool _isDisposed;

    internal Resource(bool loadMultithreaded = true)
    {
        _resourceManager = GraphicsContext.ResourceManager;
        _loadMultithreaded = loadMultithreaded;
        _isLoaded = false;
        _isInitialized = false;
        _isDisposed = false;
    }

    internal bool Load()
    {
        if (_loadMultithreaded) {
            _isLoaded = OnLoad();
            if (_isLoaded)
                _resourceManager.QueueForInitialization(this);
            else
                Dispose();
            return _isLoaded;
        }
        return false;
    }

    protected virtual bool OnLoad() { return false; }

    internal void Initialize()
    {
        if(!_loadMultithreaded)
            _isLoaded = true;

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