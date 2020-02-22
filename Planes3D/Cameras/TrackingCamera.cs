using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Planes3D.Cameras
{
    class TrackingCamera : ICamera
    {
        private Camera _camera;
        public TrackingCamera(float ratio = 1)
        {
            _camera = new Camera(new Vector3(), ratio);
        }
        private void calc(Matrix4 position, float angle)
        {
            var t = position.ExtractTranslation();
            t.X -= (float)Math.Cos(angle) * 1f;
            t.Z -= (float)Math.Sin(angle) * 1f;
            t.Y += 0.4f;
            _camera.Position = t;
            _camera.Yaw = angle*(float)(180/Math.PI);
            _camera.Pitch = -2;
           
        }
        public Matrix4 GetProjectionMatrix(Matrix4 position, float angle)
        {
            calc(position, angle);
            return _camera.GetProjectionMatrix();
        }


        public Matrix4 GetViewMatrix(Matrix4 position, float angle)
        {
            calc(position, angle);
            return _camera.GetViewMatrix();
        }

        public void SetRatio(float ratio)
        {
            _camera.AspectRatio = ratio;
        }
    }
}
