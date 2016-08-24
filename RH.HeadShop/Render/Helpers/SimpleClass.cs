using System;
using System.Collections.Generic;
using OpenTK;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;
using RH.HeadEditor.Helpers;

namespace RH.HeadShop.Render.Helpers
{
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
    }

    public class ShapeMeshInfo
    {
        public List<int> Indices = new List<int>();
        public List<Vector3> Vertices = new List<Vector3>();
        public Dictionary<int, ShapePoint> Points = new Dictionary<int, ShapePoint>();
        public Matrix4 InverceTransform;

        public ShapeMeshInfo Clone()
        {
            var result = new ShapeMeshInfo();

            foreach (var v in Vertices)
                result.Vertices.Add(v);
            foreach (var i in Indices)
                result.Indices.Add(i);
            return result;
        }
    }

    /// <summary> Points for shape actions </summary>
    public class ShapePoint
    {
        public Vector3 Position;
        public Vector3 OriginalPosition;
        public Vector2 LocalPosition = Vector2.Zero;
        public float K;

        public ShapePoint Clone()
        {
            var result = new ShapePoint();
            result.Position = Position;
            result.LocalPosition = LocalPosition;
            result.K = K;

            return result;
        }
    }

    public class PointIndices
    {
        public Vector3 Position;
        public int[] Indices;
    }

    public class BoneLineInfo
    {
        public float Length;
        public Vector3 Point;
        public Vector3 Direction;
    }

    public class BoneInfo
    {
        public List<BoneLineInfo> Lines = new List<BoneLineInfo>();
        public Vector3 Point;
        public Matrix4 BoneOffset = Matrix4.Identity;
        public Matrix4 FinalTransformation = Matrix4.Identity;
    }

    public class AnimationInfo
    {
        public string Title;

        public string Sound = string.Empty;

        public bool IsPose;
        public AnimationInfo(string title, bool isPose)
        {
            Title = title;
            IsPose = isPose;
        }

        public float TicksPerSecond;
        public float DurationInTicks;
        public Dictionary<String, NodeAnimation> AnimationNodes = new Dictionary<string, NodeAnimation>();
    }

    public class VectorInfo
    {
        public float Time;
        public Vector3 Value;
    }

    public class QuaternionInfo
    {
        public float Time;
        public Assimp.Quaternion Value;
    }

    public class NodeAnimation
    {
        public int PositionKeysCount;
        public int RotationKeysCount;
        public int ScalingKeysCount;

        public VectorInfo[] PositionKeys;
        public QuaternionInfo[] RotationKeys;
        public VectorInfo[] ScalingKeys;
    }

    internal class AnimatedNode
    {
        public Matrix4 Transform = Matrix4.Identity;
        public String Name = "";
        public AnimatedNode Parent;
        public List<AnimatedNode> Childs = new List<AnimatedNode>();
    }

    public class VectorEqualityComparer : IEqualityComparer<Vector3>, IEqualityComparer<Vector2>
    {
        private const float Delta = 0.00001f;
        public bool Equals(Vector3 a, Vector3 b)
        {
            return Math.Abs(a.X - b.X) < Delta && Math.Abs(a.Y - b.Y) < Delta && Math.Abs(a.Z - b.Z) < Delta;
        }

        public bool Equals(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) < Delta && Math.Abs(a.Y - b.Y) < Delta;
        }

        public int GetHashCode(Vector3 a)
        {
            return (int)((a.X * a.X + a.Y * a.Y + a.Z * a.Z) * 10000);
        }
        public int GetHashCode(Vector2 a)
        {
            return (int)((a.X * a.X + a.Y * a.Y) * 10000);
        }
    }

    public class MeshInfo
    {
        public String Title = String.Empty;
        public ObjMaterial Material;

        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector2> TexCoords = new List<Vector2>();

        public List<int> IndicesPositions = new List<int>();
        public List<int> IndicesNormals = new List<int>();
        public List<int> IndicesTexCoords = new List<int>();

        public MeshInfo(RenderMeshPart part)
        {
            var positionsMapping = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            var texCoordsMapping = new Dictionary<Vector2, int>(new VectorEqualityComparer());
            var normalsMapping = new Dictionary<Vector3, int>(new VectorEqualityComparer());

            foreach (var index in part.Indices)
            {
                var vertex = part.Vertices[index];
                var position = vertex.Position;
                var texCoord = vertex.TexCoord;
                texCoord.Y = 1.0f - texCoord.Y;
                var normal = vertex.Normal;

                int id;
                if (!positionsMapping.TryGetValue(position, out id))
                {
                    id = Positions.Count;
                    Positions.Add(position);
                    positionsMapping.Add(position, id);
                }
                IndicesPositions.Add(id);

                if (!normalsMapping.TryGetValue(normal, out id))
                {
                    id = Normals.Count;
                    Normals.Add(normal);
                    normalsMapping.Add(normal, id);
                }
                IndicesNormals.Add(id);

                if (!texCoordsMapping.TryGetValue(texCoord, out id))
                {
                    id = TexCoords.Count;
                    TexCoords.Add(texCoord);
                    texCoordsMapping.Add(texCoord, id);
                }
                IndicesTexCoords.Add(id);
            }

            Title = part.Name;

            //
            Material = new ObjMaterial(part.Guid.ToString().Replace("-", ""));
            Material.DiffuseColor = part.Color;
            Material.Texture = part.Texture;
            Material.DiffuseTextureMap = ProgramCore.MainForm.ctrlRenderControl.GetTexturePath(part.Texture);
            Material.TransparentTexture = part.TransparentTexture;
            Material.TransparentTextureMap = ProgramCore.MainForm.ctrlRenderControl.GetTexturePath(part.TransparentTexture);
        }

        public MeshInfo(DynamicRenderMesh parent, Vertex[] vertices, Matrix4 transformMatrix)
        {
            Material = parent.Material;
            Title = parent.Title;
            var positionsMapping = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            var texCoordsMapping = new Dictionary<Vector2, int>(new VectorEqualityComparer());
            var normalsMapping = new Dictionary<Vector3, int>(new VectorEqualityComparer());

            foreach (var vertex in vertices)
            {
                // var index = i * 3;
                var position = vertex.Position;

                var texCoord = vertex.TexCoord;
                texCoord.Y = 1.0f - texCoord.Y;

                var normal = vertex.Normal;

                if (transformMatrix != Matrix4.Zero)
                {
                    var scaleCoef = 1f;
                    if (parent.meshType == MeshType.Accessory)
                        scaleCoef = 246f;
                    else if (parent.meshType == MeshType.Head)
                        scaleCoef = PickingController.GetHeadScale(ProgramCore.Project.ManType);
                    else
                        scaleCoef = PickingController.GetHairScale(ProgramCore.Project.ManType);
                    bool useExporter = ProgramCore.PluginMode &&
                    ProgramCore.MainForm.ctrlRenderControl.pickingController.ObjExport != null;
                    var invScale = Matrix4.CreateScale(useExporter ? 1.0f : 1.0f / scaleCoef);
                    var tempTransform = transformMatrix;
                    tempTransform *= invScale;
                    if (useExporter)
                    {
                        var d = ProgramCore.MainForm.ctrlRenderControl.pickingController.ObjExport.Delta;
                        d.Y -= 0.0060975609f;
                        tempTransform *=
                           Matrix4.CreateTranslation(d * scaleCoef);
                    }

                    position = Vector3.Transform(position, tempTransform);
                    normal = Vector3.Transform(normal, tempTransform);
                }

                int id;
                if (!positionsMapping.TryGetValue(position, out id))
                {
                    id = Positions.Count;
                    Positions.Add(position);
                    positionsMapping.Add(position, id);
                }
                IndicesPositions.Add(id);

                if (!normalsMapping.TryGetValue(normal, out id))
                {
                    id = Normals.Count;
                    Normals.Add(normal);
                    normalsMapping.Add(normal, id);
                }
                IndicesNormals.Add(id);

                if (!texCoordsMapping.TryGetValue(texCoord, out id))
                {
                    id = TexCoords.Count;
                    TexCoords.Add(texCoord);
                    texCoordsMapping.Add(texCoord, id);
                }
                IndicesTexCoords.Add(id);
            }
        }
    }

    #region Cutting

    public enum CollisionType
    {
        CT_NONE = 0,
        CT_VERTEX = 1,
        CT_EDGE = 2,
        CT_INSIDE = 3,
    }

    public struct CollisionInfo
    {
        public CollisionType Type;
        public int PointIndex;
        public Vector3 Position;
        public Vector2 TexCoord;
        public int EdgeIndex;

        public static CollisionInfo Zero = new CollisionInfo
        {
            PointIndex = -1,
            Type = CollisionType.CT_NONE,
            EdgeIndex = -1,
            Position = Vector3.Zero
        };
    }

    public class SlicePoint
    {
        public Vector2 Coordinate = Vector2.Zero;
        public List<SliceLine> Lines = new List<SliceLine>();
        public List<int> Indices = new List<int>();
        public List<float> Directions = new List<float>();

        public static float GetAngle(ref Vector2 dir)
        {
            return dir.Y > 0.0f ? (float)Math.Acos(dir.X) : -(float)Math.Acos(dir.X);
        }

        public void PreparePoint()
        {
            Directions.Clear();
            if (Lines.Count == 1)
            {
                Directions.Add(GetAngle(ref Lines[0].Line.Direction));
                var invDir = -Lines[0].Line.Direction;
                Directions.Add(GetAngle(ref invDir));
            }
            else
            {
                foreach (var line in Lines)
                {
                    var dir = SliceController.PointsCompare.Equals(Coordinate, line.Line.Point0) ? line.Line.Direction : -line.Line.Direction;
                    Directions.Add(GetAngle(ref dir));
                }
            }
            Directions.Sort();
        }
    }

    public class SliceLine
    {
        public SlicePoint Point0, Point1;
        public Line2d Line;
    }

    public class Line2d
    {
        public Vector2 Point0 = Vector2.Zero, Point1 = Vector2.Zero;
        public Vector2 A = Vector2.Zero, B = Vector2.Zero;
        public Vector2 Direction;

        public void UpdateDirection()
        {
            Direction = (Point1 - Point0).Normalized();
        }
    }


    public class Triangle
    {
        public int[] Indices = new int[3];

        public bool TriangleConnection(ref Triangle t)
        {
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                    if (t.Indices[i] == Indices[j])
                        return true;
            return false;
        }

        public static int TriangleEquals(ref Triangle a, ref Triangle b)
        {
            if (a.Indices[1] == -1)
            {
                for (var i = 0; i < 3; i++)
                    if (b.Indices[i] == a.Indices[0])
                        return i;
                return -1;
            }
            for (var i = 0; i < 3; i++)
                if (a.Indices[i] != b.Indices[i])
                    return -1;
            return 0;
        }
    }

    public class TrianleConnections
    {
        public Triangle Triangle;
        public List<int> Connections = new List<int>();
    }

    #endregion
}
