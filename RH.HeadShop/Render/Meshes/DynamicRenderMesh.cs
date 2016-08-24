using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadShop.Helpers;
using RH.HeadShop.Render.Helpers;

namespace RH.HeadShop.Render.Meshes
{
    public class DynamicRenderMesh : BaseRenderMesh
    {
        #region Var

        public Vector3 Position;

        public float MeshSize = 1;
        public float MeshAngle = 0;

        public bool IsChanged;
        public MeshType meshType;

        private Vertex[] vertexNull = null;
        private Vertex[] vertexFull = null;

        #endregion

        public DynamicRenderMesh(MeshType type)
        {
            IsChanged = true;
            meshType = type;
        }

        public void SetNullPoints(List<float> vertexPositions, List<float> vertexNormals, List<float> vertexTextureCoordinates)
        {
            if (vertexPositions.Count / 3 != vertexArray.Length)
                return;

            vertexNull = new Vertex[vertexArray.Length];
            vertexFull = new Vertex[vertexArray.Length];
            vertexArray.CopyTo(vertexNull, 0);

            for (var i = 0; i < vertexPositions.Count / 3; i++)
            {
                var index = i * 3;
                vertexFull[i].Position = new Vector3(vertexPositions[index], vertexPositions[index + 1], vertexPositions[index + 2]);
                vertexFull[i].Normal = new Vector3(vertexNormals[index], vertexNormals[index + 1], vertexNormals[index + 2]);
                vertexFull[i].TexCoord = new Vector2(vertexTextureCoordinates[i * 2], 1.0f - vertexTextureCoordinates[i * 2 + 1]);
                vertexFull[i].Color = Vector4.One;
            }
        }

        public void InterpolateMesh(float k)
        {
            InterpolateMesh(k, true);
        }

        public void InterpolateMesh(float k, bool texCoords)
        {
            if (vertexNull == null)
                return;
            var invK = 1.0f - k;
            Parallel.For(0, vertexArray.Length, i =>
            {
                vertexArray[i].Position = vertexNull[i].Position * invK + vertexFull[i].Position * k;
                vertexArray[i].Normal = vertexNull[i].Normal * invK + vertexFull[i].Normal * k;
            });

            if (texCoords)
            {
                for (var i = 0; i < vertexArray.Length; i++)
                    vertexArray[i].TexCoord = vertexNull[i].TexCoord * invK + vertexFull[i].TexCoord * k;
            }

            UpdateBuffer();
            UpdatePointIndices();

            if (vertexArray.Length > 0 && vertexArray.All(x =>x.TexCoord.X == 0 && x.TexCoord.Y == 1))
                UpdateTextureCoordinates(0, 1);
        }

        public bool Create(List<Vector3> vertexPositions, List<Vector2> textureCoordinates, List<int> vertexIndices, string texturePath, string alphaTexturePath, float textureAngle, float textureSize)
        {
            if (VertexBuffer != 0)
                Destroy();

            IsChanged = true;
            var indices = new List<uint>();
            var positions = new List<float>();
            var texCoords = new List<float>();
            var bonesInfo = new List<float>();
            var normals = GetNormals(vertexPositions, vertexIndices);

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
                    var vt = textureCoordinates[pointIndex];
                    positions.AddRange(new[] { vp.X, vp.Y, vp.Z });
                    indices.Add((uint)(index + j));
                    bonesInfo.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                    texCoords.AddRange(new[] { vt.X, vt.Y });
                }
            }

            if (!Create(positions, normals, texCoords, bonesInfo, bonesInfo, indices, texturePath, alphaTexturePath))
                return false;

