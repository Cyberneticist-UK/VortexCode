using System.Drawing;

namespace VortexLibrary
{
    public class ConvolutionMatrix
    {
        public Size Size = new Size(3, 3);
        public int Width { get { return Size.Width; } }
        public int Height { get { return Size.Height; } }
        public int[,] Matrix = new int[3, 3] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
        public decimal divisor = 1;
    }
}
