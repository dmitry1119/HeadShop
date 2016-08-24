using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadEditor.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RH.HeadEditor.Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex3d
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector4 Color;
        public Vector3 AutodotsTexCoord;
        public Vector3 OriginalPosition;
        public static readonly int Stride = Marshal.SizeOf(default(Vertex3d));

        public void ToStream(BinaryWriter bw)
        {
            Position.ToStream(bw);
            Normal.ToStream(bw);

            bw.Write(TexCoord.X);
            bw.Write(TexCoord.Y);

            bw.Write(Color.X);
            bw.Write(Color.Y);
            bw.Write(Color.Z);
            bw.Write(Color.W);

            AutodotsTexCoord.ToStream(bw);
            OriginalPosition.ToStream(bw);
        }
        public static Vertex3d FromStream(BinaryReader br)
        {
            var result = new Vertex3d();
            result.Position = Vector3Ex.FromStream(br);
            result.Normal = Vector3Ex.FromStream(br);

            result.TexCoord = new Vector2(br.ReadSingle(), br.ReadSingle());
            result.Color = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            result.AutodotsTexCoord = Vector3Ex.FromStream(br);
            result.OriginalPosition = Vector3Ex.FromStream(br);

            return result;
        }
    }

    public class TrinagleInfo
    {
        public Int32 TrinagleIndex = -1;
        public float U, V, W;

        public void ToStream(BinaryWriter bw)
        {
            bw.Write(TrinagleIndex);
            bw.Write(U);
            bw.Write(V);
            bw.Write(W);
        }
        public static TrinagleInfo FromStream(BinaryReader br)
        {
            var result = new TrinagleInfo();
            result.TrinagleIndex = br.ReadInt32();
            result.U = br.ReadSingle();
            result.V = br.ReadSingle();
            result.W = br.ReadSingle();

            return result;
        }
    }

    public class Point3d
    {
        public Dictionary<int, float> Weights = new Dictionary<int, float>();
        public Vector3 Position;
        public List<uint> Indices = null;
        public List<int> Nearests = new List<int>();
        public TrinagleInfo TextureTrinagleInfo = new TrinagleInfo();
        public TrinagleInfo ShapeTrinagleInfo = new TrinagleInfo();
        public TrinagleInfo ProfileShapeTrinagleInfo = new TrinagleInfo();

        public void ToStream(BinaryWriter bw)
        {
            bw.Write(Weights.Count);
            foreach (var weight in Weights)
            {
                bw.Write(weight.Key);
                bw.Write(weight.Value);
            }
            Position.ToStream(bw);

            bw.Write(Indices.Count);
            foreach (var ind in Indices)
                bw.Write(ind);
            bw.Write(Nearests.Count);
            foreach (var near in Nearests)
                bw.Write(near);

            TextureTrinagleInfo.ToStream(bw);
            ShapeTrinagleInfo.ToStream(bw);
            ProfileShapeTrinagleInfo.ToStream(bw);
        }
        public static Point3d FromStream(BinaryReader br)
        {
            var result = new Point3d();

            var cnt = br.ReadInt32();
            for (var i = 0; i < cnt; i++)
                result.Weights.Add(br.ReadInt32(), br.ReadSingle());
            result.Position = Vector3Ex.FromStream(br);

            cnt = br.ReadInt32();
            if (cnt != 0)
            {
                result.Indices = new List<uint>();
                for (var i = 0; i < cnt; i++)
                    result.Indices.Add(br.ReadUInt32());
            }
            cnt = br.ReadInt32();
            for (var i = 0; i < cnt; i++)
                result.Nearests.Add(br.ReadInt32());

            result.TextureTrinagleInfo = TrinagleInfo.FromStream(br);
            result.ShapeTrinagleInfo = TrinagleInfo.FromStream(br);
            result.ProfileShapeTrinagleInfo = TrinagleInfo.FromStream(br);

            return result;
        }
    }

    public class MeshUndoInfo
    {
        public Dictionary<int, Vector3> Points = new Dictionary<int, Vector3>();

        public MeshUndoInfo Clone()
        {
            var result = new MeshUndoInfo();

            foreach (var v in Points)
                result.Points.Add(v.Key, v.Value);
            return result;
        }
    }

    public enum HeadMeshType
    {
        Eyes,
        Lip,
        Face,
        Head
    }

    public class RenderMeshPart
    {
        #region Var
        public Guid Guid;
        public String Name
        {
            get;
            set;
        }
        public int IndexBuffer, VertexBuffer = 0, NumIndices;
        public int CountIndices
        {
            get;
            set;
        }
        public List<uint> Indices = new List<uint>();
        public Vertex3d[] Vertices = null;
        public List<Point3d> Points = new List<Point3d>();
        public int Texture = 0;
        public int TransparentTexture = 0;

        public string TextureName = string.Empty;
        public string TransparentTextureName = string.Empty;

        public Vector4 Color = Vector4.One;
        public HeadMeshType Type = HeadMeshType.Head;
        public Dictionary<Point3d, int> PointsIndices = new Dictionary<Point3d, int>(new VectorEqualityComparer());
        public Vertex3d[] BaseVertices = null;
        private readonly List<uint> baseIndices = new List<uint>();

        public bool IsLeftToRight = false;
        public bool IsBaseTexture;
        public bool IsShaped
        {
            get
            {
                return Type == HeadMeshType.Face || Type == HeadMeshType.Lip || Type == HeadMeshType.Head;
            }
        }
        #endregion

        #region Public

        public bool IsMirrored
        {
            get
            {
                return BaseVertices != null;
            }
        }

        public Vector2 GetCenter(bool? isLeft = null)
        {
            var center = Vector2.Zero;
            var count = 0.0f;
            if (isLeft == null)
            {
                foreach (var p in Points)
                {
                    count++;
                    center += p.Position.Xy;
                }
            }
            else
            {
                foreach (var p in Points)
                {
                    if ((isLeft.Value && p.Position.X < 0.0f) || (!isLeft.Value && p.Position.X > 0.0f))
                    {
                        count++;
                        center += p.Position.Xy;
                    }
                }
            }
            return center / count;
        }

        public void AttachShapePoints(List<ShapePoint> shapePoints)
        {
            if (!IsShaped)
                return;
            float k;
            var keys = new List<int>();
            for (var i = 0; i < Points.Count; i++)
            {
                var p = Points[i];
                p.Weights.Clear();
                keys.Clear();
                var summ = 0.0f;
                for (var j = 0; j < shapePoints.Count; j++)
                {
                    var sp = shapePoints[j];
                    if (sp.CheckContains(ref p.Position, out k))
                    {
                        summ += k;
                        p.Weights.Add(j, k);
                        sp.InsertPoint(Guid, i);
                        keys.Add(j);
                    }
                }
                if (summ > 1.0f)
                {
                    foreach (var key in keys)
                        p.Weights[key] /= summ;
                }
            }
        }

        public void UpdateNormals()
        {
            var normals = Normal.CalculateNormals(Vertices.Select(v => v.Position).ToList(), Indices);
            for (int i = 0; i < normals.Count; i++)
            {
                var v = Vertices[i];
                v.Normal = normals[i];
                Vertices[i] = v;
            }
            UpdateVertexBuffer();
        }

        public void UpdateProfileShape(ref TexturingInfo s)
        {
            foreach (var p in Points)
            {
                if (p.ProfileShapeTrinagleInfo.TrinagleIndex < 0)
                    continue;
                var ti = p.ProfileShapeTrinagleInfo.TrinagleIndex * 3;
                var v1 = s.Points[s.Indices[ti]].Value;
                var v2 = s.Points[s.Indices[ti + 1]].Value;
                var v3 = s.Points[s.Indices[ti + 2]].Value;
                p.Position.Z = p.ProfileShapeTrinagleInfo.U * v1.X + p.ProfileShapeTrinagleInfo.V * v2.X + p.ProfileShapeTrinagleInfo.W * v3.X;
                p.Position.Y = p.ProfileShapeTrinagleInfo.U * v1.Y + p.ProfileShapeTrinagleInfo.V * v2.Y + p.ProfileShapeTrinagleInfo.W * v3.Y;
                foreach (var i in p.Indices)
                {
                    var v = Vertices[i];
                    v.Position = p.Position;
                    Vertices[i] = v;
                }
            }
        }

        public void UpdateShape(ref TexturingInfo s)
        {
            foreach (var p in Points)
            {
                if (p.ShapeTrinagleInfo.TrinagleIndex < 0)
                    continue;
                var ti = p.ShapeTrinagleInfo.TrinagleIndex * 3;
                var v1 = s.Points[s.Indices[ti]].Value;
                var v2 = s.Points[s.Indices[ti + 1]].Value;
                var v3 = s.Points[s.Indices[ti + 2]].Value;
                p.Position.X = p.ShapeTrinagleInfo.U * v1.X + p.ShapeTrinagleInfo.V * v2.X + p.ShapeTrinagleInfo.W * v3.X;
                p.Position.Y = p.ShapeTrinagleInfo.U * v1.Y + p.ShapeTrinagleInfo.V * v2.Y + p.ShapeTrinagleInfo.W * v3.Y;
                foreach (var i in p.Indices)
                {
                    var v = Vertices[i];
                    v.Position = p.Position;
                    Vertices[i] = v;
                }
            }
            UpdateVertexBuffer();
        }

        public void UpdateTexCoords(ref TexturingInfo t)
        {

            foreach (var p in Points)
            {
                if (p.TextureTrinagleInfo.TrinagleIndex < 0)
                    continue;
                var ti = p.TextureTrinagleInfo.TrinagleIndex * 3;
                var v1 = t.TexCoords[t.Indices[ti]];
                var v2 = t.TexCoords[t.Indices[ti + 1]];
                var v3 = t.TexCoords[t.Indices[ti + 2]];
                foreach (var i in p.Indices)
                {
                    var v = Vertices[i];
                    v.AutodotsTexCoord.X = p.TextureTrinagleInfo.U * v1.X + p.TextureTrinagleInfo.V * v2.X + p.TextureTrinagleInfo.W * v3.X;
                    v.AutodotsTexCoord.Y = p.TextureTrinagleInfo.U * v1.Y + p.TextureTrinagleInfo.V * v2.Y + p.TextureTrinagleInfo.W * v3.Y;
                    if (IsBaseTexture)
                        v.TexCoord = v.AutodotsTexCoord.Xy;

                    Vertices[i] = v;
                }
            }
            UpdateVertexBuffer();
        }

        public void FillBlendingData(List<BlendingInfo> blendingInfos)
        {
            foreach (var p in Points)
            {
                float k = 0.0f;
                foreach (var b in blendingInfos)
                {
                    var length = (b.Position - p.Position.Xy).Length;
                    if (length < b.Radius)
                        k = 1.0f;
                    else if (length < (b.Radius + b.HalfRadius))
                        k = Math.Max(k, (1.0f - ((length - b.Radius) / b.HalfRadius)));
                }
                foreach (var i in p.Indices)
                    Vertices[i].AutodotsTexCoord.Z = k;
            }
        }

        public void FillPointsInfo(ref TexturingInfo t, bool isShape, bool isProfile)
        {
            for (var i = 0; i < t.Indices.Length; i += 3)
            {
                var a = t.Points[t.Indices[i]];
                var b = t.Points[t.Indices[i + 1]];
                var c = t.Points[t.Indices[i + 2]];
                for (var index = 0; index < Points.Count; index++)
                {
                    var point = Points[index];
                    var triangle = isShape ? isProfile ? point.ProfileShapeTrinagleInfo : point.ShapeTrinagleInfo : point.TextureTrinagleInfo;
                    if (triangle.TrinagleIndex > 0)
                        continue;
                    var p = Vertices[point.Indices[0]].OriginalPosition;
                    if (TexturingInfo.PointInTriangle(ref a.Value, ref b.Value, ref c.Value, isProfile ? p.Zy : p.Xy))
                    {
                        triangle.TrinagleIndex = i / 3;
                        var x = isProfile ? p.Z : p.X;
                        var aup = a.Value.X - x;
                        var bup = b.Value.X - x;
                        var cup = c.Value.X - x;
                        var avp = a.Value.Y - p.Y;
                        var bvp = b.Value.Y - p.Y;
                        var cvp = c.Value.Y - p.Y;

                        var f = 1.0f / ((b.Value.X - a.Value.X) * (c.Value.Y - a.Value.Y) - (b.Value.Y - a.Value.Y) * (c.Value.X - a.Value.X));
                        triangle.U = (bup * cvp - bvp * cup) * f;
                        triangle.V = (cup * avp - cvp * aup) * f;
                        triangle.W = 1.0f - (triangle.U + triangle.V);
                    }
                    Points[index] = point;
                }
            }
        }

        public void UndoMirror()
        {
            if (BaseVertices == null)
                return;
            Vertices = new Vertex3d[BaseVertices.Length];
            BaseVertices.CopyTo(Vertices, 0);
            BaseVertices = null;
            Indices.Clear();
            Indices.AddRange(baseIndices);
            CountIndices = Indices.Count;
            baseIndices.Clear();
            UpdateBuffers();
        }

        public void Mirror(bool leftToRight, float axis)
        {
            IsLeftToRight = leftToRight;
            UndoMirror();
            BaseVertices = new Vertex3d[Vertices.Length];
            Vertices.CopyTo(BaseVertices, 0);
            baseIndices.Clear();
            baseIndices.AddRange(Indices);

            var mirroredPoints = new SortedList<int, int>();
            var pointsMapping = new SortedList<int, uint>();
            var vertices = new List<Vertex3d>();
            var positions = new List<Vector3>();
            var delta = leftToRight ? axis + 0.00001f : axis - 0.00001f;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var vertex = Vertices[i];
                if (vertex.Position.X < delta == leftToRight)
                {
                    var index = vertices.Count;
                    pointsMapping.Add(i, (uint)index);
                    vertices.Add(new Vertex3d
                    {
                        Position = vertex.Position,
                        TexCoord = vertex.TexCoord,
                        OriginalPosition = new Vector3(i + 2, 0.0f, 0.0f),
                        Color = Vector4.One
                    });
                    positions.Add(vertices.Last().Position);

                    if (vertex.Position.X > -delta == leftToRight)
                        mirroredPoints.Add(index, index);
                    else
                    {
                        mirroredPoints.Add(index, vertices.Count);
                        vertices.Add(new Vertex3d
                        {
                            Position = new Vector3(-vertex.Position.X, vertex.Position.Y, vertex.Position.Z),
                            TexCoord = vertex.TexCoord,
                            OriginalPosition = new Vector3(-(i + 2), 0.0f, 0.0f),
                            Color = Vector4.One
                        });
                        positions.Add(vertices.Last().Position);
                    }
                }
            }

            var indices = new List<uint>();
            var linesMapping = new Dictionary<Line, int>(new VectorEqualityComparer());
            var lines = new[] { new Line(0, 0), new Line(0, 0) };
            float k = 0.0f;
            var idx = new int[2];
            for (int i = 0; i < Indices.Count; i += 3)
            {
                var triangle = Indices.GetRange(i, 3).Select(p => (int)p).ToArray();
                var count = triangle.Count(pointsMapping.ContainsKey);
                var centerIndex = -1;
                switch (count)
                {
                    case 3:
                        indices.AddRange(triangle.Select(t => pointsMapping[t]));
                        continue;
                    case 1:
                    case 2:
                        var c = count;
                        foreach (var t in triangle.Where(pointsMapping.ContainsKey))
                        {
                            var key = (int)pointsMapping[t];
                            if (mirroredPoints[key] == key)
                            {
                                centerIndex = t;
                                c--;
                            }
                        }
                        if (c == 0)
                            continue;
                        break;
                    case 0:
                        continue;
                }
                for (int j = 0; j < 3; j++)
                    if (pointsMapping.ContainsKey(triangle[j]) == (count == 1))
                    {
                        lines[0].A = triangle[j];
                        lines[0].B = triangle[(j + 1) % 3];
                        lines[1].A = triangle[(j + 2) % 3];
                        lines[1].B = triangle[j];
                    }
                idx[0] = linesMapping.ContainsKey(lines[0]) ? linesMapping[lines[0]] : -1;
                idx[1] = linesMapping.ContainsKey(lines[1]) ? linesMapping[lines[1]] : -1;
                for (int j = 0; j < idx.Length; j++)
                    if (idx[j] < 0)
                    {
                        var line = lines[j];
                        var v0 = Vertices[line.A];
                        var v1 = Vertices[line.B];
                        if (line.A == centerIndex || line.B == centerIndex)
                        {
                            count = 1;
                            centerIndex = (j + 1) % 2;
                        }
                        else
                        {
                            idx[j] = vertices.Count;
                            linesMapping.Add(line, vertices.Count);
                            mirroredPoints.Add(vertices.Count, vertices.Count);
                            vertices.Add(new Vertex3d
                            {
                                Position = Line.GetPoint(v0.Position, v1.Position, 0.0f, ref k),
                                OriginalPosition = new Vector3(k, line.A, line.B),
                                TexCoord = v0.TexCoord + (v1.TexCoord - v0.TexCoord) * k,
                                Color = Vector4.One
                            });
                            positions.Add(vertices.Last().Position);
                        }
                    }
                if (count == 2)
                {
                    indices.Add((uint)idx[0]);
                    indices.Add(pointsMapping[lines[0].B]);
                    indices.Add(pointsMapping[lines[1].A]);

                    indices.Add(pointsMapping[lines[1].A]);
                    indices.Add((uint)idx[1]);
                    indices.Add((uint)idx[0]);
                }
                else
                {
                    switch (centerIndex)
                    {
                        case -1:
                            indices.Add(pointsMapping[lines[0].A]);
                            indices.Add((uint)idx[0]);
                            indices.Add((uint)idx[1]);
                            break;
                        default:
                            indices.Add(pointsMapping[lines[0].B]);
                            indices.Add(pointsMapping[lines[1].A]);
                            indices.Add((uint)idx[centerIndex]);
                            break;
                    }
                }
            }
            var cnt = indices.Count;
            for (int i = 0; i < cnt; i += 3)
            {
                var triangle = indices.GetRange(i, 3).ToArray();
                for (int j = 2; j >= 0; j--)
                    indices.Add((uint)mirroredPoints[(int)triangle[j]]);
            }

            var normals = Normal.CalculateNormals(positions, indices);
            for (int i = 0; i < normals.Count; i++)
            {
                var v = vertices[i];
                v.Normal = normals[i];
                vertices[i] = v;
            }

            Indices = indices;
            CountIndices = indices.Count;
            Vertices = vertices.ToArray();
            UpdateBuffers();
        }

        public void UpdateBuffers()
        {
            UpdateIndexBuffer();
            UpdateVertexBuffer();
        }

        public void Undo(MeshUndoInfo info)
        {
            foreach (var p in info.Points)
            {
                var point = Points[p.Key];
                point.Position = p.Value;
                foreach (var idx in point.Indices)
                {
                    var vertex = Vertices[idx];
                    vertex.Position = point.Position;
                    Vertices[idx] = vertex;
                }
            }
            UpdateNormals();
        }

        public MeshUndoInfo GetUndoInfo()
        {
            var info = new MeshUndoInfo();
            for (int i = 0; i < Points.Count; i++)
                info.Points.Add(i, Points[i].Position);
            return info;
        }

        public bool Create(MeshPartInfo info)
        {
            if (info.VertexPositions.Count == 0)
                return false;
            Guid = Guid.NewGuid();
            Color = info.Color;
            Texture = info.Texture;
            TransparentTexture = info.TransparentTexture;
            TextureName = info.TextureName;
            TransparentTextureName = info.TransparentTextureName;

            //Name.Contains("SkinHead") || Name.Contains("SkinNeck")
            Name = info.PartName;
            if (Name.Contains("Pupil"))
                Type = HeadMeshType.Eyes;
            if (Name.Contains("SkinFace"))
                Type = HeadMeshType.Face;
            else
                if (Name.Contains("Lip"))
                    Type = HeadMeshType.Lip;

            Indices.Clear();
            var positions = new List<Vector3>();
            var texCoords = new List<Vector2>();

            var positionsDict = new Dictionary<VertexInfo, uint>(new VectorEqualityComparer());
            var pointnsDict = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            var pointsIndicesDict = new Dictionary<int, int>();
            Points.Clear();
            for (var i = 0; i < info.VertexPositions.Count; i++)
            {
                var vertexInfo = new VertexInfo
                {
                    Position = info.VertexPositions[i],
                    TexCoords = info.TextureCoords[i]
                };
                if (!positionsDict.ContainsKey(vertexInfo))
                {
                    var index = (uint)positions.Count;
                    positionsDict.Add(vertexInfo, index);
                    Indices.Add(index);
                    positions.Add(vertexInfo.Position);
                    texCoords.Add(vertexInfo.TexCoords);

                    if (!pointnsDict.ContainsKey(vertexInfo.Position))
                    {
                        pointnsDict.Add(vertexInfo.Position, Points.Count);
                        pointsIndicesDict.Add((int)index, Points.Count);
                        Points.Add(new Point3d
                        {
                            Indices = new List<uint> { index },
                            Position = vertexInfo.Position
                        });
                    }
                    else
                    {
                        var id = pointnsDict[vertexInfo.Position];
                        Points[id].Indices.Add(index);
                        pointsIndicesDict.Add((int)index, id);
                    }
                }
                else
                    Indices.Add(positionsDict[vertexInfo]);
            }

            CountIndices = Indices.Count;
            Vertices = new Vertex3d[positions.Count];

            var normals = Normal.CalculateNormals(positions, Indices);
            for (var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = positions[i];
                Vertices[i].OriginalPosition = positions[i];
                Vertices[i].Normal = normals[i];
                Vertices[i].TexCoord = texCoords[i];
                Vertices[i].AutodotsTexCoord = new Vector3(texCoords[i].X, texCoords[i].Y, 1.0f);
                Vertices[i].Color = Vector4.One;
            }
            FillPoints(pointsIndicesDict);
            Destroy();
            GL.GenBuffers(1, out VertexBuffer);
            GL.GenBuffers(1, out IndexBuffer);
            return true;
        }
        public void Destroy()
        {
            if (VertexBuffer != 0)
            {
                GL.DeleteBuffers(1, ref VertexBuffer);
                VertexBuffer = 0;
            }

            if (IndexBuffer != 0)
            {
                GL.DeleteBuffers(1, ref IndexBuffer);
                IndexBuffer = 0;
            }
        }

        public void ToStream(BinaryWriter bw)
        {
            bw.Write(Guid.ToString());
            bw.Write(Name);

            bw.Write(Indices.Count);
            foreach (var ind in Indices)
                bw.Write(ind);

            bw.Write(Vertices.Length);
            foreach (var vert in Vertices)
                vert.ToStream(bw);

            bw.Write(Points.Count);
            foreach (var point in Points)
                point.ToStream(bw);

            bw.Write(TextureName?? string.Empty);              // если isBase - нужно будет фотку текстуры подсунуть
            bw.Write(TransparentTextureName ?? string.Empty);

            Color.ToStream(bw);
            bw.Write((int)Type);
            bw.Write(IsBaseTexture);
        }
        public static RenderMeshPart FromStream(BinaryReader br)
        {
            var result = new RenderMeshPart();
            result.Guid = new Guid(br.ReadString());
            result.Name = br.ReadString();

            var cnt = br.ReadInt32();
            result.CountIndices = cnt;
            for (var i = 0; i < cnt; i++)
                result.Indices.Add(br.ReadUInt32());

            cnt = br.ReadInt32();
            if (cnt != 0)
            {
                result.Vertices = new Vertex3d[cnt];
                for (var i = 0; i < cnt; i++)
                    result.Vertices[i] = Vertex3d.FromStream(br);
            }

            cnt = br.ReadInt32();
            for (var i = 0; i < cnt; i++)
                result.Points.Add(Point3d.FromStream(br));

            result.TextureName = br.ReadString();
            result.TransparentTextureName = br.ReadString();

            result.Color = Vector4Ex.FromStream(br);
            result.Type = (HeadMeshType)br.ReadInt32();
            result.IsBaseTexture = br.ReadBoolean();

            result.Destroy();
            GL.GenBuffers(1, out  result.VertexBuffer);
            GL.GenBuffers(1, out  result.IndexBuffer);

            return result;
        }

        #endregion

        #region Private

        private void FillPoints(Dictionary<int, int> dictionary)
        {
            PointsIndices.Clear();
            for (var p = 0; p < Points.Count; p++)
            {
                var point = Points[p];
                PointsIndices.Add(point, p);
                var triangles = new List<int>();
                foreach (var i in point.Indices)
                {
                    for (int t = 0; t < Indices.Count; t++)
                        if (Indices[t] == i)
                            triangles.Add(t - t % 3);
                }
                point.Nearests.Clear();
                //ищем все соседние точки
                foreach (var t in triangles)
                {
                    for (var i = t; i < t + 3; i++)
                    {
                        var ti = (int)Indices[i];
                        var pt = dictionary[ti];
                        if (pt != p && !point.Nearests.Contains(pt))
                            point.Nearests.Add(pt);
                    }
                }
            }
        }

        private void UpdateVertexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * Vertex3d.Stride), Vertices, BufferUsageHint.StreamDraw);
            OpenGlHelper.CheckErrors();
        }
        private void UpdateIndexBuffer()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(CountIndices * sizeof(uint)), Indices.ToArray(), BufferUsageHint.DynamicDraw);
            OpenGlHelper.CheckErrors();
        }

        #endregion
    }

    public class RenderMeshParts : List<RenderMeshPart>
    {
        public bool Contains(Guid id)
        {
            foreach (var item in this)
            {
                if (item.Guid == id)
                    return true;
            }
            return false;
        }

        public RenderMeshPart this[Guid id]
        {
            get
            {
                return this.FirstOrDefault(item => item.Guid == id);
            }
        }
    }
}
