using OpenTK.Mathematics;

namespace AJ.Engine.Graphics.Interfaces.Resources.Shaders;

public interface IShaderUniform
{
    void SetUniform(bool value);
    void SetUniform(int value);
    void SetUniform(int[] values);
    void SetUniform(float value);
    void SetUniform(double value);
    void SetUniform(Vector2 value);
    void SetUniform(Vector3 value);
    void SetUniform(Vector4 value);
    void SetUniform(Matrix2 value);
    void SetUniform(Matrix3 value);
    void SetUniform(Matrix4 value);
}