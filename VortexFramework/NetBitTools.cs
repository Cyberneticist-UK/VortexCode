using System;
using System.Drawing;

namespace VortexLibrary
{
    public static class NetBitTools
    {
        /// <summary>
        /// From the pixels in the image work out the average colour!
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Color AverageColour(NetBitmap image)
        {
            return image.AverageColour;
        }

        /// <summary>
        /// Invert the image colour!
        /// </summary>
        /// <param name="image"></param>
        //public static void InvertImage(NetBitmap image)
        //{
        //    int pixel = 0;
        //    for (int y = 0; y < image.Properties.Height; y++)
        //    {
        //        for (int x = 0; x < image.Properties.Width; x++)
        //        {
        //            pixel = (y * image.Properties.Stride) + (x * image.Properties.PixelFormatSize);
        //            image.bits[(int)Bitmap_NetStride.Red + pixel] = (byte)(255 - image.bits[(int)Bitmap_NetStride.Red + pixel]);
        //            image.bits[(int)Bitmap_NetStride.Green + pixel] = (byte)(255 - image.bits[(int)Bitmap_NetStride.Green + pixel]);
        //            image.bits[(int)Bitmap_NetStride.Blue + pixel] = (byte)(255 - image.bits[(int)Bitmap_NetStride.Blue + pixel]);
        //        }
        //    }
        //}

        /// <summary>
        /// Alter the image colour!
        /// </summary>
        /// <param name="image"></param>
        //public static void AlterColour(NetBitmap image, Bitmap_NetStride Colour, double value, bool Percentage)
        //{
        //    int pixel = 0;
        //    double Temp = 0;
        //    for (int y = 0; y < image.Properties.Height; y++)
        //    {
        //        for (int x = 0; x < image.Properties.Width; x++)
        //        {
        //            pixel = (y * image.Properties.Stride) + (x * image.Properties.PixelFormatSize);

        //            if (Percentage == false)
        //            {
        //                Temp = Convert.ToDouble(image.bits[(int)Colour + pixel]) + value;
        //            }
        //            else
        //            {
        //                Temp = Convert.ToDouble(image.bits[(int)Colour + pixel]) +
        //                    (
        //                        Convert.ToDouble(image.bits[(int)Colour + pixel]) * (value / 100)
        //                    );

        //                if (Temp > 255)
        //                    Temp = 255;
        //                if (Temp < 0)
        //                    Temp = 0;


        //                image.bits[(int)Colour + pixel] = (byte)Temp;
        //            }
        //        }
        //    }            
        //}

        /// <summary>
        /// Take the average colour value as "rgb(r,g,b)" string
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string AverageColourHTML(NetBitmap image)
        {
            Color temp = AverageColour(image);
            return "rgb("+temp.R+"," + temp.G + "," + temp.B + ")";
        }

        /// <summary>
        /// Looks at the colour and returns true if the colour is dark, false if light
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static bool ColourIsDark(Color test)
        {
            if ((test.R+test.G+test.B)/3 < 128)
            {
                return true;
            }
            return false;
        }
    }
}
