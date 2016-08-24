using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Meshes;
using BezierCurve = RH.HeadShop.Render.Helpers.BezierCurve;

namespace RH.HeadShop.Render.Controllers
{
    /// <summary> Контроллер, ответственный за разрезание. Используется при разрезании волос. </summary>
    public class SliceController
    {
        #region var

        public DynamicRenderMeshes ResultMeshes = new DynamicRenderMeshes();
        public static VectorEqualityComparer PointsCompare = new VectorEqualityComparer();
        private static readonly BezierCurve bezierCurve = new BezierCurve();

        private readonly List<SliceLine> sliceLines = new List<SliceLine>();
        private readonly List<SlicePoint> slicePoints = new List<SlicePoint>();

        private readonly Dictionary<Vector2, int> slicePointsDictionary = new Dictionary<Vector2, int>(new VectorEqualityComparer());
        private readonly Dictionary<Vector3, int> verticesDictionary = new Dictionary<Vector3, int>(new VectorEqualityComparer());
        private readonly Dictionary<int, TrianleConnections> triangles = new Dictionary<int, TrianleConnections>();
        private readonly Dictionary<int, List<int>> triangleConnections = new Dictionary<int, List<int>>();
        private readonly Dictionary<int, bool> usedTriangles = new Dictionary<int, bool>();
        private readonly List<int> foundedTriangles = new List<int>();

        private readonly List<Vector2> texCoords = new List<Vector2>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> indices = new List<int>();

        public readonly List<Line2d> Lines = new List<Line2d>();
        private readonly List<Line2d> mirrorLines = new List<Line2d>();

        public bool IsEmpty()
        {
            return Lines.Count == 0;
        }

        bool isBezier;

        #endregion

        public void BeginSlice(bool bezier = false)
        {
            isBezier = bezier;            
            Lines.Clear();
            mirrorLines.Clear();
            sliceLines.Clear();
        }
        public void EndSlice(int screenWidth, int screenHeight, Camera camera)
        {
            if (Lines.Count == 0)
                return;

            Lines.RemoveAt(Lines.Count - 1);
            if (mirrorLines.Count > 0)
                mirrorLines.RemoveAt(mirrorLines.Count - 1);
            if (isBezier)
            {
                var p = GetBezierPoints(Lines);
                Lines.Clear();
                for (var i = 1; i < p.Count; i++)
                    Lines.Add(new Line2d
                    {
                        Point0 = p[i - 1],
                        Point1 = p[i]
                    });

                if (mirrorLines.Count > 0)
                {
                    p = GetBezierPoints(mirrorLines);
                    mirrorLines.Clear();
                    for (var i = 1; i < p.Count; i++)
                        mirrorLines.Add(new Line2d
                        {
                            Point0 = p[i - 1],
                            Point1 = p[i]
                        });
                }
            }

            Lines.AddRange(mirrorLines);
            var invProjection = camera.ProjectMatrix.Inverted();
            foreach (var line in Lines)
            {
                line.Point0 = UnprojectPoint(line.Point0, screenWidth, screenHeight, invProjection);
                line.Point1 = UnprojectPoint(line.Point1, screenWidth, screenHeight, invProjection);
            }
            FindSelfCollisions();
        }

        public void AddPoint(Vector2 point, bool shift, int screenWidth = 0)
        {
            if (shift)
                point = GetShiftpoint(point);

            AddPoint(Lines, point);
            if (screenWidth > 0)
            {
                var x = point.X - screenWidth * 0.5f;
                AddPoint(mirrorLines, new Vector2(screenWidth * 0.5f - x, point.Y));
            }
        }

        const int SEGMENTS_PER_CURVE = 7;
        List<Vector2> GetBezierPoints(List<Line2d> sLines)
        {
            var controlPoints = new List<Vector2>();
            foreach (var l in sLines)
                controlPoints.Add(l.Point0);
            controlPoints.Add(sLines.Last().Point1);
            return bezierCurve.Bezier2D(controlPoints, SEGMENTS_PER_CURVE);
        }

