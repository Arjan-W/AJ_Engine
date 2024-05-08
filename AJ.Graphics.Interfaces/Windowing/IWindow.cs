using OpenTK.Mathematics;

namespace AJ.Graphics.Interfaces.Windowing
{
    public delegate void OnCloseWindowRequest();
    public delegate void OnResize(Vector2i newSize);
    public delegate void OnResizeFinished(Vector2i newSize);
    public delegate void OnFocusChanged(bool isFocused);

    public interface IWindow
    {
        string Title { get; set; }
        Vector2i Size { get; set; }
        float AspectRatio { get; }

        event OnCloseWindowRequest OnCloseWindowRequest;
        event OnResize OnResize;
        event OnResizeFinished OnResizeFinished;
        event OnFocusChanged OnFocusChanged;
    }
}
