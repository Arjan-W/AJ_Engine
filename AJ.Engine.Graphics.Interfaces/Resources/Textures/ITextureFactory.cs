namespace AJ.Engine.Graphics.Interfaces.Resources.Textures
{
    public interface ITextureFactory
    {
        public ITextureHandle CreateTexture(string path, bool flipHorizontal = false, bool flipVertical = false);
        public void DisposeTexture(ITextureHandle texture);

        public ISamplerHandle CreateSampler();
        public void DisposeSampler(ISamplerHandle sampler);
    }
}