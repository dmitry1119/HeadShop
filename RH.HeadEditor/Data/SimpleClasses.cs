using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadEditor.Helpers;

namespace RH.HeadEditor.Data
{
    public class BlendingInfo
    {
        private float radius = 0.0f;

        public float HalfRadius
        {
            get;
            private set;
        }

        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                HalfRadius = value * 0.5f;
                radius = value;
            }
        }

        public Vector2 Position;
    }

    public class RectangleAABB
    {
        public RectangleAABB()
        {
            A = new Vector3(99999.0f, 99999.0f, 99999.0f);
            B = new Vector3(-99999.0f, -99999.0f, -99999.0f);
        }

        private Vector3 a;
        public Vector3 A
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                UpdateSize();
            }
        }

        private Vector3 b;
        public Vector3 B
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
                UpdateSize();
            }
        }

        private Vector3 size = Vector3.Zero;
        public Vector3 Size
        {
            get
            {
                return size;
            }
        }

        public Vector3 Center
        {
            get
            {
                return (a + b) * 0.5f;
            }
        }

        private void UpdateSize()
        {
            size = b - a;
        }

        public void ToStream(BinaryWriter bw)
        {
            a.ToStream(bw);
            b.ToStream(bw);
        }

        public static RectangleAABB FromStream(BinaryReader br)
        {
            var result = new RectangleAABB();
            result.A = Vector3Ex.FromStream(br);
            result.B = Vector3Ex.FromStream(br);
            return result;
        }
    }

    public class ShapePoint
    {
        public static float POINT_RADIUS = 1.5f;
        public static float QUAD_POINT_RADIUS = POINT_RADIUS * POINT_RADIUS;
        public Vector2 Position;
        private readonly Dictionary<Guid, List<int>> indices = new Dictionary<Guid, List<int>>();
        public ShapePoint Prev = null, Next = null;
        private readonly bool isFront = true;

        public ShapePoint(bool front, ShapePoint p)
        {
            isFront = front;
            Prev = p;
            if (Prev != null)
                Prev.Next = this;
        }

        public void Move(int index, RenderMesh mesh, ref Vector2 delta)
        {
            var k = 0.0f;
            var delta3 = new Vector3(isFront ? delta.X : 0.0f, delta.Y, isFront ? 0.0f : delta.X);
            foreach (var part in mesh.Parts)
            {
                if (indices.ContainsKey(part.Guid))
                {
                    var list = indices[part.Guid];
                    foreach (var l in list)
                    {
                        var point = part.Points[l];
                        if (point.Weights.TryGetValue(index, out k))
                        {
                            point.Position += delta3 * k;
                            foreach (var id in point.Indices)
                            {
                                var v = part.Vertices[id];
                                v.Position = point.Position;
                                part.Vertices[id] = v;
                            }
                        }
                    }
                    part.UpdateBuffers();
                }
            }
        }

        public void InsertPoint(Guid partGuid, int pointIndex)
        {
            if (indices.ContainsKey(partGuid))
                indices[partGuid].Add(pointIndex);
            else
                indices.Add(partGuid, new List<int> { pointIndex });
        }

        private int CheckSegment(ref Vector3 p, ref Vector2 p0, ref Vector2 p1, ref float k)
        {
            var pp = new Vector2(isFront ? p.X : p.Z, p.Y);
            var v = p1 - p0;
            var w = pp - p0;
            var c1 = Vector2.Dot(w, v);
            if (c1 <= 0.0f)
                return 0;
            var c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
                return 0;
            var b = c1 / c2;
            var pb = p0 + b * v;
            var ls = (pb - pp).LengthSquared;
            if (ls > QUAD_POINT_RADIUS)
                return -1;
            k = 1.0f - (pb - pp).LengthSquared / QUAD_POINT_RADIUS;
            k *= 1.0f - (pb - p0).Length / (p1 - p0).Length;
            return 1;
        }

        public bool CheckContains(ref Vector3 p, out float k)
        {
            k = 0.0f;
            int c0 = 0, c1 = 0;
            float k0 = 0.0f, k1 = 0.0f;
            if (Prev != null)
                c0 = CheckSegment(ref p, ref Position, ref Prev.Position, ref k0);
            if (Next != null)
            {
                c1 = CheckSegment(ref p, ref Position, ref Next.Position, ref k1);
                if (c0 == 0 && c1 == -1)
                    c0 = -1;
                else
                {
                    if (c1 == 1)
                    {
                        switch (c0)
                        {
                            case 1:
                                k0 = (k0 + k1) * 0.5f;
                                break;
                            default:
                                c0 = 1;
                                k0 = k1;
                                break;
                        }
                    }
                }
            }

            switch (c0)
            {
                case 1:
                    k = k0;
                    return true;
                case -1:
                    return false;
            }

            var dy = Math.Abs(p.Y - Position.Y);
            if (dy > POINT_RADIUS)
                return false;
            var dx = Math.Abs((isFront ? p.X : p.Z) - Position.X);
            if (dx > POINT_RADIUS)
                return false;
            var qlen = dx * dx + dy * dy;
            if (qlen > QUAD_POINT_RADIUS)
                return false;
            k = 1.0f - qlen / QUAD_POINT_RADIUS;
            return true;
        }
    }

    public class TexturingInfo
    {
        public HeadPoints<HeadPoint> Points;
        public Vector2[] TexCoords;
        public Int32[] Indices;

        public void UpdatePointsInfo(Vector2 scale, Vector2 center)
        {
            if (Points != null)
            {
                foreach (HeadPoint point in Points)
                {
                    var p = point.Value - center;
                    p.X *= scale.X;
                    p.Y *= scale.Y;
                    point.Value = p + center;
                }
            }
        }

        public static bool PointInTriangle(ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector3 p)
        {
            return PointInTriangle(ref a, ref b, ref c, p.Xy);
        }
        public static bool PointInTriangle(ref Vector2 a, ref Vector2 b, ref Vector2 c, Vector2 p)
        {
            var n1 = (b.Y - a.Y) * (p.X - a.X) - (b.X - a.X) * (p.Y - a.Y);
            var n2 = (c.Y - b.Y) * (p.X - b.X) - (c.X - b.X) * (p.Y - b.Y);
            var n3 = (a.Y - c.Y) * (p.X - c.X) - (a.X - c.X) * (p.Y - c.Y);
            return (n1 <= 0.0f && n2 <= 0.0f && n3 <= 0.0f) || (n1 >= 0.0f && n2 >= 0.0f && n3 >= 0.0f);
        }

        public void ToStream(BinaryWriter bw)
        {
            if (Points == null)
                bw.Write(0);
            else
            {
                bw.Write(Points.Count);
                foreach (var point in Points)
                    point.ToStream(bw);
            }

            if (TexCoords == null)
                bw.Write(0);
            else
            {
                bw.Write(TexCoords.Length);
                foreach (var coord in TexCoords)
                {
                    bw.Write(coord.X);
                    bw.Write(coord.Y);
                }
            }

            if (Indices == null)
                bw.Write(0);
            else
            {
                bw.Write(Indices.Length);
                foreach (var index in Indices)
                    bw.Write(index);
            }
        }
        public static TexturingInfo FromStream(BinaryReader br)
        {
            var result = new TexturingInfo();

            var cnt = br.ReadInt32();
            result.Points = new HeadPoints<HeadPoint>();
            for (var i = 0; i < cnt; i++)
                result.Points.Add(HeadPoint.FromStream(br));

            cnt = br.ReadInt32();
            if (cnt != 0)
            {
                result.TexCoords = new Vector2[cnt];
                for (var i = 0; i < cnt; i++)
                {
                    var v = new Vector2(br.ReadSingle(), br.ReadSingle());
                    result.TexCoords[i] = v;
                }
            }

            cnt = br.ReadInt32();
            result.Indices = new int[cnt];
            for (var i = 0; i < cnt; i++)
                result.Indices[i] = br.ReadInt32();

            return result;
        }

        public TexturingInfo Clone()
        {
            var result = new TexturingInfo();
            result.Points = new HeadPoints<HeadPoint>();
            foreach (var point in Points)
                result.Points.Add(point.Clone());

            if (TexCoords != null)
            {
                result.TexCoords = new Vector2[TexCoords.Length];
                for (var i = 0; i < TexCoords.Length; i++)
                    result.TexCoords[i] = TexCoords[i];
            }

            if (Indices != null)
            {
                result.Indices = new Int32[Indices.Length];
                for (var i = 0; i < Indices.Length; i++)
                    result.Indices[i] = Indices[i];
            }

            return result;
        }
    }

    public class OpenGlHelper
    {
        static public void DrawAABB(Vector3 a, Vector3 b)
        {
            var c = new Vector3(a.X, b.Y, a.Z);
            var d = new Vector3(new Vector3(b.X, a.Y, a.Z));
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Begin(PrimitiveType.Lines);
            DrawLine(ref a, ref c);
            DrawLine(ref c, ref b);
            DrawLine(ref b, ref d);
            DrawLine(ref d, ref a);
            GL.End();
        }

        static public void DrawLine(ref Vector3 a, ref Vector3 b)
        {
            GL.Vertex3(a);
            GL.Vertex3(b);
        }

        static public void CheckErrors()
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }
        }
    }


    public class Normal
    {
        private Vector3 normal = Vector3.Zero;
        private float count = 0.0f;

        public Vector3 GetNormal()
        {
            return normal / count;
        }

        public void AddNormal(Vector3 n)
        {
            normal += n;
            count += 1.0f;
        }

        public static List<Vector3> CalculateNormals(List<Vector3> vertexPositions, List<uint> vertexIndices)
        {
            var result = new List<Vector3>();
            var normalsDict = new Dictionary<Vector3, Normal>(new VectorEqualityComparer());

            for (var i = 0; i < vertexIndices.Count / 3; i++)
            {
                var index = i * 3;
                var p0 = vertexPositions[(int)vertexIndices[index]];
                var p1 = vertexPositions[(int)vertexIndices[index + 1]];
                var p2 = vertexPositions[(int)vertexIndices[index + 2]];
                var n = GetNormal(ref p0, ref p1, ref p2);

                for (var j = 0; j < 3; j++)
                {
                    var pointIndex = vertexIndices[index + j];
                    var vp = vertexPositions[(int)pointIndex];
                    if (!normalsDict.ContainsKey(vp))
                        normalsDict.Add(vp, new Normal());
                    normalsDict[vp].AddNormal(n);
                }
            }

            foreach (var position in vertexPositions)
                result.Add(normalsDict[position].GetNormal());

            return result;
        }

        public static Vector3 GetNormal(ref Vector3 posA, ref Vector3 posB, ref Vector3 posC)
        {
            var edge1 = Vector3.Subtract(posB, posA);
            var edge2 = Vector3.Subtract(posC, posA);
            var normal = Vector3.Cross(edge1, edge2);
            normal.Normalize();
            return normal;
        }
    }

    public class Line
    {
        public int A, B;

        public Line(int a, int b)
        {
            A = a;
            B = b;
        }

        public static Vector3 GetPoint(Vector3 a, Vector3 b, float x, ref float k)
        {
            var y = a.Y + (x - a.X) * (b.Y - a.Y) / (b.X - a.X);
            var c = new Vector2(x, y);
            k = (c - a.Xy).Length / (b.Xy - a.Xy).Length;
            return a + (b - a) * k;
        }
    }

    public class MeshPartInfo
    {
        public List<Vector3> VertexPositions = new List<Vector3>();
        public List<Vector2> TextureCoords = new List<Vector2>();
        public List<uint> VertexIndices = new List<uint>();
        public String PartName;
        public Vector4 Color;
        public int Texture = 0;
        public int TransparentTexture = 0;
        public string TextureName;
        public string TransparentTextureName;

        public void Clear()
        {
            TextureCoords.Clear();
            VertexPositions.Clear();
            VertexIndices.Clear();
            PartName = String.Empty;
        }
    }

    public class VertexInfo
    {
        public Vector3 Position;
        public Vector2 TexCoords;
    }

    public class VectorEqualityComparer :
        IEqualityComparer<Vector3>,
        IEqualityComparer<Vector2>,
        IEqualityComparer<VertexInfo>,
        IEqualityComparer<Point3d>,
        IEqualityComparer<Line>
    {
        private const float Delta = 0.00001f;
        public bool Equals(Line a, Line b)
        {
            return (a.A == b.A && a.B == b.B) || (a.A == b.B && a.B == b.A);
        }

        public int GetHashCode(Line a)
        {
            return a.A * 10000 + a.B;
        }

        public static bool EqualsVector3(Vector3 a, Vector3 b)
        {
            return Math.Abs(a.X - b.X) < Delta && Math.Abs(a.Y - b.Y) < Delta && Math.Abs(a.Z - b.Z) < Delta;
        }

        public bool Equals(Point3d a, Point3d b)
        {
            return EqualsVector3(a.Position, b.Position);
        }
        public bool Equals(Vector3 a, Vector3 b)
        {
            return EqualsVector3(a, b);
        }
        public bool Equals(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) < Delta && Math.Abs(a.Y - b.Y) < Delta;
        }
        public bool Equals(VertexInfo a, VertexInfo b)
        {
            return EqualsVector3(a.Position, b.Position) && Equals(a.TexCoords, b.TexCoords);
        }

        public int GetHashCode(Vector3 a)
        {
            return (int)((a.X * a.X + a.Y * a.Y + a.Z * a.Z) * 10000);
        }
        public int GetHashCode(Vector2 a)
        {
            return (int)((a.X * a.X + a.Y * a.Y) * 10000);
        }
        public int GetHashCode(VertexInfo a)
        {
            return GetHashCode(a.Position) * GetHashCode(a.TexCoords);
        }
        public int GetHashCode(Point3d p)
        {
            return GetHashCode(p.Position);
        }
    }

    public static class Vector2Ex
    {
        public static void ToStream(this Vector2 v, BinaryWriter bw)
        {
            bw.Write(v.X);
            bw.Write(v.Y);
        }
        public static Vector2 FromStream(BinaryReader br)
        {
            var result = new Vector2(br.ReadSingle(), br.ReadSingle());
            return result;
        }
    }
    public static class Vector3Ex
    {
        public static void ToStream(this Vector3 v, BinaryWriter bw)
        {
            bw.Write(v.X);
            bw.Write(v.Y);
            bw.Write(v.Z);
        }
        public static Vector3 FromStream(BinaryReader br)
        {
            var result = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            return result;
        }
    }
    public static class Vector4Ex
    {
        public static void ToStream(this Vector4 v, BinaryWriter bw)
        {
            bw.Write(v.X);
            bw.Write(v.Y);
            bw.Write(v.Z);
            bw.Write(v.W);
        }
        public static Vector4 FromStream(BinaryReader br)
        {
            var result = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            return result;
        }
    }
}
