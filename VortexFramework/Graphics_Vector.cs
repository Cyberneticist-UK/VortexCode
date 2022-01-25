using System;
using System.Drawing;

namespace VortexLibrary
{
    public static class Graphic_Vector
    {
        static DoublePoint Location = new DoublePoint(0, 0);
        static int PenWidth = 1;
        static Color PenColor = Color.FromArgb(255, 255, 255);
        static Bitmap bmp = new Bitmap(4, 4);
        static Graphics gfx = Graphics.FromImage(bmp);

        // Width,Height;
        // SX,Y; - Start X,Y
        // Wx; - Width of line
        // RRGGBB; - colour
        // RW,H; - Rectangle Width,Height
        // LX,Y; - Line to X,Y
        // rW,H; - Hollow rectangle Width, height
        // CX,Y; - Solid Circle
        // cX,Y; - hollow circle
        // X; - clear bitmap to colour

        public static Bitmap ConvertVec(string Vector, double Scale)
        {
            string[] Commands = Vector.Split(new char[] { ';' });
            foreach (string command in Commands)
                ProcessCommand(command, Scale);
            return bmp;
        }

        public static void ProcessCommand(string command, double Scale)
        {
            if (command != "")
            {
                // Command is separated out, without a semi-colon :-
                if (command.IndexOf(",") == -1)
                {
                    // No comma so either X (clear screen), set colour, or width of the line:
                    if (command == "X")
                    {
                        gfx.Clear(PenColor);
                    }
                    else if (command[0] == 'W')
                    {
                        // Width of line:
                        if (Int32.TryParse((Convert.ToDouble(command.Substring(1)) / Scale).ToString(), out PenWidth) == false)
                        {
                            PenWidth = 1;
                        }
                    }
                    else
                    {
                        SetColour(command);
                    }
                }
                else
                {
                    string[] sections = command.Split(new char[] { ',' });
                    int First = 0;
                    int Second = Int32.Parse(sections[1]);
                    if (Int32.TryParse(sections[0], out First) == true)
                    {
                        if ((Convert.ToInt32(First / Scale) > 0) && (Convert.ToInt32(Second / Scale) > 0))
                        {
                            // Must be a width and height:-
                            bmp = new Bitmap(Convert.ToInt32(First / Scale), Convert.ToInt32(Second / Scale));
                            gfx = Graphics.FromImage(bmp);
                        }
                    }
                    else
                    {
                        if (sections[0][0].ToString().ToLower() == "t")
                        {
                            // T[R/M/L/r/m/l]X,Y; - Solid Triangle (Right hand, Middle, Left hand, Right upside down, middle upside, left upside
                            // t[R/M/L/r/m/l]X,Y; - Hollow Triangle
                            First = Int32.Parse(sections[0].Substring(2));
                            if (sections[0][0] == 'T')
                            {
                                switch (sections[0][1])
                                {
                                    case 'R':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'M':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + (First / 2)) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'L':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'r':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'm':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + (First / 2)) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'l':
                                        gfx.FillPolygon(new SolidBrush(PenColor), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                }
                            }
                            else if (sections[0][0] == 't')
                            {
                                switch (sections[0][1])
                                {
                                    case 'R':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'M':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + (First / 2)) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'L':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'r':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'm':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32((Location.X + (First / 2)) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;
                                    case 'l':
                                        gfx.DrawPolygon(new Pen(PenColor, PenWidth), new PointF[] { new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y / Scale))), new Point(Convert.ToInt32((Location.X + First) / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)), new Point(Convert.ToInt32(Location.X / Scale), Convert.ToInt32((Location.Y + Second) / Scale)) });
                                        break;

                                }
                            }
                        }
                        else
                        {
                            // SX,Y; - Start X,Y
                            // LX,Y; - Line to X,Y
                            // rW,H; - Hollow rectangle Width, height
                            // RW,H; - Rectangle Width,Height
                            // CX,Y; - Solid Circle
                            // cX,Y; - hollow circle
                            First = Int32.Parse(sections[0].Substring(1));
                            switch (sections[0][0])
                            {
                                case 'S':
                                    // Move start location:
                                    Location.X = Convert.ToDouble(First);
                                    Location.Y = Convert.ToDouble(Second);
                                    break;
                                case 'L':
                                    // Draw a Line
                                    gfx.DrawLine(new Pen(PenColor, PenWidth), new Point((Convert.ToInt32(Location.X / Scale)), Convert.ToInt32(Location.Y / Scale)), new Point((Convert.ToInt32((Location.X + First) / Scale)), Convert.ToInt32((Location.Y + Second) / Scale)));
                                    break;
                                case 'r':
                                    gfx.DrawRectangle(new Pen(PenColor, PenWidth), new Rectangle(new Point((Convert.ToInt32(Location.X / Scale)), Convert.ToInt32(Location.Y / Scale)), new Size(Convert.ToInt32(First / Scale), Convert.ToInt32(Second / Scale))));
                                    break;
                                case 'R':
                                    gfx.FillRectangle(new SolidBrush(PenColor), new Rectangle(new Point((Convert.ToInt32(Location.X / Scale)), Convert.ToInt32(Location.Y / Scale)), new Size(Convert.ToInt32(First / Scale), Convert.ToInt32(Second / Scale))));
                                    break;
                                case 'c':
                                    gfx.DrawEllipse(new Pen(PenColor, PenWidth), new Rectangle(new Point((Convert.ToInt32(Location.X / Scale)), Convert.ToInt32(Location.Y / Scale)), new Size(Convert.ToInt32(First / Scale), Convert.ToInt32(Second / Scale))));
                                    break;
                                case 'C':
                                    gfx.FillEllipse(new SolidBrush(PenColor), new RectangleF(new PointF((Convert.ToSingle(Location.X / Scale)), Convert.ToSingle(Location.Y / Scale)), new SizeF(Convert.ToSingle(First / Scale), Convert.ToSingle(Second / Scale))));
                                    break;
                            }
                        }
                        if (sections[0][0] != 'S')
                        {
                            //     Location.X += First / Scale;
                            //     Location.Y += Second / Scale;
                            Location.X += First;
                            Location.Y += Second;
                        }
                    }
                }
            }
        }

        public static void SetColour(string colour)
        {
            PenColor = Color.FromArgb(
            HexToInt(colour[0]) * 16 + HexToInt(colour[1]),
            HexToInt(colour[2]) * 16 + HexToInt(colour[3]),
            HexToInt(colour[4]) * 16 + HexToInt(colour[5]));
        }

        private static int HexToInt(char Hex)
        {
            switch (Hex)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'A':
                    return 10;
                case 'B':
                    return 11;
                case 'C':
                    return 12;
                case 'D':
                    return 13;
                case 'E':
                    return 14;
                case 'F':
                    return 15;
                default:
                    return 0;
            }
        }

    }

    public static class CompactVector
    {
        public static string Compact(string Code)
        {
            string[] Commands = Code.Split(new char[] { ';' });
            string Result = "";
            foreach (string command in Commands)
                Result += ProcessCommand(command);
            return Result;
        }

        private static string ProcessCommand(string command)
        {
            string actualCom = command.Substring(0, command.IndexOf("("));
            return "";
        }
    }

    public struct DoublePoint
    {
        public double X;
        public double Y;
        public DoublePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public DoublePoint(Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        public DoublePoint(Double x, Double y)
        {
            X = x;
            Y = y;
        }

        public bool TestLocation(DoublePoint Scale, DoublePoint Offset)
        {
            int outX, outY = 0;
            if (Int32.TryParse(((X / Scale.X) - Offset.X).ToString(), out outX) == true)
            {
                if (Int32.TryParse(((Y / Scale.Y) - Offset.Y).ToString(), out outY) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public PointF ToPointF()
        {
            return new PointF(Convert.ToSingle(X), Convert.ToSingle(Y));

        }

        public PointF ToPointF(DoublePoint Scale, DoublePoint Offset)
        {
            return new PointF(Convert.ToSingle((X / Scale.X) - Offset.X), Convert.ToSingle((Y / Scale.Y) - Offset.Y));

        }
    }
}
