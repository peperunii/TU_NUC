using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Cameras
{
    public abstract class Camera
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public abstract object GetData(CameraDataType type);
    }
}
