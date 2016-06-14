using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KinectHandTracker
{
    public static class Constants
    {
        public static readonly double dpi = 96.0;
        public static readonly PixelFormat format = PixelFormats.Bgr32;
        public static readonly int bytesPerPixel = (format.BitsPerPixel + 7)/8;
        public static readonly int croppedRegionWidth = 90;
        public static readonly int croppedRegionHeight = 90;
        public static readonly int croppedReginSize = croppedRegionWidth*croppedRegionHeight;
    }
}
