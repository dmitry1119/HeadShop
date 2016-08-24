using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RH.HeadShop.Render
{
    public class Camera
    {
        #region Var

        private const double m_pi = 3.14159265358979323846;

        public Vector3 Position
        {
            get;
            set;
        }
        public Matrix4 ViewMatrix
        {
            get;
            private set;
        }
        public Matrix4 ProjectMatrix
        {
            get;
            private set;
        }
        public int WindowWidth
        {
            get;
            private set;
        }
        public int WindowHeight
        {
            get;
            private set;
        }

        public double Radius;
        public double beta;

        #endregion

        public Camera()
        {
            InitCamera();
        }

        public void ResetCamera(bool updateScale)
        {
            if (updateScale)
            {
                Scale = 0.06772818f;
                UpdateDy();
                Wheel(0);
            }

            InitCamera(1.5707f, 500);
        }

        private void InitCamera()            // default initialize
        {
            InitCamera(1.5707f, 500);
        }

        public void InitCamera(double _beta, double _radius)
        {
            beta = _beta;
            while (beta < 0)
            {
                beta += m_pi * 2.0;
            }
            while (beta >= m_pi * 2)
            {
                beta -= m_pi * 2.0;
            }
            Radius = Math.Max(_radius, 0.1f);

            Position = new Vector3((float)(Radius * Math.Cos(beta)), (float)(Radius), (float)(Radius * Math.Sin(beta)));
        }

        public void SetupCamera(Vector2 dataLeft, Vector2 dataRight)
        {
            var l = dataRight.X * dataLeft.X;
            Scale = (float)Math.Sqrt(l * l / (WindowWidth * WindowWidth + WindowHeight * WindowHeight));
            var h = dataLeft.Y * Scale * WindowHeight;
            var y0 = dataRight.Y - h;
            var y1 = y0 + Scale * WindowHeight;
            dy = (y0 + y1) * 0.5f;
            UpdateViewport(WindowWidth, WindowHeight);
            PutCamera();
        }

        public float Scale = 0.06772818f;
        public void UpdateViewport(int width, int hegiht)
        {
            WindowWidth = width;
            WindowHeight = hegiht;

            GL.Viewport(0, 0, WindowWidth, WindowHeight);

            var orhto = Matrix4.CreateOrthographic(WindowWidth * Scale, WindowHeight * Scale, 0.0f, 10000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref orhto);
            GL.MatrixMode(MatrixMode.Modelview);
            ProjectMatrix = orhto;
        }

        public void UpdateDy()
        {
            dy = 0.0f;
        }

        public float dy = 0.0f;
        public void PutCamera()
        {
            Position = new Vector3((float)(Radius * Math.Cos(beta)), 0, (float)(Radius * Math.Sin(beta)));
            var lookat = Matrix4.LookAt(Position.X, Position.Y + dy, Position.Z, 0, dy, 0,
                                           (float)(Math.Cos(m_pi / 2) * Math.Cos(beta + m_pi)), (float)(Math.Sin(m_pi * 0.5f)), (float)(Math.Cos(m_pi * 0.5f) * Math.Sin(beta + m_pi)));

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            ViewMatrix = lookat;
        }

        public void StepTop()
        {

        }

        public void LeftRight(double delta)
        {
            beta += delta;
            while (beta < 0)
            {
                beta += m_pi * 2.0;
            }
            while (beta >= m_pi * 2.0)
            {
                beta -= m_pi * 2.0;
            }
        }

        public void Wheel(float delta1)
        {
            Scale += delta1;
            if (Scale < 0.01)
            {
                Scale = 0.01f;
                return;
            }
            if (Scale > 5)
            {
                Scale = 5;
                return;
            }

            var orhto = Matrix4.CreateOrthographic(WindowWidth * Scale, WindowHeight * Scale, 0.0f, 10000.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref orhto);
            GL.MatrixMode(MatrixMode.Modelview);
            ProjectMatrix = orhto;
        }

        public Vector3 GetWorldPoint(int screenX, int screenY, int screenWidth, int screenHeight, float depth)
        {
            var x = (screenX * 2.0f / screenWidth) - 1.0f;
            var y = 1.0f - (screenY * 2.0f / screenHeight);
            var p = new Vector3(x, y, -1.0f);

            var viewProj = ViewMatrix * ProjectMatrix;
            var invViewProj = viewProj.Inverted();

            p = Vector3.Transform(p, invViewProj);

            var dir = new Vector3(Position.X, 0.0f, Position.Z);
            dir.Normalize();
            var length = Vector3.Dot(dir, p);
            return p - dir * (length - depth);
        }
    }
}
