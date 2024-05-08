using OpenTK.Mathematics;
using System;


public abstract class Camera {

    private Vector3 _position;
    private Vector3 _x_vector;
    private Vector3 _y_vector;
    private Vector3 _z_vector;
    private float _zNear;
    private float _zFar;
    private bool _isProjectionChanged;
    private bool _isViewChanged;
    private Matrix4 _viewMatrix;
    private Matrix4 _projectionMatrix;
    private Matrix4 _viewProjectionMatrix;

    public Vector3 Position {
        get => _position;
        set {
            _position = value;
            _isViewChanged = true;
        }
    }

    public float Position_X {
        get => _position.X;
        set {
            _position.X = value;
            _isViewChanged = true;
        }
    }

    public float Position_Y {
        get => _position.Y;
        set {
            _position.Y = value;
            _isViewChanged = true;
        }
    }

    public float Position_Z {
        get => _position.Z;
        set {
            _position.Z = value;
            _isViewChanged = true;
        }
    }

    public Vector3 x_vector {
        get => _x_vector;
        set {
            _x_vector = value;
            _isViewChanged = true;
        }
    }

    public Vector3 y_vector {
        get => _y_vector;
        set {
            _y_vector = value;
            _isViewChanged = true;
        }
    }

    public Vector3 z_vector {
        get => _z_vector;
        set {
            _z_vector = value;
            _isViewChanged = true;
        }
    }

    public float zNear {
        get => _zNear;
        set {
            _zNear = value;
            _isViewChanged = true;
        }
    }

    public float zFar {
        get => _zFar;
        set {  
            _zFar = value;
            _isViewChanged = true;
        }
    }

    protected Camera(Vector3 position, Vector3 z, float zNear, float zFar) {
        _position = position;
        _z_vector = z;
        _x_vector = Vector3.Cross(new Vector3(0,1,0), _z_vector);
        _y_vector = Vector3.Cross(_x_vector, _z_vector);        
        _zNear = zNear;
        _zFar = zFar;
        _isViewChanged = true;
        _isProjectionChanged = true;
    }

    public void Update() {
        if (_isProjectionChanged || _isViewChanged) {
            if (_isProjectionChanged)
                _projectionMatrix = CalculateProjection();

            if (_isViewChanged)
                _viewMatrix = CalculateView();

            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;
            _isProjectionChanged = false;
            _isViewChanged = false;
        }
    }

    protected void ProjectionChanged() {
        _isProjectionChanged = true;
    }

    protected abstract Matrix4 CalculateProjection();

    protected virtual Matrix4 CalculateView() {
        Matrix4 matrix1 = new Matrix4();
        matrix1.Row0 = new Vector4(_x_vector.X, _x_vector.Y, _x_vector.Z, 0);
        matrix1.Row1 = new Vector4(_y_vector.X, _y_vector.Y, _y_vector.Z, 0);
        matrix1.Row2 = new Vector4(_z_vector.X, _z_vector.Y, _z_vector.Z, 0);
        matrix1.M44 = 1;

        Matrix4 matrix2 = Matrix4.Identity;
        matrix2.Column3 = new Vector4(_position.X, _position.Y, _position.Z, 1);

        return matrix1 * matrix2;
    }
}
