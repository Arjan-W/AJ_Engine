using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace AJ.Engine.Graphics.OpenTK.Resources.Shaders;

internal class ShaderUniform : IShaderUniform
{
    private readonly string _name;
    private readonly int _location;
    private readonly ActiveUniformType _uniformType;

    internal ShaderUniform(string name, int location, ActiveUniformType uniformType)
    {
        _name = name;
        _location = location;
        _uniformType = uniformType;
    }

    public void SetUniform(bool value)
    {
        if (_uniformType == ActiveUniformType.Bool)
        {
            GL.Uniform1(_location, value ? 1 : 0);
        }
    }

    public void SetUniform(int value)
    {
        if (_uniformType == ActiveUniformType.Int || _uniformType == ActiveUniformType.Sampler2D)
        {
            GL.Uniform1(_location, value);
        }
    }

    public void SetUniform(int[] values)
    {
        if (_uniformType == ActiveUniformType.Int || _uniformType == ActiveUniformType.Sampler2D)
        {
            GL.Uniform1(_location, values.Length, values);
        }
    }

    public void SetUniform(float value)
    {
        if (_uniformType == ActiveUniformType.Float)
        {
            GL.Uniform1(_location, value);
        }
    }

    public void SetUniform(double value)
    {
        if (_uniformType == ActiveUniformType.Double)
        {
            GL.Uniform1(_location, value);
        }
    }

    public void SetUniform(Vector2 value)
    {
        if (_uniformType == ActiveUniformType.FloatVec2)
        {
            GL.Uniform2(_location, value);
        }
    }

    public void SetUniform(Vector3 value)
    {
        if (_uniformType == ActiveUniformType.FloatVec3)
        {
            GL.Uniform3(_location, value);
        }
    }

    public void SetUniform(Vector4 value)
    {
        if (_uniformType == ActiveUniformType.FloatVec4)
        {
            GL.Uniform4(_location, value);
        }
    }

    public void SetUniform(Matrix2 value)
    {
        if (_uniformType == ActiveUniformType.FloatMat2)
        {
            GL.UniformMatrix2(_location, false, ref value);
        }
    }

    public void SetUniform(Matrix3 value)
    {
        if (_uniformType == ActiveUniformType.FloatMat3)
        {
            GL.UniformMatrix3(_location, false, ref value);
        }
    }

    public void SetUniform(Matrix4 value)
    {
        if (_uniformType == ActiveUniformType.FloatMat4)
        {
            GL.UniformMatrix4(_location, false, ref value);
        }
    }
}