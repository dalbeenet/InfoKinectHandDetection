using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KinectHandTracker
{
    public static class Utilites
    {
        public static BitmapSource DepthToBitmap(ushort[] src, int width, int height)
        {
            byte[] pixels = new byte[width * height * Constants.bytesPerPixel];
            WriteableBitmap bitmap = new WriteableBitmap(width, height, Constants.dpi, Constants.dpi, Constants.format, null);

            // Convert the depth to RGB.
            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < src.Length; ++depthIndex)
            {
                // Get the depth for this pixel
                ushort depth = src[depthIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).
                //byte intensity = (byte)(depth >= Parameters.minDepth && depth <= Parameters.maxDepth ? depth : 0);

                if (depth != 0)
                {
                    var attrs = BitConverter.GetBytes(src[depthIndex]);
                    pixels[colorIndex++] = attrs[0]; // Blue
                    pixels[colorIndex++] = (byte)(255 - attrs[0]); // Green
                    pixels[colorIndex++] = 0; // Red    
                }
                else
                {
                    pixels[colorIndex++] = 0; // Blue
                    pixels[colorIndex++] = 0; // Green
                    pixels[colorIndex++] = 0; // Red     
                }
 
                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                // If we were outputting BGRA, we would write alpha here.
                ++colorIndex;
            }

            bitmap.Lock();

            Marshal.Copy(pixels, 0, bitmap.BackBuffer, pixels.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, Constants.croppedRegionWidth, Constants.croppedRegionHeight));

            bitmap.Unlock();

            return bitmap;
        }

        public static void DepthToFile(string className, ushort[] src, int width, int height)
        {
            byte[] pixels = new byte[width * height * Constants.bytesPerPixel];
            WriteableBitmap bitmap = new WriteableBitmap(width, height, Constants.dpi, Constants.dpi, Constants.format, null);

            // Convert the depth to RGB.
            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < src.Length; ++depthIndex)
            {
                // Get the depth for this pixel
                ushort depth = src[depthIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).
                if (depth != 0)
                {
                    var attrs = BitConverter.GetBytes(src[depthIndex]);
                    pixels[colorIndex++] = attrs[0]; // Blue
                    pixels[colorIndex++] = (byte)(255 - attrs[0]); // Green
                    pixels[colorIndex++] = 0; // Red    
                }
                else
                {
                    pixels[colorIndex++] = 0; // Blue
                    pixels[colorIndex++] = 0; // Green
                    pixels[colorIndex++] = 0; // Red     
                }

                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                // If we were outputting BGRA, we would write alpha here.
                ++colorIndex;
            }

            bitmap.Lock();

            Marshal.Copy(pixels, 0, bitmap.BackBuffer, pixels.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, Constants.croppedRegionWidth, Constants.croppedRegionHeight));

            bitmap.Unlock();
            CreateThumbnail("Output\\" + className + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png", bitmap.Clone());            
        }

        private static void CreateThumbnail(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                    stream5.Close();
                }
            }
        }
    }
}
