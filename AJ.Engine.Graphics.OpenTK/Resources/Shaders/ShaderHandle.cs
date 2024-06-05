using AJ.Engine.Graphics.Interfaces.Resources;
using AJ.Engine.Graphics.Interfaces.Resources.Shaders;
using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.TaskManagement;
using AJ.Engine.Interfaces.Util;
using AJ.Engine.Logging.Interfaces;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AJ.Engine.Graphics.OpenTK.Resources.Shaders;

internal class ShaderHandle : Resource, IShaderHandle, ITask
{
    private const string LOG_TITLE = "ShaderHandle";

    private const string VERTEX_TAG = "$VERTEX$";
    private const string GEOMETRY_TAG = "$GEOMETRY$";
    private const string FRAGMENT_TAG = "$FRAGMENT$";

    private const int VERTEX_SHADER_ID = 0;
    private const int GEOMETRY_SHADER_ID = 1;
    private const int FRAGMENT_SHADER_ID = 2;
    private const int NUMBER_OF_SHADERS = 3;

    public bool HasVertexShader => _hasVertexShader;
    public bool HasGeometryShader => _hasGeometryShader;
    public bool HasFragmentShader => _hasFragmentShader;

    public IShaderUniform this[string value]
    {
        get
        {
            _shaderUniforms.TryGetValue(value, out var shaderUniform);
            return shaderUniform;
        }
    }

    private readonly ILogger _logger;
    private readonly IFileHandle _fileHandle;
    private StringBuilder _vertexShaderSource;
    private StringBuilder _geometryShaderSource;
    private StringBuilder _fragmentShaderSource;
    private bool _hasVertexShader;
    private bool _hasGeometryShader;
    private bool _hasFragmentShader;
    private int[] _shaderIds;
    private bool[] _shaderStatuses;
    private Dictionary<string, ShaderUniform> _shaderUniforms;
    private int _programId;

    internal ShaderHandle(IFileHandle fileHandle)
    {
        _logger = GraphicsContext.Logger;
        _fileHandle = fileHandle;
        _hasVertexShader = false;
        _hasGeometryShader = false;
        _hasFragmentShader = false;
        _shaderIds = new int[NUMBER_OF_SHADERS];
        Array.Fill(_shaderIds, INVALID_ID);
        _shaderStatuses = new bool[NUMBER_OF_SHADERS];
        Array.Fill(_shaderStatuses, false);
        _shaderUniforms = new Dictionary<string, ShaderUniform>();
        _programId = INVALID_ID;
    }

    public bool OnRunTask() => Load();

    protected override bool OnLoad()
    {
        try
        {
            using (StreamReader sr = new StreamReader(_fileHandle.OpenRead()))
            {
                string line;
                StringBuilder currentStringBuilder = null;
                while ((line = sr.ReadLine()) != null)
                {
                    switch (line)
                    {
                        case VERTEX_TAG:
                            if (_vertexShaderSource == null)
                            {
                                _vertexShaderSource = new StringBuilder();
                                _hasVertexShader = true;
                            }
                            currentStringBuilder = _vertexShaderSource;
                            break;
                        case GEOMETRY_TAG:
                            if (_geometryShaderSource == null)
                            {
                                _geometryShaderSource = new StringBuilder();
                                _hasGeometryShader = true;
                            }
                            currentStringBuilder = _geometryShaderSource;
                            break;
                        case FRAGMENT_TAG:
                            if (_fragmentShaderSource == null)
                            {
                                _fragmentShaderSource = new StringBuilder();
                                _hasFragmentShader = true;
                            }
                            currentStringBuilder = _fragmentShaderSource;
                            break;
                        default:
                            currentStringBuilder?.AppendLine(line);
                            break;
                    }
                }
            }

            if (!_hasVertexShader || !_hasFragmentShader)
            {
                _logger.LogError(LOG_TITLE, $"Shader sources from {_fileHandle.AbsolutePath} need atleast a one vertex and one fragment source!");
                return false;
            }

            _logger.LogDebug(LOG_TITLE, $"Shader sources loaded from {_fileHandle.AbsolutePath} loaded!");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(LOG_TITLE, $"Encountered a error while trying to load shader source {_fileHandle.AbsolutePath}{Globals.NewLine}{e.ToString()}");
            return false;
        }
    }

