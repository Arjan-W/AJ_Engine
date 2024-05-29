namespace AJ.Engine.Graphics.Interfaces.Resources.Shaders;

public interface IShaderFactory
{
    public IShaderHandle CreateShader(string path);
    public void DisposeShader(IShaderHandle shader);
}