using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planes3D.Cameras
{
    static class CameraFactory
    {
        static public ICamera Produce(CameraMode mode, float ratio = 1)
        {
            switch (mode)
            {
                case CameraMode.StationaryObserving:
                    return new StationaryObservingCamera(ratio);
                case CameraMode.StationaryTracking:
                    return new StationaryTrackingCamera(ratio);
                case CameraMode.Tracking:
                    return new TrackingCamera(ratio);
                default:
                    break;
            }
            throw new Exception();
        }
    }
}