            TextureAngle = textureAngle;
            TextureSize = textureSize;
            UpdatePointIndices();
            if ((textureAngle != 0.0 && textureSize != 1.0) || texCoords.All(x => x == 0))         // if changed any - need recalc. another - use initial
                UpdateTextureCoordinates(textureAngle, textureSize);
            return true;
        }

        public MeshInfo GetMeshInfo()
        {
            return new MeshInfo(this, vertexArray, Transform);
        }

        public void UpdateTextureCoordinates(float angle, float scale)
        {
            Parallel.For(0, vertexArray.Length / 3, i =>
                                                              {
                                                                  var posA = Vector3.Transform(vertexArray[i * 3].Position, Transform);
                                                                  var posB = Vector3.Transform(vertexArray[i * 3 + 1].Position, Transform);
                                                                  var posC = Vector3.Transform(vertexArray[i * 3 + 2].Position, Transform);

                                                                  var normal = GetNormal(ref posA, ref posB, ref posC);

                                                                  var center = (posA + posB + posC) / 3.0f;

                                                                  var nxy = new Vector2(normal.X, normal.Y);
                                                                  nxy.Normalize();
                                                                  var rotation = Matrix4.Identity;
                                                                  rotation.M22 = rotation.M11 = nxy.X;
                                                                  rotation.M12 = -nxy.Y;
                                                                  rotation.M21 = nxy.Y;

                                                                  var v1 = Vector3.Transform(posA - center, rotation);
                                                                  var v2 = Vector3.Transform(posB - center, rotation);
                                                                  var v3 = Vector3.Transform(posC - center, rotation);

                                                                  normal = Vector3.Transform(normal, rotation);

                                                                  nxy.X = normal.Z;
                                                                  nxy.Y = normal.X;

                                                                  rotation = Matrix4.Identity;
                                                                  rotation.M33 = rotation.M11 = nxy.X;
                                                                  rotation.M13 = nxy.Y;
                                                                  rotation.M31 = -nxy.Y;

                                                                  v1 = Vector3.Transform(v1, rotation) + center;
                                                                  v2 = Vector3.Transform(v2, rotation) + center;
                                                                  v3 = Vector3.Transform(v3, rotation) + center;

                                                                  rotation = Matrix4.CreateRotationZ(angle);

                                                                  v1 = Vector3.Transform(v1, rotation) / scale;
                                                                  v2 = Vector3.Transform(v2, rotation) / scale;
                                                                  v3 = Vector3.Transform(v3, rotation) / scale;

                                                                  vertexArray[i * 3].TexCoord = v1.Xy;
                                                                  vertexArray[i * 3 + 1].TexCoord = v2.Xy;
                                                                  vertexArray[i * 3 + 2].TexCoord = v3.Xy;
                                                              });

            FullUpdateTexcoords();
        }

        public void FullUpdateTexcoords()
        {
            foreach (var p in points)
            {
                var texcoord = Vector2.Zero;
                foreach (var i in p.Indices)
                {
                    texcoord += vertexArray[i].TexCoord;
                }
                texcoord /= (float)p.Indices.Length;

                foreach (var i in p.Indices)
                {
                    vertexArray[i].TexCoord = texcoord;
                }
            }

            UpdateBuffer();
            IsChanged = true;
        }

        public void Rotate(float angle, Matrix4 tempTransform, bool isRadAngle, Vector3 centerRotatePoint, Vector3 oldPosition)
        {
            try
            {
                var dir = new Vector3(ProgramCore.MainForm.ctrlRenderControl.camera.Position.X, 0.0f, ProgramCore.MainForm.ctrlRenderControl.camera.Position.Z);
                dir.Normalize();

                var radAngle = isRadAngle ? angle : (float)(Math.PI * angle / 180f);
                var cosA = (float)Math.Cos(radAngle);
                var sinA = (float)Math.Sin(radAngle);

                var rotateMatrix = new Matrix4();
                rotateMatrix[0, 0] = dir.X * dir.X * (1 - cosA) + cosA;
                rotateMatrix[0, 1] = dir.Z * sinA;
                rotateMatrix[0, 2] = dir.X * dir.Z * (1 - cosA);
                rotateMatrix[0, 3] = 0f;
                rotateMatrix[1, 0] = -dir.Z * sinA;
                rotateMatrix[1, 1] = cosA;
                rotateMatrix[1, 2] = dir.X * sinA;
                rotateMatrix[1, 3] = 0f;
                rotateMatrix[2, 0] = dir.X * dir.Z * (1 - cosA);
                rotateMatrix[2, 1] = -dir.X * sinA;
                rotateMatrix[2, 2] = dir.Z * dir.Z * (1 - cosA) + cosA;
                rotateMatrix[2, 3] = 0f;
                rotateMatrix[3, 0] = 0f;
                rotateMatrix[3, 1] = 0f;
                rotateMatrix[3, 2] = 0f;
                rotateMatrix[3, 3] = 1f;

                Transform = tempTransform;
                Transform[3, 0] -= centerRotatePoint.X;
                Transform[3, 1] -= centerRotatePoint.Y;
                Transform[3, 2] -= centerRotatePoint.Z;
                Position = oldPosition - centerRotatePoint;
                Transform *= rotateMatrix;
                Position = Vector3.Transform(Position, rotateMatrix);
                Transform[3, 0] += centerRotatePoint.X;
                Transform[3, 1] += centerRotatePoint.Y;
                Transform[3, 2] += centerRotatePoint.Z;
                Position += centerRotatePoint;
            }
            finally
            {
                IsChanged = true;
            }
        }

        public void SimpleDraw()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

            GL.VertexPointer(3, VertexPointerType.Float, Vertex.Stride, new IntPtr(0));

            GL.DrawRangeElements(BeginMode.Triangles, 0, NumIndices, NumIndices, DrawElementsType.UnsignedInt, new IntPtr(0));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
        }

        public void SetVertices(List<Vector3> vertexPositions, List<int> vertexIndices)
        {
            IsChanged = true;
            var tmpNormals = new List<Vector3>();
            var tmpCount = new List<float>();
            for (var i = 0; i < vertexPositions.Count; i++)
            {
                tmpNormals.Add(Vector3.Zero);
                tmpCount.Add(0.0f);
            }

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
                    vertexArray[(index + j)].Position = vertexPositions[pointIndex];
                    tmpNormals[pointIndex] = tmpNormals[pointIndex] + n;
                    tmpCount[pointIndex] = tmpCount[pointIndex] + 1.0f;
                }
            }

            for (var i = 0; i < tmpNormals.Count; i++)
                tmpNormals[i] = tmpNormals[i] / tmpCount[i];

            for (var i = 0; i < vertexIndices.Count; i++)
            {
                var index = vertexIndices[i];
                vertexArray[i].Normal = tmpNormals[index];
            }

            UpdateBuffer();
            UpdatePointIndices();
            if (TextureAngle != 0.0 && TextureSize != 1.0)          // if changed any - need recalc. another - use initial
                UpdateTextureCoordinates(TextureAngle, TextureSize);
        }
    }

    public enum MeshType
    {
        Head,
        Hair,
        Accessory
    }

    public class DynamicRenderMeshes : Collection<DynamicRenderMesh>
    {
        public bool Contains(string title)
        {
            foreach (var item in this)
            {
                if (item.Title == title)
                    return true;
            }
            return false;
        }
        public bool Contains(Guid id)
        {
            foreach (var item in this)
            {
                if (item.Id == id)
                    return true;
            }
            return false;
        }

        public DynamicRenderMesh this[string title]
        {
            get
            {
                return this.FirstOrDefault(item => item.Title == title);
            }
        }
        public DynamicRenderMesh this[Guid id]
        {
            get
            {
                return this.FirstOrDefault(item => item.Id == id);
            }
        }
    }
}
