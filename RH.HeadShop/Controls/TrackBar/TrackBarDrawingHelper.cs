using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RH.HeadShop.Controls.TrackBar
{
    public sealed class TrackBarDrawingHelper
    {
        public static void DrawAquaPill(Graphics g, RectangleF drawRectF, Color drawColor, Orientation orientation)
        {
            var colorBlend = new ColorBlend();

            var color1 = OpacityMix(Color.White, SoftLightMix(drawColor, Color.Black, 100), 40);
            var color2 = OpacityMix(Color.White, SoftLightMix(drawColor, CreateColorFromRgb(64, 64, 64), 100), 20);
            var color3 = SoftLightMix(drawColor, CreateColorFromRgb(128, 128, 128), 100);
            var color4 = SoftLightMix(drawColor, CreateColorFromRgb(192, 192, 192), 100);
            var color5 = OverlayMix(SoftLightMix(drawColor, Color.White, 100), Color.White, 75);

            //			
            colorBlend.Colors = new[] { color1, color2, color3, color4, color5 };
            colorBlend.Positions = new[] { 0, 0.25f, 0.5f, 0.75f, 1 };
            var gradientBrush = orientation == Orientation.Horizontal ? new LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top - 1), new Point((int)drawRectF.Left, (int)drawRectF.Top + (int)drawRectF.Height + 1), color1, color5) :
                new LinearGradientBrush(new Point((int)drawRectF.Left - 1, (int)drawRectF.Top), new Point((int)drawRectF.Left + (int)drawRectF.Width + 1, (int)drawRectF.Top), color1, color5);
            gradientBrush.InterpolationColors = colorBlend;
            FillPill(gradientBrush, drawRectF, g);

            //
            color2 = Color.White;
            colorBlend.Colors = new[] { color2, color3, color4, color5 };
            colorBlend.Positions = new[] { 0, 0.5f, 0.75f, 1 };
            gradientBrush = orientation == Orientation.Horizontal ? new LinearGradientBrush(new Point((int)drawRectF.Left + 1, (int)drawRectF.Top), new Point((int)drawRectF.Left + 1, (int)drawRectF.Top + (int)drawRectF.Height - 1), color2, color5) :
                                                                    new LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top + 1), new Point((int)drawRectF.Left + (int)drawRectF.Width - 1, (int)drawRectF.Top + 1), color2, color5);
            gradientBrush.InterpolationColors = colorBlend;
            FillPill(gradientBrush, RectangleF.Inflate(drawRectF, -3, -3), g);

        }

        public static void DrawAquaPillSingleLayer(Graphics g, RectangleF drawRectF, Color drawColor, Orientation orientation)
        {
            var colorBlend = new ColorBlend();

            var color1 = drawColor;
            var color2 = ControlPaint.Light(color1);
            var color3 = ControlPaint.Light(color2);
            var color4 = ControlPaint.Light(color3);

            colorBlend.Colors = new[] { color1, color2, color3, color4 };
            colorBlend.Positions = new[] { 0, 0.25f, 0.65f, 1 };

            var gradientBrush = orientation == Orientation.Horizontal ? new LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top), new Point((int)drawRectF.Left, (int)drawRectF.Top + (int)drawRectF.Height), color1, color4) :
                new LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top), new Point((int)drawRectF.Left + (int)drawRectF.Width, (int)drawRectF.Top), color1, color4);
            gradientBrush.InterpolationColors = colorBlend;

            FillPill(gradientBrush, drawRectF, g);

        }

        public static void FillPill(Brush b, RectangleF rect, Graphics g)
        {
            if (rect.Width > rect.Height)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top, rect.Height, rect.Height));
                g.FillEllipse(b, new RectangleF(rect.Left + rect.Width - rect.Height, rect.Top, rect.Height, rect.Height));

                var w = rect.Width - rect.Height;
                var l = rect.Left + ((rect.Height) / 2);
                g.FillRectangle(b, new RectangleF(l, rect.Top, w, rect.Height));
                g.SmoothingMode = SmoothingMode.Default;
            }
            else if (rect.Width < rect.Height)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top, rect.Width, rect.Width));
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top + rect.Height - rect.Width, rect.Width, rect.Width));

                var t = rect.Top + (rect.Width / 2);
                var h = rect.Height - rect.Width;
                g.FillRectangle(b, new RectangleF(rect.Left, t, rect.Width, h));
                g.SmoothingMode = SmoothingMode.Default;
            }
            else if (rect.Width == rect.Height)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, rect);
                g.SmoothingMode = SmoothingMode.Default;
            }
        }

        #region Color Helper

        private static Color CreateColorFromRgb(int red, int green, int blue)
        {
            //Corect Red element
            var r = red;
            if (r > 255)
                r = 255;
            if (r < 0)
                r = 0;

            //Corect Green element
            var g = green;
            if (g > 255)
                g = 255;
            if (g < 0)
                g = 0;

            //Correct Blue Element
            var b = blue;
            if (b > 255)
                b = 255;
            if (b < 0)
                b = 0;

            return Color.FromArgb(r, g, b);
        }

        private static Color OpacityMix(Color blendColor, Color baseColor, int opacity)
        {
            int r1 = blendColor.R;
            int g1 = blendColor.G;
            int b1 = blendColor.B;
            int r2 = baseColor.R;
            int g2 = baseColor.G;
            int b2 = baseColor.B;
            var r3 = (int)(((r1 * ((float)opacity / 100)) + (r2 * (1 - ((float)opacity / 100)))));
            var g3 = (int)(((g1 * ((float)opacity / 100)) + (g2 * (1 - ((float)opacity / 100)))));
            var b3 = (int)(((b1 * ((float)opacity / 100)) + (b2 * (1 - ((float)opacity / 100)))));
            return CreateColorFromRgb(r3, g3, b3);
        }

        private static Color SoftLightMix(Color baseColor, Color blendColor, int opacity)
        {
            int r1 = baseColor.R;
            int g1 = baseColor.G;
            int b1 = baseColor.B;
            int r2 = blendColor.R;
            int g2 = blendColor.G;
            int b2 = blendColor.B;
            var r3 = SoftLightMath(r1, r2);
            var g3 = SoftLightMath(g1, g2);
            var b3 = SoftLightMath(b1, b2);
            return OpacityMix(CreateColorFromRgb(r3, g3, b3), baseColor, opacity);
        }

        private static Color OverlayMix(Color baseColor, Color blendColor, int opacity)
        {
            var r3 = OverlayMath(baseColor.R, blendColor.R);
            var g3 = OverlayMath(baseColor.G, blendColor.G);
            var b3 = OverlayMath(baseColor.B, blendColor.B);
            return OpacityMix(CreateColorFromRgb(r3, g3, b3), baseColor, opacity);
        }

        private static int SoftLightMath(int ibase, int blend)
        {
            var dbase = (float)ibase / 255;
            var dblend = (float)blend / 255;
            if (dblend < 0.5)
                return (int)(((2 * dbase * dblend) + (Math.Pow(dbase, 2)) * (1 - (2 * dblend))) * 255);

            return (int)(((Math.Sqrt(dbase) * (2 * dblend - 1)) + ((2 * dbase) * (1 - dblend))) * 255);
        }

        private static int OverlayMath(int ibase, int blend)
        {
            var dbase = (double)ibase / 255;
            var dblend = (double)blend / 255;
            if (dbase < 0.5)
                return (int)((2 * dbase * dblend) * 255);

            return (int)((1 - (2 * (1 - dbase) * (1 - dblend))) * 255);
        }

        #endregion

    }
}
