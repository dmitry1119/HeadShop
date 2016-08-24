using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RH.HeadShop.Helpers;

namespace RH.HeadShop.Controls
{
    public partial class ctrlAngleSelector : UserControlEx
    {
        #region Var

        private Rectangle drawRegion;
        private Point origin;

        private int angle;
        public int Angle
        {
            get { return angle; }
            set
            {
                angle = value;

                if (!DesignMode && OnAngleChanged != null)
                    OnAngleChanged(); //Raise event

                Refresh();
            }
        }

        public delegate void AngleChangedDelegate();
        public event AngleChangedDelegate OnAngleChanged;

        #endregion

        public ctrlAngleSelector()
        {
            InitializeComponent();
        }

        #region Form's event

        private void ctrlAngleSelector_Load(object sender, EventArgs e)
        {
            SetDrawRegion();
        }
        private void ctrlAngleSelector_SizeChanged(object sender, EventArgs e)
        {
            Height = Width; //Keep it a square
            SetDrawRegion();
        }

        private bool inMove;
        private void ctrlAngleSelector_MouseUp(object sender, MouseEventArgs e)
        {
            if (!inMove)
            {
                var thisAngle = FindNearestAngle(new Point(e.X, e.Y));
                if (thisAngle != -1)
                {
                    Angle = thisAngle;
                    Refresh();
                }
            }
            inMove = false;
        }
        private void ctrlAngleSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var thisAngle = FindNearestAngle(new Point(e.X, e.Y));

                if (thisAngle != -1)
                {
                    Angle = thisAngle;
                    Refresh();
                }
                inMove = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            var outline = new Pen(Color.FromArgb(86, 103, 141), 2.0f);
            var fill = new SolidBrush(Color.FromArgb(90, 255, 255, 255));

            var anglePoint = DegreesToXY(angle, origin.X - 2, origin);
            var originSquare = new Rectangle(origin.X - 1, origin.Y - 1, 3, 3);

            //Draw
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(outline, drawRegion);
            g.FillEllipse(fill, drawRegion);
            g.DrawLine(Pens.Black, origin, anglePoint);

            g.SmoothingMode = SmoothingMode.HighSpeed; //Make the square edges sharp
            g.FillRectangle(Brushes.Black, originSquare);

            fill.Dispose();
            outline.Dispose();

            base.OnPaint(e);
        }

        #endregion

        #region Supported void's

        private void SetDrawRegion()
        {
            drawRegion = new Rectangle(0, 0, Width, Height);
            drawRegion.X += 2;
            drawRegion.Y += 2;
            drawRegion.Width -= 4;
            drawRegion.Height -= 4;

            const int offset = 2;
            origin = new Point(drawRegion.Width / 2 + offset, drawRegion.Height / 2 + offset);

            Refresh();
        }

        private PointF DegreesToXY(float degrees, float radius, Point origin)
        {
            var xy = new PointF();
            var radians = degrees * Math.PI / 180.0;

            xy.X = (float)Math.Cos(radians) * radius + origin.X;
            xy.Y = (float)Math.Sin(-radians) * radius + origin.Y;

            return xy;
        }
        private float XYToDegrees(Point xy, Point origin)
        {
            var alphaAngle = 0.0;

            if (xy.Y <= origin.Y)
            {
                if (xy.X > origin.X)
                {
                    alphaAngle = (xy.X - origin.X) / (double)(origin.Y - xy.Y);
                    alphaAngle = Math.Atan(alphaAngle);
                    alphaAngle = 90.0 - alphaAngle * 180.0 / Math.PI;
                }
                else if (xy.X <= origin.X)
                {
                    alphaAngle = (origin.X - xy.X) / (double)(origin.Y - xy.Y);
                    alphaAngle = Math.Atan(-alphaAngle);
                    alphaAngle = 90.0 - alphaAngle * 180.0 / Math.PI;
                }
            }
            else if (xy.Y > origin.Y)
            {
                if (xy.X > origin.X)
                {
                    alphaAngle = (xy.X - origin.X) / (double)(xy.Y - origin.Y);
                    alphaAngle = Math.Atan(-alphaAngle);
                    alphaAngle = 270.0 - alphaAngle * 180.0 / Math.PI;
                }
                else if (xy.X <= origin.X)
                {
                    alphaAngle = (origin.X - xy.X) / (double)(xy.Y - origin.Y);
                    alphaAngle = Math.Atan(alphaAngle);
                    alphaAngle = 270.0 - alphaAngle * 180.0 / Math.PI;
                }
            }

            //       if (angle > 180) angle -= 360; //Optional. Keeps values between -180 and 180
            return (float)alphaAngle;
        }

        private int FindNearestAngle(Point mouseXY)
        {
            var thisAngle = (int)XYToDegrees(mouseXY, origin);
            if (thisAngle != 0)
                return thisAngle;
            return -1;
        }

        #endregion
       
    }
}
