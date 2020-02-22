using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Planes3D.Cameras
{
    class StationaryTrackingCamera : ICamera
    {
        private Camera _camera;

        public StationaryTrackingCamera(float ratio = 1)
        {
            _camera = new Camera(new Vector3(10.3f, 5.6f, -21.3f), ratio);
        }

        private void calc(Vector3 position)
        {
            var dv = position - _camera.Position;
            _camera.Pitch = (float)((180 / Math.PI) * Math.Asin(dv.Y / dv.Length));
            dv.Y = 0;
            _camera.Yaw = -(float)((180 / Math.PI) * Math.Asin(dv.Z / dv.Length)) + 180;
        }

        public Matrix4 GetProjectionMatrix(Matrix4 position, float angle)
        {
            calc(position.ExtractTranslation());
            return _camera.GetProjectionMatrix();
        }

        public Matrix4 GetViewMatrix(Matrix4 position, float angle)
        {
            calc(position.ExtractTranslation());
            return _camera.GetViewMatrix();
        }

        public void SetRatio(float ratio)
        {
            _camera.AspectRatio = ratio;
        }
    }
}
