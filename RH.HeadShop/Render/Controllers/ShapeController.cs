using System;
using System.Collections.Generic;
using OpenTK;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Helpers;

namespace RH.HeadShop.Render.Controllers
{
    /// <summary> Контроллер для трансформирования мешей (одежда, волосы) </summary>
    public class ShapeController
    {
        public Dictionary<Guid, ShapeMeshInfo> MeshesInfo = new Dictionary<Guid,ShapeMeshInfo>();

        private DynamicRenderMeshes meshes;
        private Vector2 point0, point1, point2, point3;
        private Vector2 directionVertical;
        private Vector2 directionHorisontal;
        private Vector2 center;
        private Vector2 halfSize;
        private const float linesCount = 7.0f;
        private int type;

        public void Initialize(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, DynamicRenderMeshes m, Camera camera, int t)
        {
            type = t;
            var verticesMap = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            MeshesInfo.Clear();            

            var invProjection = camera.ProjectMatrix.Inverted();
            point0 = SliceController.UnprojectPoint(p0, camera.WindowWidth, camera.WindowHeight, invProjection);
            point1 = SliceController.UnprojectPoint(p1, camera.WindowWidth, camera.WindowHeight, invProjection);
            point2 = SliceController.UnprojectPoint(p2, camera.WindowWidth, camera.WindowHeight, invProjection);
            point3 = SliceController.UnprojectPoint(p3, camera.WindowWidth, camera.WindowHeight, invProjection);

            var d0 = (point0 + point1) * 0.5f;
            var d1 = (point2 + point3) * 0.5f;
            center = (d0 + d1) * 0.5f;

            directionHorisontal = d1 - d0;
            halfSize.X = directionHorisontal.Length * 0.5f;
            directionHorisontal.Normalize();

            directionVertical = point0 - point1;
            halfSize.Y = directionVertical.Length * 0.5f;
            directionVertical.Normalize();

            meshes = m;
            foreach (var mesh in meshes)
            {
                verticesMap.Clear();
                var meshInfo = new ShapeMeshInfo();
                var tempVertices = mesh.GetVertices();
                var tempNormals = mesh.GetNormals();
                var transform = mesh.Transform * camera.ViewMatrix;
                meshInfo.InverceTransform = transform.Inverted();
                var normals = new List<Vector3>();
                for (var i = 0; i < tempVertices.Count; i++)
                {
                    var vertex = tempVertices[i];
                    int index;
                    if (!verticesMap.TryGetValue(vertex, out index))
                    {
                        index = meshInfo.Vertices.Count;
                        meshInfo.Vertices.Add(vertex);
                        normals.Add(tempNormals[i]);
                        verticesMap.Add(vertex, index);
                    }
                    meshInfo.Indices.Add(index);
                }

                var dt = halfSize.X / linesCount;
                foreach (var p in verticesMap)
                {
                    var position = Vector3.Transform(p.Key, transform);
                    if (PointInRectangle(ref point0, ref point1, ref point2, ref point3, ref position))
                    {
                        var shapePoint = new ShapePoint();
                        meshInfo.Points.Add(p.Value, shapePoint);
                        shapePoint.OriginalPosition = position;
                        var v = position.Xy - center;
                        shapePoint.LocalPosition.X = Vector2.Dot(directionHorisontal, v);
                        shapePoint.LocalPosition.Y = Vector2.Dot(directionVertical, v);
                        switch (type)
                        {
                            case 0://shape
                                shapePoint.K = 1.0f - Math.Abs(shapePoint.LocalPosition.Y / halfSize.Y);
                                break;
                            case 1://stretch
                                shapePoint.K = Math.Abs((shapePoint.LocalPosition.X + halfSize.X) / (halfSize.X * 2.0f));
                                break;
                            case 2://pleat
                                var x = shapePoint.LocalPosition.X + halfSize.X;
                                var l0 = (int)Math.Floor(x / dt);
                                x -= l0 * dt;
                                var ky = 1.0f - Math.Abs(shapePoint.LocalPosition.Y / halfSize.Y);
                                shapePoint.K = Math.Abs((x / dt) * 2.0f - 1.0f) * ky;
                                var normal = Vector3.Transform(normals[p.Value], transform);
                                if (normal.Z < 0.0f)
                                    shapePoint.K = -shapePoint.K;
                                break;
                        }
                    }
                }

                MeshesInfo.Add(mesh.Id, meshInfo);
            }
        }

        private bool PointInRectangle(ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d, ref Vector3 p)
        {
            var n1 = (b.Y - a.Y) * (p.X - a.X) - (b.X - a.X) * (p.Y - a.Y);
            var n2 = (c.Y - b.Y) * (p.X - b.X) - (c.X - b.X) * (p.Y - b.Y);
            var n3 = (d.Y - c.Y) * (p.X - c.X) - (d.X - c.X) * (p.Y - c.Y);
            var n4 = (a.Y - d.Y) * (p.X - d.X) - (a.X - d.X) * (p.Y - d.Y);
            return (n1 < 0.0f && n2 < 0.0f && n3 < 0.0f && n4 < 0.0f) || (n1 > 0.0f && n2 > 0.0f && n3 > 0.0f && n4 > 0.0f);
        }

        public void FillDataForMesh()
        {
            foreach (var meshInfo in MeshesInfo)
            {
                foreach (var point in meshInfo.Value.Points)
                    meshInfo.Value.Vertices[point.Key] = Vector3.Transform(point.Value.Position, meshInfo.Value.InverceTransform);
            }
        }

        public void Synchronize(ShapeController controller)
        {
            foreach (var meshInfo in MeshesInfo)
            {
                var tempMeshInfo = controller.MeshesInfo[meshInfo.Key];
                for (var i = 0; i < meshInfo.Value.Vertices.Count; i++)
                {
                    meshInfo.Value.Vertices[i] = (meshInfo.Value.Vertices[i] + tempMeshInfo.Vertices[i]) * 0.5f;
                }
            }
        }

        public void Transform(float delta)
        {
            foreach (var meshInfo in MeshesInfo)
            {
                foreach (var point in meshInfo.Value.Points)
                {
                    var deltaX = 0.0f;
                    point.Value.Position = point.Value.OriginalPosition;
                    switch (type)
                    {
                        case 0:
                            deltaX = point.Value.LocalPosition.X;
                            break;
                        case 1:
                            deltaX = 100.0f;
                            break;
                        case 2:
                            point.Value.Position.Z = point.Value.OriginalPosition.Z + 5.0f * delta * point.Value.K;
                            break;
                    }
                    var p = center +
                        directionHorisontal * (point.Value.LocalPosition.X + deltaX * point.Value.K * delta) +
                        directionVertical * point.Value.LocalPosition.Y;
                    point.Value.Position.X = p.X;
                    point.Value.Position.Y = p.Y;
                }
            }
        }
    }
}
