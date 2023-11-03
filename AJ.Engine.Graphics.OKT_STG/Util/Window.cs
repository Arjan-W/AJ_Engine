using AJ.Engine.Graphics.Interfaces.Util;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace AJ.Engine.Graphics.OKT_STG.Util
{
    internal class Window : IWindow
    {
        public string Title
        {
            get => _nativeWindow.Title;
            set => _nativeWindow.Title = value;
        }

        public Vector2i Size
        {
            get => _nativeWindow.ClientSize;
            set => _nativeWindow.ClientSize = value;
        }

        public float AspectRatio
        {
            get => (float)_nativeWindow.ClientSize.X / (float)_nativeWindow.ClientSize.Y;
        }

        public event OnCloseWindowRequest OnCloseWindowRequest;
        public event OnResize OnResize;
        public event OnResizeFinished OnResizeFinished;
        public event OnFocusChanged OnFocusChanged;

        private NativeWindow _nativeWindow;
        private bool _isResizing;
        private bool _isResizeFinsihed;

        internal Window(NativeWindow nativeWindow)
        {
            _nativeWindow = nativeWindow;
            _nativeWindow.Closing += _nativeWindow_Closing;
            _nativeWindow.Resize += _nativeWindow_Resize;
            _nativeWindow.FocusedChanged += _nativeWindow_FocusedChanged;
        }

        private void _nativeWindow_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            obj.Cancel = true;
            OnCloseWindowRequest?.Invoke();
        }

        private void _nativeWindow_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            OnResize?.Invoke(_nativeWindow.ClientSize);
            _isResizing = true;
            _isResizeFinsihed = false;
        }

        private void _nativeWindow_FocusedChanged(OpenTK.Windowing.Common.FocusedChangedEventArgs obj)
        {
            OnFocusChanged?.Invoke(obj.IsFocused);
        }

        internal void Update()
        {
            if(!_isResizing && _isResizeFinsihed)
            {
                OnResizeFinished?.Invoke(_nativeWindow.ClientSize);
                _isResizeFinsihed = false;
            }

            if (_isResizing)
            {
                _isResizing = false;
                _isResizeFinsihed = true;
            }
            _nativeWindow.Context.SwapBuffers();
        }
    }
}