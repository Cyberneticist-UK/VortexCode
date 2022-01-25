using System;
using System.Drawing;

namespace VortexLibrary
{
    /// <summary>
    /// Summary description for ScreenCapture.
    /// </summary>
    public class ScreenCapture
    {
        public NetBitmap bitmap;
        IntPtr hWnd;
        Win32.Rect rc;
        bool Run = true;
        Graphics gfxBitmap;
        public ScreenCapture()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void PrepareWindow(IntPtr handle)
        {
            hWnd = handle;
            rc = new Win32.Rect();
            Run = Win32.GetWindowRect(hWnd, ref rc);
            // create a bitmap from the visible clipping bounds of the graphics object from the window
            if (rc.Width != 0 && rc.Height != 0)
            {
                bitmap = new NetBitmap(rc.Width, rc.Height);
            }
            else
                bitmap = new NetBitmap(10, 10);

            // create a graphics object from the bitmap
            gfxBitmap = Graphics.FromImage(bitmap.ToImage());
        }

        public void GetWindowCaptureAsBitmap(IntPtr handle)
        {
            if (hWnd != handle)
            {
                PrepareWindow(handle);
            }
            if (Run)
            {

                // get a device context for the bitmap
                IntPtr hdcBitmap = gfxBitmap.GetHdc();

                // get a device context for the window
                IntPtr hdcWindow = Win32.GetWindowDC(hWnd);

                // bitblt the window to the bitmap
                Win32.BitBlt(hdcBitmap, 0, 0, rc.Width, rc.Height, hdcWindow, 0, 0, (int)Win32.TernaryRasterOperations.SRCCOPY);

                // release the bitmap's device context
                gfxBitmap.ReleaseHdc(hdcBitmap);

                Win32.ReleaseDC(hWnd, hdcWindow);

                // dispose of the bitmap's graphics object
                // gfxBitmap.Dispose();

                // return the bitmap of the window
                // return bitmap;
            }
        }


    }
}
