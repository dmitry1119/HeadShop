using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Threading.Tasks;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Obj;
using System.Runtime.InteropServices;

namespace RH.HeadShop.Render.Meshes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector4 Color;
        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }

    public class BaseRenderMesh
    {
        #region Var

        public readonly List<PointIndices> points = new List<PointIndices>();
        protected int IndexBuffer, VertexBuffer = 0, NumIndices;

        public ObjMaterial Material;

        public float TextureSize = 1;
        public float TextureAngle;

        public string groupName;

        public Matrix4 Transform = Matrix4.Identity;

        public Vertex[] vertexArray = null;
        protected Vertex[] vertexAnimatedArray = null;

        public float[] vertexBoneIndicsArray;
        public float[] vertexBoneWeightArray;


        public string Title;
        public bool IsVisible = true;
        public Guid Id;

        #endregion

        public BaseRenderMesh()
        {
            Id = Guid.NewGuid();
            Material = new ObjMaterial(Id.ToString().Replace("-", ""));
        }

        public List<Vector3> GetVertices()
        {
            var result = new List<Vector3>();
            foreach(var vertex in vertexArray)
                result.Add(vertex.Position);
            return result;
        }
        public List<Vector3> GetNormals()
        {
            var result = new List<Vector3>();
            foreach (var vertex in vertexArray)
                result.Add(vertex.Normal);
            return result;
        }
        public List<Vector2> GetTexCoords()
        {
            var result = new List<Vector2>();
            foreach (var vertex in vertexArray)
                result.Add(vertex.TexCoord);
            return result;
        }

        public void Draw(ShaderController shader)
        {
            if (!IsVisible)
                return;

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, Material.TransparentTexture);
            shader.UpdateUniform("u_TransparentMap", 1);
            shader.UpdateUniform("u_UseTransparent", (float)Material.TransparentTexture);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Material.Texture);
            shader.UpdateUniform("u_Texture", 0);
            shader.UpdateUniform("u_Color", Material.DiffuseColor);
            shader.UpdateUniform("u_UseTexture", (float)Material.Texture);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

            GL.VertexPointer(3, VertexPointerType.Float, Vertex.Stride, new IntPtr(0));
            GL.NormalPointer(NormalPointerType.Float, Vertex.Stride, new IntPtr(Vector3.SizeInBytes));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Stride, new IntPtr(2 * Vector3.SizeInBytes));
            GL.ColorPointer(4, ColorPointerType.Float, Vertex.Stride, new IntPtr(2 * Vector3.SizeInBytes + Vector2.SizeInBytes));

            GL.DrawRangeElements(PrimitiveType.Triangles, 0, NumIndices, NumIndices, DrawElementsType.UnsignedInt, new IntPtr(0));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.ColorArray);
        }

        public static Vector3 GetNormal(ref Vector3 posA, ref Vector3 posB, ref Vector3 posC)
        {
            var edge1 = Vector3.Subtract(posB, posA);
            var edge2 = Vector3.Subtract(posC, posA);
            var normal = Vector3.Cross(edge1, edge2);
            normal.Normalize();
            return normal;
        }

        protected List<float> GetNormals(List<Vector3> vertexPositions, List<int> vertexIndices)
        {
            var normals = new List<float>();
            var normalsDict = new Dictionary<Vector3, Normal>(new VectorEqualityComparer());

            for (var i = 0; i < vertexIndices.Count / 3; i++)
            {
                var index = i * 3;

                var p0 = vertexPositions[vertexIndices[index]];
                var p1 = vertexPositions[vertexIndices[index + 1]];
                var p2 = vertexPositions[vertexIndices[index + 2]];
                var n = GetNormal(ref p0, ref p1, ref p2);

                for (var j = 0; j < 3; j++)
                {
                    var pointIndex = vertexIndices[index + j];
                    var vp = vertexPositions[pointIndex];
                    if (!normalsDict.ContainsKey(vp))
                        normalsDict.Add(vp, new Normal());
                    normalsDict[vp].AddNormal(n);
                }
            }

            for (var i = 0; i < vertexIndices.Count; i++)
            {
                var normal = normalsDict[vertexPositions[vertexIndices[i]]].GetNormal();
                normals.AddRange(new[] { normal.X, normal.Y, normal.Z });
            }
            return normals;
        }

        public bool Create(List<float> vertexPositions,
            List<float> vertexTextureCoordinates, List<float> vertexBoneIndices, List<float> vertexBoneWeights,
            List<uint> vertexIndices, string texturePath, string alphaTexturePath)
        {
            var vertices = new List<Vector3>();
            for (var i = 0; i < vertexPositions.Count / 3; i++)
                vertices.Add(new Vector3(vertexPositions[i*3], vertexPositions[i*3 + 1], vertexPositions[i*3 + 2]));
            var indices = new List<int>();
            foreach(var i in vertexIndices)
                indices.Add((int)i);
            var normals = GetNormals(vertices, indices);
            return Create(vertexPositions, normals, vertexTextureCoordinates, vertexBoneIndices, vertexBoneWeights, vertexIndices, texturePath, alphaTexturePath);
        }

        public bool Create(List<float> vertexPositions, List<float> vertexNormals,
            List<float> vertexTextureCoordinates, List<float> vertexBoneIndices, List<float> vertexBoneWeights,
            List<uint> indices, string texturePath, string alphaTexturePath)
        {
            Material.DiffuseTextureMap = texturePath;
            Material.TransparentTextureMap = alphaTexturePath;
            NumIndices = indices.Count;

            GL.GenBuffers(1, out VertexBuffer);
            GL.GenBuffers(1, out IndexBuffer);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indices.Count * sizeof(uint)), indices.ToArray(), BufferUsageHint.DynamicDraw);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }

            vertexArray = new Vertex[vertexPositions.Count / 3];
            for (var i = 0; i < vertexPositions.Count / 3; i++)
            {
                var index = i * 3;
                vertexArray[i].Position = new Vector3(vertexPositions[index], vertexPositions[index + 1], vertexPositions[index + 2]);
                vertexArray[i].Normal = new Vector3(vertexNormals[index], vertexNormals[index + 1], vertexNormals[index + 2]);
                vertexArray[i].TexCoord = new Vector2(vertexTextureCoordinates[i * 2], 1.0f - vertexTextureCoordinates[i * 2 + 1]);
                vertexArray[i].Color = Vector4.One;
            }

            vertexBoneIndicsArray = vertexBoneIndices.ToArray();
            vertexBoneWeightArray = vertexBoneWeights.ToArray();

            UpdateBuffer();
            UpdatePointIndices();

            return true;
        }

        protected void UpdateBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexArray.Length * Vertex.Stride), vertexArray, BufferUsageHint.StreamDraw);
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }
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

        public void UpdatePointIndices()
        {
            points.Clear();
            var positions = new Dictionary<Vector3, List<int>>(new VectorEqualityComparer());
            for (var i = 0; i < vertexArray.Length; i++)
            {
                var pos = vertexArray[i].Position;
                List<int> list;
                if (positions.TryGetValue(pos, out list))
                    list.Add(i);
                else
                    positions.Add(pos, new List<int> { i });
            }

            foreach (var p in positions)
            {
                points.Add(new PointIndices
                {
                    Indices = p.Value.ToArray(),
                    Position = p.Key
                });
            }
        }

        public void BeginAnimate()
        {
            if (vertexAnimatedArray == null)
                vertexAnimatedArray = new Vertex[vertexArray.Length];
            for (var i = 0; i < vertexArray.Length; i++)
                vertexAnimatedArray[i].TexCoord = vertexArray[i].TexCoord;
        }

        public void EndAnimate()
        {
            UpdateBuffer();
        }

        public void AttachMeshes(DynamicRenderMeshes meshes)
        {
            var tmpVertexArray = new List<Vertex>();
            var indices = new List<uint>();
            vertexAnimatedArray = null;

            foreach (var m in meshes)
            {
                foreach (var v in m.vertexArray)
                {
                    tmpVertexArray.Add(v);
                    indices.Add((uint)indices.Count);
                }
            }

            NumIndices = indices.Count;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indices.Count * sizeof(uint)), indices.ToArray(), BufferUsageHint.DynamicDraw);

            vertexArray = tmpVertexArray.ToArray();
            UpdateBuffer();
            UpdatePointIndices();
        }

        public bool IsSelected(List<Vector2> points, Camera camera)
        {
            if (!IsVisible)
                return false;

            var transform = Transform * camera.ViewMatrix;
            var verticesMap = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            for (var i = 0; i < vertexArray.Length; i++)
            {
                var vertex = vertexArray[i].Position;
                int index;
                if (!verticesMap.TryGetValue(vertex, out index))
                    verticesMap.Add(vertex, 0);
            }

            foreach (var v in verticesMap)
            {
                var vertex = Vector3.Transform(v.Key, transform);
                var count = 0;
                for (var i = 0; i < points.Count; i++)
                {
                    var j = (i + 1) % points.Count;
                    var p0 = points[i];
                    var p1 = points[j];

                    if (p0.Y == points[j].Y)
                        continue;
                    if (p0.Y > vertex.Y && p1.Y > vertex.Y)
                        continue;
                    if (p0.Y < vertex.Y && p1.Y < vertex.Y)
                        continue;
                    if (Math.Max(p0.Y, p1.Y) == vertex.Y)
                        count++;
                    else
                    {
                        if (Math.Min(p0.Y, p1.Y) == vertex.Y)
                            continue;
                        
                        var t = (vertex.Y - p0.Y) / (p1.Y - p0.Y);
                        if (p0.X + t * (p1.X - p0.X) >= vertex.X)
                            count++;
                    } 
                }
                if (count % 2 == 1)
                    return true;
            }
            return false;
        }

        public void SetAnimationFrame(Matrix4[] AnimationTransform)
        {
            Parallel.For(0, points.Count, i =>
            {
                var pointInfo = points[i];
                if (pointInfo.Indices.Length > 0)
                {
                    var index = pointInfo.Indices[0];
                    var vertex = vertexArray[index];
                    var position = vertex.Position;
                    var normal = new Vector4(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z, 0.0f);

                    position = Vector3.Transform(position, Transform);

                    var bonesTransform = Matrix4.Zero;
                    var boneIndex = index * 4;
                    for (var j = 0; j < 4; j++)
                    {
                        var id = boneIndex + j;
                        bonesTransform += AnimationTransform[(int)vertexBoneIndicsArray[id]] * vertexBoneWeightArray[id];
                    }

                    position = Vector3.Transform(position, bonesTransform);
                    normal = Vector4.Transform(normal, bonesTransform);

                    foreach (var pointIndex in pointInfo.Indices)
                    {
                        vertexAnimatedArray[pointIndex].Position = position;
                        vertexAnimatedArray[pointIndex].Normal = normal.Xyz;
                    }
                }
            });

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexAnimatedArray.Length * Vertex.Stride), vertexAnimatedArray, BufferUsageHint.StreamDraw);
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