        public void Draw()
        {
            GL.Color3(1.0f, 0.0f, 0.0f);

            if (isBezier && Lines.Count > 1)
            {
                var p = GetBezierPoints(Lines);
                GL.Begin(PrimitiveType.Lines);
                for (var i = 1; i < p.Count; i++)
                {
                    GL.Vertex2(p[i - 1]);
                    GL.Vertex2(p[i]);
                }

                if (mirrorLines.Count > 0)
                {
                    p = GetBezierPoints(mirrorLines);
                    for (var i = 1; i < p.Count; i++)
                    {
                        GL.Vertex2(p[i - 1]);
                        GL.Vertex2(p[i]);
                    }
                }
                GL.End();
            }
            else
            {
                GL.Begin(PrimitiveType.Lines);
                foreach (var line in Lines)
                {
                    GL.Vertex2(line.Point0);
                    GL.Vertex2(line.Point1);
                }

                foreach (var line in mirrorLines)
                {
                    GL.Vertex2(line.Point0);
                    GL.Vertex2(line.Point1);
                }
                GL.End();
            }

            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Points);
            foreach (var line in Lines)
                GL.Vertex2(line.Point0);            
            foreach (var line in mirrorLines)
                GL.Vertex2(line.Point0);
            if (Lines.Count > 0)
                GL.Vertex2(Lines.Last().Point1);
            if (mirrorLines.Count > 0)
                GL.Vertex2(mirrorLines.Last().Point1);
            GL.End();
            GL.PointSize(1.0f);
        }

        private bool CheckAABB(ref Vector3 point0, ref Vector3 point1, ref Vector3 point2, ref Vector2 A, ref Vector2 B)
        {
            return !((point0.X < A.X && point1.X < A.X && point2.X < A.X) ||
                     (point0.X > B.X && point1.X > B.X && point2.X > B.X) ||
                     (point0.Y < A.Y && point1.Y < A.Y && point2.Y < A.Y) ||
                     (point0.Y > B.Y && point1.Y > B.Y && point2.Y > B.Y));
        }

        private bool PointInTriangle(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector2 p)
        {
            var n1 = (b.Y - a.Y) * (p.X - a.X) - (b.X - a.X) * (p.Y - a.Y);
            var n2 = (c.Y - b.Y) * (p.X - b.X) - (c.X - b.X) * (p.Y - b.Y);
            var n3 = (a.Y - c.Y) * (p.X - c.X) - (a.X - c.X) * (p.Y - c.Y);
            return (n1 < 0.0f && n2 < 0.0f && n3 < 0.0f) || (n1 > 0.0f && n2 > 0.0f && n3 > 0.0f);
        }

