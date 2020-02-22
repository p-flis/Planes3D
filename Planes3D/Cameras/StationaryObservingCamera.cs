using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Planes3D.Cameras
{
    public class StationaryObservingCamera : ICamera
    {
        private Camera _camera;
        public StationaryObservingCamera(float ratio = 1)
        {
            _camera = new Camera(new Vector3(9.8f, -0.7f, 2.8f), ratio)
            {
                Yaw = -111f,
                Pitch = -15f
            };
        }

        public Matrix4 GetProjectionMatrix(Matrix4 position, float angle)
        {
            return _camera.GetProjectionMatrix();
        }

        public void SetRatio(float ratio)
        {
            _camera.AspectRatio = ratio;
        }

        public Matrix4 GetViewMatrix(Matrix4 position, float angle)
        {
            return _camera.GetViewMatrix();
        }
    }
}