    protected override bool OnInitialize()
    {
        if (_hasVertexShader)
            _shaderStatuses[VERTEX_SHADER_ID] = CreateShader(out _shaderIds[VERTEX_SHADER_ID],
                _vertexShaderSource.ToString(), ShaderType.VertexShader);
        if (_hasGeometryShader)
            _shaderStatuses[GEOMETRY_SHADER_ID] = CreateShader(out _shaderIds[GEOMETRY_SHADER_ID],
                _geometryShaderSource.ToString(), ShaderType.GeometryShader);
        if (_hasFragmentShader)
            _shaderStatuses[FRAGMENT_SHADER_ID] = CreateShader(out _shaderIds[FRAGMENT_SHADER_ID],
                _fragmentShaderSource.ToString(), ShaderType.FragmentShader);

        if (_shaderStatuses[VERTEX_SHADER_ID] && _shaderStatuses[FRAGMENT_SHADER_ID] &&
            _hasGeometryShader == _shaderStatuses[GEOMETRY_SHADER_ID])
        {
            if (!CreateProgram(out var programId))
                return false;

            AttachShaders(programId);

            if (!LinkShaders(programId))
                return false;

            DeleteShaders();
            FindAllShaderUniforms(programId);

            _programId = programId;
            _logger.LogDebug(LOG_TITLE, $"ShaderHandle {_fileHandle.AbsolutePath} initialized!");
            return true;
        }
        else
            return false;
    }

    private bool CreateShader(out int shaderId, string source, ShaderType shaderType)
    {
        shaderId = INVALID_ID;
        if (string.IsNullOrWhiteSpace(source))
        {
            _logger.LogError(LOG_TITLE, $"No source provided for {shaderType} shader for shaderHandle{_fileHandle.AbsolutePath}!");
            return false;
        }

        int id = GL.CreateShader(shaderType);

        if (id == Globals.FALSE)
        {
            _logger.LogError(LOG_TITLE, $"Failed to create a {shaderType} shader for shaderHandle {_fileHandle.AbsolutePath}!");
            return false;
        }

        GL.ShaderSource(id, source);

        GL.CompileShader(id);
        GL.GetShader(id, ShaderParameter.CompileStatus, out int compileStatus);

        if (compileStatus == Globals.FALSE)
        {
            _logger.LogError(LOG_TITLE, $"Failed to compile {shaderType} shader for shaderHandle {_fileHandle.AbsolutePath}!{Globals.NewLine}{GL.GetShaderInfoLog(id)}");
            return false;
        }

        shaderId = id;
        return true;
    }

    private bool CreateProgram(out int programId)
    {
        programId = GL.CreateProgram();

        if (programId == Globals.FALSE)
        {
            _logger.LogError(LOG_TITLE, $"Failed to create program for shaderHandle {_fileHandle.AbsolutePath}!");
            return false;
        }

        return true;
    }

    private void AttachShaders(int programId)
    {
        for (int i = 0; i < NUMBER_OF_SHADERS; i++)
        {
            if (_shaderStatuses[i])
                GL.AttachShader(programId, _shaderIds[i]);
        }
    }

    private bool LinkShaders(int programId)
    {
        GL.LinkProgram(programId);
        GL.GetProgram(programId, GetProgramParameterName.LinkStatus, out int status);
        if (status == Globals.FALSE)
        {
            _logger.LogError(LOG_TITLE, $"Failed to Link program for shaderHandle {_fileHandle.AbsolutePath}!{Globals.NewLine}{GL.GetProgramInfoLog(programId)}");
            return false;
        }

        return true;
    }

    public bool ValidateProgram()
    {
        if (IsReady)
        {
            GL.ValidateProgram(_programId);
            GL.GetProgram(_programId, GetProgramParameterName.ValidateStatus, out int validationStatus);
            if (validationStatus == Globals.FALSE)
            {
                _logger.LogError(LOG_TITLE,
                    $"Failed to validate program for shaderHandle {_fileHandle.AbsolutePath}!{Globals.NewLine}{GL.GetProgramInfoLog(_programId)}");
                return false;
            }

            return true;
        }

        return false;
    }

    public bool Bind() {
        if (IsReady) {
            GL.UseProgram(_programId);
            return true;
        }
        return false;
    }

    private void FindAllShaderUniforms(int programId)
    {
        GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, out int activeUniformCount);
        for (int i = 0; i < activeUniformCount; i++)
        {
            GL.GetActiveUniform(programId, i, 60, out var _, out var _, out ActiveUniformType uniformType, out string uniformName);
            int uniformLocation = GL.GetUniformLocation(programId, uniformName);
            _shaderUniforms.Add(uniformName, new ShaderUniform(uniformName, uniformLocation, uniformType));
        }
    }

    protected override void OnDispose()
    {
        if (_programId != INVALID_ID)
        {
            DeleteShaders();
            GL.DeleteShader(_programId);
            _logger.LogDebug(LOG_TITLE, $"Deleted shaderHandle {_fileHandle.AbsolutePath}!");
        }
    }

    private void DeleteShaders()
    {
        for (int i = 0; i < NUMBER_OF_SHADERS; i++)
        {
            ref int shaderId = ref _shaderIds[i];
            ref bool shaderSuccesStatus = ref _shaderStatuses[i];

            if (shaderId != INVALID_ID && shaderSuccesStatus)
            {
                GL.DeleteShader(shaderId);
                shaderId = INVALID_ID;
                shaderSuccesStatus = false;
            }
        }
    }
}