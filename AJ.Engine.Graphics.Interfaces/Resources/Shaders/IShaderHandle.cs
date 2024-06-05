using AJ.Engine.Graphics.Interfaces.Resources;

namespace AJ.Engine.Graphics.Interfaces.Resources.Shaders;

public interface IShaderHandle : IResource
{
    public bool HasVertexShader { get; }
    public bool HasGeometryShader { get; }
    public bool HasFragmentShader { get; }
    public IShaderUniform this[string value] { get; }

    public bool ValidateProgram();
    public bool Bind();
}