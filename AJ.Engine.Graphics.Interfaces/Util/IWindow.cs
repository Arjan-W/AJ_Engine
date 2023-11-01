using OpenTK.Mathematics;

namespace AJ.Engine.Graphics.Interfaces.Util
{
    public delegate void OnCloseWindowRequest();
    public delegate void OnResize(Vector2i newSize);
    public delegate void OnResizeFinished(Vector2i newSize);
    public delegate void OnFocusChanged(bool isFocused);

    public interface IWindow
    {
        public string Title { get; set; }
        public Vector2i Size { get; set; }
        public float AspectRatio { get; }

        public event OnCloseWindowRequest OnCloseWindowRequest;
        public event OnResize OnResize;
        public event OnResizeFinished OnResizeFinished;
        public event OnFocusChanged OnFocusChanged;
    }
}