using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadEditor.Helpers;

namespace RH.HeadEditor.Data
{
    public class RenderMesh
    {
        public delegate void BeforePartDrawHandler(RenderMeshPart part);
        public event BeforePartDrawHandler OnBeforePartDraw;
        public List<BlendingInfo> BlendingInfos = new List<BlendingInfo>();

        public RenderMeshParts Parts
        {
            get;
            set;
        }
        public RectangleAABB AABB = new RectangleAABB();
        public Vector2 Scale = Vector2.Zero;
        public Vector2 Center = Vector2.Zero;

        public RenderMesh()
        {
            Parts = new RenderMeshParts();
        }

        public float SetSize(float diagonal)
        {
            Vector2 a = new Vector2(99999.0f, 99999.0f), b = new Vector2(-99999.0f, -99999.0f);
            foreach (var part in Parts)
                foreach (var vertex in part.Vertices)
                {
                    a.X = Math.Min(vertex.Position.X, a.X);
                    a.Y = Math.Min(vertex.Position.Y, a.Y);
                    b.X = Math.Max(vertex.Position.X, b.X);
                    b.Y = Math.Max(vertex.Position.Y, b.Y);
                }
            float d = (b - a).Length;
            if (d == 0.0f)
                return 0.0f;
            float scale = diagonal / d;
            foreach (var part in Parts)
            {
                for (var i = 0; i < part.Vertices.Length; i++)
                {
                    var vertex = part.Vertices[i];
                    vertex.OriginalPosition = vertex.Position;
                    vertex.Position.X *= scale;
                    vertex.Position.Y *= scale;
                    vertex.Position.Z *= scale;
                    part.Vertices[i] = vertex;
                }
                foreach (var p in part.Points)
                {
                    var pos = p.Position;
                    pos.X *= scale;
                    pos.Y *= scale;
                    pos.Z *= scale;
                    p.Position = pos;
                }
            }
            return 1.0f / scale;
        }

        public Vector3 Transform(float k)
        {
            if (Parts.Count == 0)
                return Vector3.Zero;
            Scale.X = 1.0f;//(float)Math.Sqrt(Math.Abs(k * AABB.Size.Y / AABB.Size.X));
            Scale.Y = 1.0f / Scale.X;
            var center = Vector3.Zero;
            var count = 0.0f;
            foreach (var part in Parts)
                foreach (var v in part.Vertices)
                {
                    count += 1.0f;
                    center += v.Position;
                }
            center /= count;
            Center = center.Xy;
            foreach (var part in Parts)
            {
                for (var i = 0; i < part.Vertices.Length; i++)
                {
                    var vertex = part.Vertices[i];
                    vertex.OriginalPosition = vertex.Position;
                    vertex.Position -= center;
                    vertex.Position.X *= Scale.X;
                    vertex.Position.Y *= Scale.Y;
                    vertex.Position += center;
                    part.Vertices[i] = vertex;
                }
                foreach (var p in part.Points)
                {
                    var pos = p.Position;
                    pos -= center;
                    pos.X *= Scale.X;
                    pos.Y *= Scale.Y;
                    pos += center;
                    p.Position = pos;
                }
            }
            var a = AABB.A - center;
            a.X *= Scale.X;
            a.Y *= Scale.Y;
            AABB.A = a + center;
            var b = AABB.B - center;
            b.X *= Scale.X;
            b.Y *= Scale.Y;
            AABB.B = b + center;
            return center;
        }

