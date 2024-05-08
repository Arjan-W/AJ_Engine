using OpenTK.Mathematics;
using System;


namespace AJ.Engine.Graphics.Cameras
{
    internal class PerspectiveCamera : Camera {

        public float _fov;

        public float FOV {
            get => _fov;
            set {
                _fov = value;
                ProjectionChanged();
            }
        }


        public PerspectiveCamera(Vector3 position, Vector3 z, float zNear, float zFar) : base(position, z, zNear, zFar) {

        }

        protected override Matrix4 CalculateProjection() {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, ratio, zNear, zFar);
        }
    }
}
