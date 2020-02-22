using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D.Move
{
    interface IMoveModule
    {
        Matrix4 Move(float time);
    }
}
