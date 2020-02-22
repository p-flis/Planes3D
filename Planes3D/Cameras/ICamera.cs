using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D.Cameras
{
    interface ICamera
    {
        Matrix4 GetProjectionMatrix(Matrix4 position, float angle);
        Matrix4 GetViewMatrix(Matrix4 position, float angle);
        void SetRatio(float ratio);
    }
}
