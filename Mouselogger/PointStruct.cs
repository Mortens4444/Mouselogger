using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Mouselogger
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointStruct
    {
        public int X;
        public int Y;

        public PointStruct(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(PointStruct p)
        {
            return new Point(p.X, p.Y);
        }

        public static implicit operator PointStruct(Point p)
        {
            return new PointStruct(p.X, p.Y);
        }

        public override string ToString()
        {
            return String.Format("X: {0}, Y: {1}", X, Y);
        }
    }
}
