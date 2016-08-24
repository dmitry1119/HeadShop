using System.Drawing;
using System.Drawing.Drawing2D;

namespace RH.HeadShop.Render.Helpers
{
    public static class DrawingTools
    {
        public static SolidBrush BlueSolidBrush = new SolidBrush(Color.Blue);
        public static SolidBrush GreenSolidBrush = new SolidBrush(Color.Green);
        public static SolidBrush GreenBrushTransparent80 = new SolidBrush(Color.FromArgb(80, Color.Green));
        public static SolidBrush RedSolidBrush = new SolidBrush(Color.Red);
        public static SolidBrush YellowSolidBrush = new SolidBrush(Color.Yellow);

        public static Pen GreenPen = new Pen(Color.Green);
        public static Pen RedPen = new Pen(Color.Red);
        public static Pen BluePen = new Pen(Color.Blue);

        public static Pen DashedRedPan = new Pen(Color.Red, 0.5f) { DashStyle = DashStyle.DashDot };
    }
}
