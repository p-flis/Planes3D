using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D.Move
{
    public static class Bezier
    {
        public static Vector2 Value(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            return
               (float)Math.Pow(1 - t, 3) * p1 +
               3 * (float)Math.Pow(1 - t, 2) * (float)Math.Pow(t, 1) * p2 +
               3 * (float)Math.Pow(1 - t, 1) * (float)Math.Pow(t, 2) * p3 +
               (float)Math.Pow(t, 3) * p4;
        }

        public static float Angle(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var r1 = Value(t, p1, p2, p3, p4);
            var r2 = Value(t + 0.0001f, p1, p2, p3, p4);
            return (float)Math.Atan2(r2.Y - r1.Y, r2.X - r1.X);
        }

        public static float Centriperal(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            var d = 0.01f;
            var r1 = Value(t - d, p1, p2, p3, p4);
            var r2 = Value(t, p1, p2, p3, p4);
            var r3 = Value(t + d, p1, p2, p3, p4);
            var x = r2 - r1;
            var y = r3 - r2;
            double sin = x.X * y.Y - y.X * x.Y;
            double cos = x.X * y.X + x.Y * y.Y;

            float angle = -(float)Math.Atan2(sin, cos);
            if (angle < -Math.PI / 2)
            {
                angle = -(float)(Math.PI - angle);
                angle *= -1;
            }
            if (angle > Math.PI / 2)
            {
                angle = (float)(Math.PI - angle);
                angle *= -1;
            }

            return angle;
        }
    }
}
