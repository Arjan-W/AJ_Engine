using AJ.Engine.Graphics.Interfaces.Windowing;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.ComponentModel;

namespace AJ.Engine.Graphics.OpenTK.Windowing
{
    internal class Display : IDisplay
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

        public event OnCloseRequest OnCloseRequest;
        public event OnResize OnResize;
        public event OnResizeFinished OnResizeFinished;
        public event OnFocusChanged OnFocusChanged;

        private NativeWindow _nativeWindow;
        private bool _isResizing;
        private bool _isResizeFinsihed;

        internal Display(NativeWindow nativeWindow)
        {
            _nativeWindow = nativeWindow;
            _nativeWindow.Closing += _nativeWindow_Closing;
            _nativeWindow.Resize += _nativeWindow_Resize;
            _nativeWindow.FocusedChanged += _nativeWindow_FocusedChanged;
        }

        private void _nativeWindow_Closing(CancelEventArgs obj)
        {
            obj.Cancel = true;
            OnCloseRequest?.Invoke();
        }

        private void _nativeWindow_Resize(ResizeEventArgs obj)
        {
            OnResize?.Invoke(_nativeWindow.ClientSize);
            _isResizing = true;
            _isResizeFinsihed = false;
        }

        private void _nativeWindow_FocusedChanged(FocusedChangedEventArgs obj)
        {
            OnFocusChanged?.Invoke(obj.IsFocused);
        }

        internal void Update()
        {
            if (!_isResizing && _isResizeFinsihed)
            {
                OnResizeFinished?.Invoke(_nativeWindow.ClientSize);
                _isResizeFinsihed = false;
            }

            if (_isResizing)
            {
                _isResizing = false;
                _isResizeFinsihed = true;
            }
        }
    }
}