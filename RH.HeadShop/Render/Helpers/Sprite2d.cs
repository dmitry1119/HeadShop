using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RH.HeadShop.Render.Helpers
{
    public class Sprite2D
    {
        public Vector2 Size
        {
            set
            {
                size = value * 0.5f;
                UpdatePoints();
            }
        }
        public Vector2 Position
        {
            set
            {
                position = value;
                UpdatePoints();
            }
        }
        public Vector4 Color = Vector4.One;
        public int Texture;

        private Vector2 size, position;
        private Vector2 leftBottom = Vector2.Zero, rightTop = Vector2.One;
        public Vector2 TexCoordLeftBottom = Vector2.Zero, TexCoordRightTop = Vector2.One;

        private void UpdatePoints()
        {
            leftBottom = position - size;
            rightTop = position + size;
        }

        public void Draw(bool profile = true, bool transparent = false)
        {
            if (transparent)
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            GL.Enable(EnableCap.Texture2D);
            GL.Color4(Color);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.Begin(PrimitiveType.Quads);
            if (profile)
            {
                GL.TexCoord2(TexCoordLeftBottom);
                GL.Vertex3(0.0f, leftBottom.Y, leftBottom.X);
                GL.TexCoord2(TexCoordRightTop.X, TexCoordLeftBottom.Y);
                GL.Vertex3(0.0f, leftBottom.Y, rightTop.X);
                GL.TexCoord2(TexCoordRightTop);
                GL.Vertex3(0.0f, rightTop.Y, rightTop.X);
                GL.TexCoord2(TexCoordLeftBottom.X, TexCoordRightTop.Y);
                GL.Vertex3(0.0f, rightTop.Y, leftBottom.X);
            }
            else
            {
                GL.TexCoord2(-TexCoordLeftBottom.X, TexCoordLeftBottom.Y);
                GL.Vertex2(leftBottom);
                GL.TexCoord2(-TexCoordRightTop.X, TexCoordLeftBottom.Y);
                GL.Vertex2(rightTop.X, leftBottom.Y);
                GL.TexCoord2(-TexCoordRightTop.X, TexCoordRightTop.Y);
                GL.Vertex2(rightTop);
                GL.TexCoord2(-TexCoordLeftBottom.X, TexCoordRightTop.Y);
                GL.Vertex2(leftBottom.X, rightTop.Y);
            }
            GL.End();
            if (transparent)
            {
                GL.Disable(EnableCap.Blend);
                GL.DepthMask(true);
            }
            GL.Disable(EnableCap.Texture2D);
        }

        public Sprite2D Clone()
        {
            var result = new Sprite2D();
            result.size = size;
            result.position = position;
            result.Texture = Texture;
            result.Color = Color;

            result.leftBottom = leftBottom;
            result.rightTop = rightTop;
            result.TexCoordLeftBottom = TexCoordLeftBottom;
            result.TexCoordRightTop = TexCoordRightTop;

            return result;
        }
    }
}