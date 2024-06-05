namespace AJ.Engine.Graphics.Interfaces.Resources.Textures
{
    public enum MinificationFilter
    {
        NEAREST,
        LINEAR,
        NEAREST_MIPMAP_NEAREST,
        NEAREST_MIPMAP_LINEAR,
        LINEAR_MIPMAP_NEAREST,
        LINEAR_MIPMAP_LINEAR
    }

    public enum MagnificationFilter
    {
        NEAREST,
        LINEAR
    }

    public enum WrapMode
    {
        REPEAT,
        MIRRORED_REPEAT,
        CLAMP_TO_BORDER,
        CLAMP_TO_EDGE
    }

    public interface ISamplerHandle
    {
        public void SetMinificationFilter(MinificationFilter filter);
        public void SetMagnificationFilter(MagnificationFilter filter);
        public void SetTextureHorizontalWrap(WrapMode wrap);
        public void SetTextureVerticalWrap(WrapMode wrap);
    }
}