        public void SetAABB(Vector2 leye, Vector2 reye, Vector2 lip, Vector2 face)
        {
            var a = AABB.A;
            var b = AABB.B;
            a.Y = Math.Min(Math.Min(leye.Y, lip.Y), reye.Y);
            b.Y = Math.Max(Math.Max(leye.Y, lip.Y), reye.Y);
            a.X = Math.Min(Math.Min(leye.X, lip.X), reye.X);
            b.X = Math.Max(Math.Max(leye.X, lip.X), reye.X);
            AABB.A = a;
            AABB.B = b;

            BlendingInfos.Clear();
            float radius = Math.Abs(leye.X - reye.X) * 0.5f;
            BlendingInfos.Add(new BlendingInfo
            {
                Position = leye,
                Radius = radius
            });
            BlendingInfos.Add(new BlendingInfo
            {
                Position = reye,
                Radius = radius
            });
            BlendingInfos.Add(new BlendingInfo
            {
                Position = AABB.Center.Xy,
                Radius = (AABB.Center.Xy - AABB.B.Xy).Length
            });
            BlendingInfos.Add(new BlendingInfo
            {
                Position = lip,
                Radius = (AABB.Center.Xy - lip).Length
            });
            BlendingInfos.Add(new BlendingInfo
            {
                Position = face,
                Radius = (AABB.B.Xy - face).Length * 0.6f
            });
            foreach (var part in Parts)
                part.FillBlendingData(BlendingInfos);
        }
        public void UpdateAABB(RenderMeshPart part, ref Vector3 a, ref Vector3 b)
        {
            foreach (var vertex in part.Vertices)
            {
                a.Z = Math.Min(vertex.Position.Z, a.Z);
                b.Z = Math.Max(vertex.Position.Z, b.Z);
            }
        }

        public void AddPart(RenderMeshPart part)
        {
            Parts.Add(part);
            var a = AABB.A;
            var b = AABB.B;
            UpdateAABB(part, ref a, ref b);
            AABB.A = a;
            AABB.B = b;
        }

        public void Draw(bool debug)
        {
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            foreach (var part in Parts)
            {
                if (OnBeforePartDraw != null)
                    OnBeforePartDraw(part);

                GL.BindBuffer(BufferTarget.ArrayBuffer, part.VertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.IndexBuffer);

                GL.VertexPointer(3, VertexPointerType.Float, Vertex3d.Stride, new IntPtr(0));
                GL.NormalPointer(NormalPointerType.Float, Vertex3d.Stride, new IntPtr(Vector3.SizeInBytes));
                // GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex3d.Stride, new IntPtr(2 * Vector3.SizeInBytes + Vector2.SizeInBytes + Vector4.SizeInBytes));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex3d.Stride, new IntPtr(2 * Vector3.SizeInBytes));
                GL.ColorPointer(4, ColorPointerType.Float, Vertex3d.Stride, new IntPtr(2 * Vector3.SizeInBytes + Vector2.SizeInBytes));

                GL.DrawRangeElements(PrimitiveType.Triangles, 0, part.CountIndices, part.CountIndices, DrawElementsType.UnsignedInt, new IntPtr(0));
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.ColorArray);

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);

            if (debug)
                OpenGlHelper.DrawAABB(AABB.A, AABB.B);
        }

        public void DrawToTexture(int newTextureId)
        {
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            foreach (var part in Parts)
            {
                if (part.Texture != newTextureId)
                    continue;

                GL.BindBuffer(BufferTarget.ArrayBuffer, part.VertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, part.IndexBuffer);

                GL.VertexPointer(2, VertexPointerType.Float, Vertex3d.Stride, new IntPtr(2 * Vector3.SizeInBytes)); //Как позицию используем основные текстурные координаты
                GL.NormalPointer(NormalPointerType.Float, Vertex3d.Stride, new IntPtr(0));//Как нормаль используем позиции (координата Z потребуется для вычисления смешивания 
                GL.TexCoordPointer(3, TexCoordPointerType.Float, Vertex3d.Stride, new IntPtr(2 * Vector3.SizeInBytes + Vector2.SizeInBytes + Vector4.SizeInBytes));//Как текстурные координаты берем дополнительные текстурные координаты

                GL.DrawRangeElements(PrimitiveType.Triangles, 0, part.CountIndices, part.CountIndices, DrawElementsType.UnsignedInt, new IntPtr(0));
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }

        public void Destroy()
        {
            foreach (var part in Parts)
            {
                part.Destroy();
            }
        }

        public void Save(string path)
        {
            using (var bw = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
            {
                AABB.ToStream(bw);
                Scale.ToStream(bw);
                Center.ToStream(bw);

                bw.Write(Parts.Count);
                foreach (var part in Parts)
                    part.ToStream(bw);
            }
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
                return;

            Parts.Clear();
            using (var br = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                AABB = RectangleAABB.FromStream(br);
                Scale = Vector2Ex.FromStream(br);
                Center = Vector2Ex.FromStream(br);

                var cnt = br.ReadInt32();
                for (var i = 0; i < cnt; i++)
                    Parts.Add(RenderMeshPart.FromStream(br));
            }
        }
    }
}
