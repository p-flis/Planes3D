using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D.Move
{
    class EightMove : IMoveModule
    {
        private readonly Vector2 p1 = new Vector2(2.5f, -22f);
        private readonly Vector2 p2 = new Vector2(-19.3f, -42f);
        private readonly Vector2 p3 = new Vector2(25f, -60f);
        private readonly Vector2 p4 = new Vector2(2.5f, -22f);

        public Matrix4 Move(float time)
        {
            var tp1 = p1;
            var tp2 = p2;
            var tp3 = p3;
            var tp4 = p4;
            var coef = 1f;
            if(time > 1)
            {
                time -= 1;
                coef *= -1;
                tp1 = p1;
                tp4 = p4;
                tp3 = (p1 - p2) + p1;
                tp2 = (p1 - p3) + p1;
            }

            Vector2 r = Bezier.Value(time, tp1, tp2, tp3, tp4);
            float an = Bezier.Angle(time, tp1, tp2, tp3, tp4);
            float cen = Bezier.Centriperal(time, tp1, tp2, tp3, tp4);

            return Matrix4.CreateRotationX(cen*cen*25*coef) * Matrix4.CreateRotationY(-an) * Matrix4.CreateTranslation(r.X, 0, r.Y);
        }
    }
}
