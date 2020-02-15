using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Planes3D
{
    class TimeModule
    {
        Timer timer;
        int ticks;

        public TimeModule(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            ticks++;
            ticks %= 10000;
        }

        public int Current()
        {
            return ticks;
        }

        public void Reset()
        {
            ticks = 0;
        }
    }
}