        public void Slice(DynamicRenderMesh mesh, Camera camera)
        {
            slicePointsDictionary.Clear();
            verticesDictionary.Clear();
            slicePoints.Clear();
            vertices.Clear();
            texCoords.Clear();
            indices.Clear();
            triangles.Clear();
            ResultMeshes.Clear();

            FillVertices(mesh, camera);
            Vector2 A = new Vector2(99999.0f, 99999.0f), B = new Vector2(-99999.0f, -99999.0f);
            var tempLine = new Line2d();
            var workedIndices = new List<int>();
            var tempIndices = new List<int>();
            foreach (var line in Lines)
            {
                for (var i = 0; i < 2; i++)
                {
                    if (line.A[i] < A[i])
                        A[i] = line.A[i];
                    if (line.B[i] > B[i])
                        B[i] = line.B[i];
                }
            }
            for (var i = 0; i < indices.Count / 3; i++)
            {
                var index = i * 3;
                var point0 = vertices[indices[index]];
                var point1 = vertices[indices[index + 1]];
                var point2 = vertices[indices[index + 2]];
                


                if (!CheckAABB(ref point0, ref point1, ref point2, ref A, ref B))
                {
                    for (var j = 0; j < 3; j++)
                        tempIndices.Add(indices[index + j]);
                    continue;
                }

                for (var j = 0; j < 3; j++)
                    workedIndices.Add(indices[index + j]);
            }

            if (workedIndices.Count == 0)
                return;

            var triangle = new Vector3[3];
            var triangleT = new Vector2[3];
            var tempCollisions = new List<Vector3>();
            var tempTexCoords = new List<Vector2>();
            var tempCollisionsEdgeIndices = new List<int>();
            var newTriangles = new List<int>();
            var info0 = CollisionInfo.Zero;
            var info1 = CollisionInfo.Zero;

            for (var i = 0; i < sliceLines.Count; i++)
            {
                var line = Lines[i];
                var sliceLine = sliceLines[i];
                var trianglesCount = workedIndices.Count / 3;
                for (var j = 0; j < trianglesCount; j++)
                {
                    var index = j * 3;
                    for (var l = 0; l < 3; l++)
                    {
                        triangle[l] = vertices[workedIndices[index + l]];
                        triangleT[l] = texCoords[workedIndices[index + l]];
                    }

                    info0 = CollisionInfo.Zero;
                    info1 = CollisionInfo.Zero;
                    tempCollisions.Clear();
                    tempTexCoords.Clear();
                    tempCollisionsEdgeIndices.Clear();
                    newTriangles.Clear();
                    //looking for a point / intersection point of the triangle and line
                    for (int k = 0, l = 2; k < 3; l = k, k++)
                    {
                        if (info1.Type != CollisionType.CT_NONE)
                            break;
                        var tPoint0 = triangle[l];
                        var tPoint1 = triangle[k];

                        var tTexCoord0 = triangleT[l];
                        var tTexCoord1 = triangleT[k];

                        tempLine.Point0.X = tPoint0.X;
                        tempLine.Point0.Y = tPoint0.Y;
                        tempLine.Point1.X = tPoint1.X;
                        tempLine.Point1.Y = tPoint1.Y;

                        float ua, ub;
                        if (!GetUaUb(tempLine, line, out ua, out ub))
                        {
                            //lines coincide?
                            //Verify whether some point belongs line segment tempLine
                            //if find - get result and break.
                        }
                        else
                        {
                            if (ub < 0.0f || ub > 1.0f)
                                continue;
                            var collisionPoint = tPoint0 + (tPoint1 - tPoint0) * ub;
                            var collisionTexCoord = tTexCoord0 + (tTexCoord1 - tTexCoord0) * ub;
                            if (tempCollisions.Count == 0 || !PointsCompare.Equals(tempCollisions[0], collisionPoint))
                            {
                                tempCollisions.Add(collisionPoint);
                                tempTexCoords.Add(collisionTexCoord);
                                tempCollisionsEdgeIndices.Add(l);
                            }
                            if (ua < 0.0f || ua > 1.0f)
                                continue;

                            var pointType = CollisionType.CT_VERTEX;
                            Vector3 point;
                            Vector2 texCoord;
                            var pointIndex = -1;
                            var edgeIndex = -1;

                            if (ub > 0.0f)
                            {
                                if (ub < 1.0f)
                                {
                                    pointType = CollisionType.CT_EDGE;
                                    point = tempCollisions.Last();
                                    texCoord = tempTexCoords.Last();
                                    edgeIndex = l;
                                }
                                else
                                {
                                    point = tPoint1;
                                    texCoord = tTexCoord1;
                                    pointIndex = workedIndices[index + k];
                                }
                            }
                            else
                            {
                                point = tPoint0;
                                texCoord = tTexCoord0;
                                pointIndex = workedIndices[index + l];
                            }
                            if (info0.Type == CollisionType.CT_NONE)
                            {
                                info0.PointIndex = pointIndex;
                                info0.Type = pointType;
                                info0.Position = point;
                                info0.TexCoord = texCoord;
                                info0.EdgeIndex = edgeIndex;
                            }
                            else
                            {
                                if (pointIndex == -1 || info0.PointIndex != pointIndex)
                                {
                                    info1.PointIndex = pointIndex;
                                    info1.Type = pointType;
                                    info1.Position = point;
                                    info1.TexCoord = texCoord;
                                    info1.EdgeIndex = edgeIndex;
                                }
                            }
                        }
                    }

                    if (info1.Type == CollisionType.CT_NONE)
                    {
                        if (tempCollisions.Count == 0)
                        {
                            if (info0.Type == CollisionType.CT_NONE)
                                continue;
                        }
                        else
                        {
                            if (tempCollisions.Count > 1)
                            {
                                //Perhaps the point inside the triangle
                                var dir = line.Direction;
                                for (var l = 0; l < 2; l++)
                                {
                                    var p = l == 0 ? line.Point0 : line.Point1;
                                    if (PointInTriangle(ref triangle[0], ref triangle[1], ref triangle[2], ref p))
                                    {
                                        var v0 = tempCollisions[1].Xy - tempCollisions[0].Xy;
                                        var v1 = p - tempCollisions[0].Xy;
                                        var k = (v1.Length / v0.Length);
                                        var z = tempCollisions[0].Z + (tempCollisions[1].Z - tempCollisions[0].Z) * k;
                                        var t = tempTexCoords[0] + (tempTexCoords[1] - tempTexCoords[0]) * k;
                                        if (info0.Type == CollisionType.CT_NONE)
                                        {
                                            info0.Type = CollisionType.CT_INSIDE;
                                            info0.Position = new Vector3(p.X, p.Y, z);
                                            info0.TexCoord = t;
                                            if (Vector2.Dot(dir, v0) > 0.0f)
                                                info0.EdgeIndex = tempCollisionsEdgeIndices[0];
                                            else
                                                info0.EdgeIndex = tempCollisionsEdgeIndices[1];
                                            tempCollisionsEdgeIndices.Remove(info0.EdgeIndex);
                                        }
                                        else
                                        {
                                            info1.Type = CollisionType.CT_INSIDE;
                                            info1.Position = new Vector3(p.X, p.Y, z);
                                            info1.TexCoord = t;
                                            info1.EdgeIndex = tempCollisionsEdgeIndices[0];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (info0.Type == CollisionType.CT_NONE)
                        continue;
                    //Create new triangles, if we have two points of intersection, and they are not vertices
                    int pi1 = 0, pi0 = 0;
                    if (info1.Type != CollisionType.CT_NONE && (info0.Type != CollisionType.CT_VERTEX || info1.Type != CollisionType.CT_VERTEX ))
                    {
                        if (info1.Type == CollisionType.CT_VERTEX
                            || (info1.Type == CollisionType.CT_EDGE && info0.Type == CollisionType.CT_INSIDE)
                            || (info1.Type == CollisionType.CT_EDGE && info0.Type == CollisionType.CT_EDGE && (info0.EdgeIndex + 1) % 3 != info1.EdgeIndex)
                            || (info0.Type == CollisionType.CT_INSIDE && info1.Type == CollisionType.CT_INSIDE && (info0.EdgeIndex + 1) % 3 != info1.EdgeIndex))
                        {
                            var temp = info1;
                            info1 = info0;
                            info0 = temp;
                        }

                        if (!verticesDictionary.TryGetValue(info1.Position, out pi1))
                        {
                            pi1 = vertices.Count;
                            vertices.Add(info1.Position);
                            texCoords.Add(info1.TexCoord);
                            verticesDictionary.Add(info1.Position, pi1);
                        }

                        if (info0.Type == CollisionType.CT_VERTEX) //One point of intersection coincides with the vertex
                        {
                            pi0 = info0.PointIndex;
                            int i0 = workedIndices[index], i1 = workedIndices[index + 1], i2 = workedIndices[index + 2];
                            if (i0 == info0.PointIndex)
                            {
                                i0 = i2;
                                i2 = i1;
                            }
                            else
                            {
                                if (i2 == info0.PointIndex)
                                {
                                    i2 = i0;
                                    i0 = i1;
                                }
                            }
                            i1 = info0.PointIndex;

                            newTriangles.AddRange(new[] {   i0, i1, pi1,
                                                            i1, i2, pi1 });
                            if (info1.Type == CollisionType.CT_INSIDE) //The second point inside the triangle
                                newTriangles.AddRange(new[] { i2, i0, pi1 });
                        }
                        else
                        {
                            if (!verticesDictionary.TryGetValue(info0.Position, out pi0))
                            {
                                pi0 = vertices.Count;
                                vertices.Add(info0.Position);
                                texCoords.Add(info0.TexCoord);
                                verticesDictionary.Add(info0.Position, pi0);
                            }

                            if (info1.Type != info0.Type) //One point crosses the brink, the second inside the triangle
                            {
                                var prev = info0.EdgeIndex == 0 ? 2 : info0.EdgeIndex - 1;
                                prev = workedIndices[index + prev];
                                var next = info0.EdgeIndex == 2 ? 0 : info0.EdgeIndex + 1;
                                next = workedIndices[index + next];
                                var curr = workedIndices[index + info0.EdgeIndex];
                                newTriangles.AddRange(new[] {   prev, curr, pi1,
                                                                curr, pi0, pi1,
                                                                pi0, next, pi1,
                                                                next, prev, pi1 });
                            }
                            else
                            {
                                var c0 = workedIndices[index + info0.EdgeIndex];
                                var c1 = workedIndices[index + info1.EdgeIndex];
                                var c2 = workedIndices[index + ((info1.EdgeIndex + 1) % 3)];

                                if (info0.Type == CollisionType.CT_EDGE)
                                {
                                    newTriangles.AddRange(new[] {   c0, pi0, pi1, 
                                                                pi0, c1, pi1, 
                                                                pi1, c2, c0 });
                                }
                                else
                                {
                                    newTriangles.AddRange(new[] {   c0, c1, pi0,
                                                                    c1, pi1, pi0,
                                                                    c1, c2, pi1,
                                                                    c2, c0, pi1,
                                                                    c0, pi0, pi1});

                                }
                            }
                        }
                    }
                    int slicePointIndex;
                    SlicePoint slicePoint;
                    for (var l = 0; l < 2; l++)
                    {
                        if (l == 1 && info1.Type == CollisionType.CT_NONE)
                            break;
                        var position = l == 0 ? info0.Position.Xy : info1.Position.Xy;
                        var pointIndex = l == 0 ? pi0 : pi1;
                        if (!slicePointsDictionary.TryGetValue(position, out slicePointIndex))
                        {
                            slicePoint = new SlicePoint
                            {
                                Coordinate = position
                            };
                            slicePoint.Lines.Add(sliceLine);
                            slicePoint.PreparePoint();
                            slicePointIndex = slicePoints.Count;
                            slicePoints.Add(slicePoint);
                            slicePointsDictionary.Add(position, slicePointIndex);
                        }
                        else
                            slicePoint = slicePoints[slicePointIndex];
                        if (!slicePoint.Indices.Contains(pointIndex))
                            slicePoint.Indices.Add(pointIndex);
                    }

                    if (newTriangles.Count > 0)
                    {
                        for (var l = 0; l < 3; l++)
                            workedIndices[index + l] = newTriangles[l];
                        newTriangles.RemoveRange(0, 3);
                        workedIndices.InsertRange(index, newTriangles);
                        var count = (newTriangles.Count / 3);
                        j += count;
                        trianglesCount += count;
                    }
                }
            }

            for (var i = 0; i < workedIndices.Count / 3; i++)
            {
                var index = i * 3;
                var t = new Triangle();
                for (var j = 0; j < 3; j++)
                    t.Indices[j] = workedIndices[index + j];
                triangles.Add(i, new TrianleConnections
                {
                    Triangle = t
                });
            }

            var ind = new List<int>();
            var tempTriangle = new Triangle();
            tempTriangle.Indices[1] = -1;
            foreach (var point in slicePoints)
            {
                point.PreparePoint();
                foreach (var index in point.Indices)
                {
                    ind.Clear();
                    ind.Add(index);
                    tempTriangle.Indices[0] = index;
                    //Duplicate the verticle several times
                    for (var i = 1; i < point.Directions.Count; i++)
                    {
                        ind.Add(vertices.Count);
                        vertices.Add(vertices[index]);
                        texCoords.Add(texCoords[index]);
                    }
                    foreach (var t in triangles)
                    {
                        var tr = t.Value.Triangle;
                        var id = Triangle.TriangleEquals(ref tempTriangle, ref tr);
                        if (id > -1)
                        {
                            var center = (vertices[tr.Indices[0]].Xy + vertices[tr.Indices[1]].Xy + vertices[tr.Indices[2]].Xy) / 3.0f;
                            var dir = (center - point.Coordinate).Normalized();
                            var angle = SlicePoint.GetAngle(ref dir);
                            var ii = point.Directions.Count - 1;
                            for (var j = 0; j < point.Directions.Count; j++)
                            {
                                if (angle < point.Directions[j])
                                    break;
                                ii = j;
                            }
                            tr.Indices[id] = ind[ii];
                        }
                    }
                }
            }

            var invTransform = mesh.Transform * camera.ViewMatrix;
            invTransform.Invert();
            for (var i = 0; i < vertices.Count; i++)
            {
                vertices[i] = Vector3.Transform(vertices[i], invTransform);
            }

            workedIndices.Clear();

            foreach (var t in triangles)
            {
                foreach (var i in t.Value.Triangle.Indices)
                    workedIndices.Add(i);
            }
            workedIndices.AddRange(tempIndices);
            triangleConnections.Clear();
            triangles.Clear();

            for (var i = 0; i < workedIndices.Count / 3; i++)
            {
                var index = i * 3;
                var t = new Triangle();
                for (var j = 0; j < 3; j++)
                {
                    t.Indices[j] = workedIndices[index + j];
                    List<int> l;
                    if (!triangleConnections.TryGetValue(t.Indices[j], out l))
                    {
                        l = new List<int>();
                        triangleConnections.Add(t.Indices[j], l);
                    }
                    l.Add(i);
                }
                triangles.Add(i, new TrianleConnections
                {
                    Triangle = t
                });
            }

            var mainMesh = true;
            while (triangles.Count > 0)
            {
                foundedTriangles.Clear();
                FillTriangles(triangles.First().Key);

                if (mainMesh)
                {
                    mesh.Create(vertices, texCoords, foundedTriangles, mesh.Material.DiffuseTextureMap, mesh.Material.TransparentTextureMap, mesh.TextureAngle, mesh.TextureSize);
                    mainMesh = false;
                }
                else
                {
                    var tmpMesh = new DynamicRenderMesh(MeshType.Hair);
                    tmpMesh.Create(vertices, texCoords, foundedTriangles, mesh.Material.DiffuseTextureMap, mesh.Material.TransparentTextureMap, mesh.TextureAngle, mesh.TextureSize);
                    tmpMesh.Transform = mesh.Transform;
                    tmpMesh.Material.DiffuseColor = mesh.Material.DiffuseColor;
                    tmpMesh.MeshAngle = mesh.MeshAngle;
                    tmpMesh.MeshSize = mesh.MeshSize;

                    var info = tmpMesh.GetMeshInfo();
                    var center = Vector3.Zero;
                    var scale = PickingController.GetHairScale(ProgramCore.Project.ManType);
                    foreach (var vert in info.Positions)
                    {
                        center.X += vert.X * scale;
                        center.Y += vert.Y * scale;
                        center.Z += vert.Z * scale;
                    }
                    center /= info.Positions.Count;
                    tmpMesh.Position = center;

                    ResultMeshes.Add(tmpMesh);
                }
            }
        }

        private void FillTriangles(int id)
        {
            var turn = new Queue<int>();
            usedTriangles.Clear();
            usedTriangles.Add(id, true);
            turn.Enqueue(id);
            while (turn.Count > 0)
            {
                var index = turn.Dequeue();
                var t = triangles[index].Triangle;
                triangles.Remove(index);
                foundedTriangles.AddRange(t.Indices);
                foreach (var i in t.Indices)
                {
                    var list = triangleConnections[i];
                    foreach (var l in list)
                        if (triangles.ContainsKey(l) && !usedTriangles.ContainsKey(l))
                        {
                            usedTriangles.Add(l, true);
                            turn.Enqueue(l);
                        }
                }
            }
        }

        #region prepare

        private bool GetUaUb(Line2d line1, Line2d line2, out float ua, out float ub)
        {
            ua = 0.0f;
            ub = 0.0f;
            // denominator
            var a = line1.Point1.Y - line1.Point0.Y;
            var d = line1.Point1.X - line1.Point0.X;
            var b = line2.Point0.X - line2.Point1.X;
            var c = line2.Point0.Y - line2.Point1.Y;
            var e = line2.Point0.X - line1.Point0.X;
            var f = line2.Point0.Y - line1.Point0.Y;

            var z = a * b - c * d;
            // numerator 1 
            var ca = a * e - f * d;
            // numerator 2
            var cb = f * b - c * e;
            if (z == 0.0f)
                return false;

            ua = ca / z;
            ub = cb / z;
            return true;
        }

        private bool CollisionSegments(Line2d line1, Line2d line2, out Vector2 point) // The point of intersection of two segments
        {
            point = Vector2.Zero;
            float ua, ub;
            if (!GetUaUb(line1, line2, out ua, out ub))
                return false;
            if ((0.0f < ua) && (ua < 1.0f) && (0.0f < ub) && (ub < 1.0f))
            {
                point.X = line1.Point0.X + (line1.Point1.X - line1.Point0.X) * ub;
                point.Y = line1.Point0.Y + (line1.Point1.Y - line1.Point0.Y) * ub;
                return true;
            }
            return false;
        }

        public static Vector2 UnprojectPoint(Vector2 point, int screenWidth, int screenHeight, Matrix4 invProjection)
        {
            var x = (point.X * 2.0f / screenWidth) - 1.0f;
            var y = 1.0f - (point.Y * 2.0f / screenHeight);
            var p = new Vector3(x, y, -1.0f);
            return Vector3.Transform(p, invProjection).Xy;
        }

        private void AddPoint(List<Line2d> sliceLines, Vector2 point)
        {
            if (sliceLines.Count > 0)
                sliceLines.Last().Point1 = point;
            sliceLines.Add(new Line2d
            {
                Point0 = point,
                Point1 = point
            });
        }

        private void FindSelfCollisions()
        {
            for (var i = 0; i < Lines.Count; i++)
            {
                var line1 = Lines[i];
                for (var j = i + 1; j < Lines.Count; j++)
                {
                    var line2 = Lines[j];
                    Vector2 point;
                    if (CollisionSegments(line1, line2, out point))
                    {
                        Lines.Add(new Line2d
                        {
                            Point0 = point,
                            Point1 = line1.Point1
                        });
                        line1.Point1 = point;
                        Lines.Add(new Line2d
                        {
                            Point0 = point,
                            Point1 = line2.Point1
                        });
                        line2.Point1 = point;
                    }
                }
            }
            var tmpIndices = new List<int>();
            slicePointsDictionary.Clear();
            foreach (var line in Lines)
            {
                line.UpdateDirection();
                for (var i = 0; i < 2; i++)
                {
                    if (line.Point0[i] < line.Point1[i])
                    {
                        line.A[i] = line.Point0[i];
                        line.B[i] = line.Point1[i];
                    }
                    else
                    {
                        line.A[i] = line.Point1[i];
                        line.B[i] = line.Point0[i];
                    }
                }
                tmpIndices.Clear();
                for (var i = 0; i < 2; i++)
                {
                    var point = i == 0 ? line.Point0 : line.Point1;
                    var index = slicePoints.Count;
                    if (!slicePointsDictionary.TryGetValue(point, out index))
                    {
                        index = slicePoints.Count;
                        slicePoints.Add(new SlicePoint
                        {
                            Coordinate = point
                        });
                        slicePointsDictionary.Add(point, index);
                    }
                    tmpIndices.Add(index);
                }
                var sliceLine = new SliceLine
                {
                    Point0 = slicePoints[tmpIndices[0]],
                    Point1 = slicePoints[tmpIndices[1]],
                    Line = line
                };
                sliceLine.Point0.Lines.Add(sliceLine);
                sliceLine.Point1.Lines.Add(sliceLine);
                sliceLines.Add(sliceLine);
            }
        }

        private void FillVertices(DynamicRenderMesh mesh, Camera camera)
        {
            var transform = mesh.Transform * camera.ViewMatrix;
            var tmpVertices = mesh.GetVertices();
            var tmpTexCoords = mesh.GetTexCoords();
            for (var i = 0; i<tmpVertices.Count; i++)
            {
                var vertex = tmpVertices[i];
                var index = vertices.Count;
                if (!verticesDictionary.TryGetValue(vertex, out index))
                {
                    index = vertices.Count;
                    verticesDictionary.Add(vertex, index);
                    vertices.Add(vertex);
                    texCoords.Add(tmpTexCoords[i]);
                }
                indices.Add(index);
            }
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = Vector3.Transform(vertices[i], transform);
        }
        #endregion

        private void MovePoint(List<Line2d> sliceLines, Vector2 point)
        {
            if (sliceLines.Count > 0)
            {
                sliceLines.Last().Point1 = point;
            }
        }
        public void MovePoint(Vector2 point, bool shift, int screenWidth = 0)
        {
            if (shift)
                point = GetShiftpoint(point);

            MovePoint(Lines, point);
            if (screenWidth > 0)
            {
                var x = point.X - screenWidth * 0.5f;
                MovePoint(mirrorLines, new Vector2(screenWidth * 0.5f - x, point.Y));
            }
        }

        private Vector2 GetShiftpoint(Vector2 point)
        {
            if (Lines.Count == 0)
                return point;
            var d = point - Lines.Last().Point0;
            if (Math.Abs(d.X) > Math.Abs(d.Y))
                d.Y = 0.0f;
            else
                d.X = 0.0f;
            return Lines.Last().Point0 + d;
        }

    }
}
