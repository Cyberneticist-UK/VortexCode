using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VortexLibrary
{

    public class NetBitmap : IDisposable
    {
        Bitmap internalImage;
        string filename;
        private bool disposedValue;
        public bool imageDamaged = false;
        public NetBitmap(string filename, bool AutoRotate = true)
        {
            try
            {
                imageDamaged = false;
                if (internalImage != null)
                    internalImage.Dispose();
                this.filename = filename;
                internalImage = new Bitmap(filename);
                if (AutoRotate)
                    ExifRotate();
            }
            catch
            {
                imageDamaged = true;
            }
        }

        public NetBitmap(Bitmap image)
        {
            try
            {
                imageDamaged = false;
                if (internalImage != null)
                    internalImage.Dispose();
                this.filename = "";
                internalImage = (Bitmap)image.Clone();
            }
            catch
            {
                imageDamaged = true;
            }
        }
        public NetBitmap(int Width, int Height)
        {
            try
            {
                imageDamaged = false;
                if (internalImage != null)
                    internalImage.Dispose();
                this.filename = "";
                internalImage = new Bitmap(Width, Height);
            }
            catch
            {
                imageDamaged = true;
            }
        }

        public NetBitmap(NetBitmap Original)
        {
            try
            {
                imageDamaged = false;
                if (internalImage != null)
                    internalImage.Dispose();
                this.filename = "";
                internalImage = Original.internalImage;
            }
            catch
            {
                imageDamaged = true;
            }

        }

        public Bitmap ToImage()
        {
            return internalImage;
        }

        private void ExifRotate()
        {
            int exifOrientationID = 0x112; //274
            if (Array.IndexOf(internalImage.PropertyIdList, exifOrientationID) < 0)
                return;

            var prop = internalImage.GetPropertyItem(exifOrientationID);
            int val = BitConverter.ToUInt16(prop.Value, 0);
            var rot = RotateFlipType.RotateNoneFlipNone;

            if (val == 3 || val == 4)
                rot = RotateFlipType.Rotate180FlipNone;
            else if (val == 5 || val == 6)
                rot = RotateFlipType.Rotate90FlipNone;
            else if (val == 7 || val == 8)
                rot = RotateFlipType.Rotate270FlipNone;

            if (val == 2 || val == 4 || val == 5 || val == 7)
                rot |= RotateFlipType.RotateNoneFlipX;

            if (rot != RotateFlipType.RotateNoneFlipNone)
                internalImage.RotateFlip(rot);
        }

        public DateTime DateTaken
        {
            get
            {
                DateTime dtaken;

                try
                {
                    //Property Item 306 corresponds to the Date Taken
                    PropertyItem propItem = internalImage.GetPropertyItem(0x0132);
                    //Convert date taken metadata to a DateTime object
                    string sdate = Encoding.UTF8.GetString(propItem.Value).Trim();
                    string secondhalf = sdate.Substring(sdate.IndexOf(" "), (sdate.Length - sdate.IndexOf(" ")));
                    string firsthalf = sdate.Substring(0, 10);
                    firsthalf = firsthalf.Replace(":", "-");
                    sdate = firsthalf + secondhalf;
                    dtaken = DateTime.Parse(sdate);
                }
                catch
                {
                    if (filename == null || filename == "")
                        dtaken = System.DateTime.Now;
                    else
                        dtaken = new System.IO.FileInfo(filename).CreationTime;
                }
                return dtaken;
            }
        }
        public Color AverageColour
        {
            get
            {
                BitmapData bitmapData = internalImage.LockBits(new Rectangle(0, 0, internalImage.Width, internalImage.Height), ImageLockMode.ReadOnly, internalImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(internalImage.PixelFormat) / 8;
                int byteCount = bitmapData.Stride * internalImage.Height;
                byte[] pixels = new byte[byteCount];
                IntPtr ptrFirstPixel = bitmapData.Scan0;
                Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;

                decimal pixelCounter = 0;
                decimal redCounter = 0;
                decimal greenCounter = 0;
                decimal blueCounter = 0;
                for (int y = 0; y < heightInPixels; y++)
                {
                    int currentLine = y * bitmapData.Stride;
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        blueCounter += pixels[currentLine + x];
                        greenCounter += pixels[currentLine + x + 1];
                        redCounter += pixels[currentLine + x + 2];
                        pixelCounter++;
                    }
                }

                // copy modified bytes back
                // Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
                internalImage.UnlockBits(bitmapData);
                redCounter = redCounter / pixelCounter;
                greenCounter = greenCounter / pixelCounter;
                blueCounter = blueCounter / pixelCounter;
                return Color.FromArgb(Convert.ToInt32(redCounter), Convert.ToInt32(greenCounter), Convert.ToInt32(blueCounter));
            }
        }

        public void InvertImage()
        {

            BitmapData bitmapData = internalImage.LockBits(new Rectangle(0, 0, internalImage.Width, internalImage.Height), ImageLockMode.ReadOnly, internalImage.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(internalImage.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * internalImage.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    pixels[currentLine + x] = Convert.ToByte(255 - pixels[currentLine + x]);
                    pixels[currentLine + x + 1] = Convert.ToByte(255 - pixels[currentLine + x + 1]);
                    pixels[currentLine + x + 2] = Convert.ToByte(255 - pixels[currentLine + x + 2]);
                }
            }

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            internalImage.UnlockBits(bitmapData);

        }

        public void AlterByValue(int Red, int Green, int Blue)
        {
            BitmapData bitmapData = internalImage.LockBits(new Rectangle(0, 0, internalImage.Width, internalImage.Height), ImageLockMode.ReadOnly, internalImage.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(internalImage.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * internalImage.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int Temp = pixels[currentLine + x];
                    Temp += Blue;
                    if (Temp > 255)
                        Temp = 255;
                    if (Temp < 0)
                        Temp = 0;
                    pixels[currentLine + x] = Convert.ToByte(Temp);

                    Temp = pixels[currentLine + x + 1];
                    Temp += Green;
                    if (Temp > 255)
                        Temp = 255;
                    if (Temp < 0)
                        Temp = 0;
                    pixels[currentLine + x + 1] = Convert.ToByte(Temp);

                    Temp = pixels[currentLine + x + 2];
                    Temp += Red;
                    if (Temp > 255)
                        Temp = 255;
                    if (Temp < 0)
                        Temp = 0;
                    pixels[currentLine + x + 2] = Convert.ToByte(Temp);
                }
            }

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            internalImage.UnlockBits(bitmapData);

        }

        public void AlterByPercent(decimal Red, decimal Green, decimal Blue)
        {
            Red = (Red / 100) * 255;
            Green = (Green / 100) * 255;
            Blue = (Blue / 100) * 255;
            AlterByValue(Convert.ToInt32(Red), Convert.ToInt32(Green), Convert.ToInt32(Blue));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    internalImage.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NetBitmap()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
