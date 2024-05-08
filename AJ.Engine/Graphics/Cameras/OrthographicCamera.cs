using OpenTK.Mathematics;
using System;


namespace AJ.Engine.Graphics.Cameras
{
    public class OrthographicCamera : Camera
    {
        private float _width;
        private float _height;

        public OrthographicCamera(Vector3 position, Vector3 z, float zNear, float zFar) : base(position, z, zNear, zFar) {
            _width = z.X;
            _height = z.Y;
        }

        public float Width {
            get => _width;
            set { 
                _width = value;
                ProjectionChanged();
            }
        }

        public float Height {
            get => _width;
            set {
                _height = value;
                ProjectionChanged();
            }
        }

        protected override Matrix4 CalculateProjection() {
            return Matrix4.CreateOrthographic(_width, _height, zNear, zFar);
        }
    }
}
