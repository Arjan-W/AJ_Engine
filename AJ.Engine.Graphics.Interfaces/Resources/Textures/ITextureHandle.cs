namespace AJ.Engine.Graphics.Interfaces.Resources.Textures
{
    public interface ITextureHandle
    {
        public int Width { get; }
        public int Height { get; }
        public bool FlippedHorizontal { get; }
        public bool FlippedVertical { get; }
    }
}