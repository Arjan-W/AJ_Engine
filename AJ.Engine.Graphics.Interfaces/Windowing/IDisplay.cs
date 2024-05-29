using OpenTK.Mathematics;

namespace AJ.Engine.Graphics.Interfaces.Windowing
{
    public delegate void OnCloseRequest();
    public delegate void OnResize(Vector2i newSize);
    public delegate void OnResizeFinished(Vector2i newSize);
    public delegate void OnFocusChanged(bool isFocused);

    public interface IDisplay
    {
        string Title { get; set; }
        Vector2i Size { get; set; }
        float AspectRatio { get; }

        event OnCloseRequest OnCloseRequest;
        event OnResize OnResize;
        event OnResizeFinished OnResizeFinished;
        event OnFocusChanged OnFocusChanged;
    }
}