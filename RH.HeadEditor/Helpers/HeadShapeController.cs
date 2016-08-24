using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using RH.HeadEditor.Data;

namespace RH.HeadEditor.Helpers
{
    public enum ShapeCoefType
    {
        Grade4,
        Qubed,
        Squared,
        SquarRoot,
    }

    public class ShapePointData
    {
        public List<KeyValuePair<RenderMeshPart, int>> PartsPoints = new List<KeyValuePair<RenderMeshPart, int>>();
        public float Distance = 10000.0f;
        public float Coef = 0.0f;
        public bool IsMirrored = false;

        public void Shape(Vector3 dv, ref Dictionary<Guid, MeshUndoInfo> undoInfo)
        {
            if (IsMirrored)
                dv.X *= -1.0f;
            foreach (var p in PartsPoints)
            {
                MeshUndoInfo info;
                if (!undoInfo.TryGetValue(p.Key.Guid, out info))
                {
                    info = new MeshUndoInfo();
                    undoInfo.Add(p.Key.Guid, info);
                }
                var point = p.Key.Points[p.Value];
                if (!info.Points.ContainsKey(p.Value))
                    info.Points.Add(p.Value, point.Position);
                
                point.Position += dv * Coef;
                foreach (var id in point.Indices)
                {
                    var v = p.Key.Vertices[id];
                    v.Position = point.Position;
                    p.Key.Vertices[id] = v;
                }
            }
        }

        public void UpdateColor()
        {
            var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            foreach (var part in PartsPoints)
            {
                var point = part.Key.Points[part.Value];
                foreach (var i in point.Indices)
                    part.Key.Vertices[i].Color = color;
            }
        }
    }

    public class ShapePartInfo
    {
        public List<int> Points;
        public List<float> Distance;
    }

    public class HeadShapeController
    {
        private HeadMeshesController headMeshController;
        private readonly Dictionary<Guid, RenderMeshPart> shapedParts = new Dictionary<Guid, RenderMeshPart>();
        private readonly Dictionary<Vector3, ShapePointData> shapePoints = new Dictionary<Vector3, ShapePointData>(new VectorEqualityComparer());
        public Vector3 ShapePoint;
        private float shapeRadius = 0.0f;
        private RenderMeshPart startPart = null;
        private int startTriangle, startTriangleMirror = 0;
        private float isLeft = -1.0f;

        public void Initialize(HeadMeshesController hmc)
        {
            headMeshController = hmc;
        }

        public float StartShaping(Vector3 point, Matrix4 vm, bool isMirror, float radius, ShapeCoefType type)
        {
            EndShape();
            var depth = -10000.0f;

            foreach (var part in headMeshController.RenderMesh.Parts)
            {
                for (var i = 0; i < part.Indices.Count; i += 3)
                {
                    var p0 = Vector3.Transform(part.Vertices[part.Indices[i]].Position, vm);
                    var p1 = Vector3.Transform(part.Vertices[part.Indices[i + 1]].Position, vm);
                    var p2 = Vector3.Transform(part.Vertices[part.Indices[i + 2]].Position, vm);

                    var a = p0.Xy;
                    var b = p1.Xy;
                    var c = p2.Xy;

                    if (TexturingInfo.PointInTriangle(ref a, ref b, ref c, ref point))
                    {
                        var aup = a.X - point.X;
                        var bup = b.X - point.X;
                        var cup = c.X - point.X;
                        var avp = a.Y - point.Y;
                        var bvp = b.Y - point.Y;
                        var cvp = c.Y - point.Y;

                        var f = 1.0f / ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X));
                        var u = (bup * cvp - bvp * cup) * f;
                        var v = (cup * avp - cvp * aup) * f;
                        var w = 1.0f - (u + v);

                        var z = u * p0.Z + v * p1.Z + w * p2.Z;
                        if (depth < z)
                        {
                            startPart = part;
                            startTriangle = i;
                            depth = z;
                        }
                    }
                }
            }

            if (startPart == null || startTriangle < 0)
                return 0.0f;

            ShapePoint = point;
            ShapePoint.Z = depth;
            ShapePoint = Vector3.Transform(ShapePoint, vm.Inverted());
            startTriangleMirror = -1;
            if (isMirror)
            {
                //Ищем точку в оригинальных координатах 
                var triangle = new[] { startPart.Indices[startTriangle], startPart.Indices[startTriangle + 1], startPart.Indices[startTriangle + 2] };
                var a = startPart.Vertices[triangle[0]].OriginalPosition;
                var b = startPart.Vertices[triangle[1]].OriginalPosition;
                var c = startPart.Vertices[triangle[2]].OriginalPosition;
                a.X *= -1.0f;
                b.X *= -1.0f;
                c.X *= -1.0f;
                isLeft = ShapePoint.X < 0.0f ? 1.0f : -1.0f;
                int idA = -1, idB = -1, idC = -1;
                for (int i = 0; i < startPart.Vertices.Length; i++ )
                {
                    if (idA >= 0 && idB >= 0 && idC >= 0)
                        break;
                    var position = startPart.Vertices[i].OriginalPosition;
                    if (position.X * isLeft >= 0.0f)
                    {
                        if (idA < 0 && VectorEqualityComparer.EqualsVector3(position, a))
                        {
                            idA = i;
                            continue;
                        }
                        if (idB < 0 && VectorEqualityComparer.EqualsVector3(position, b))
                        {
                            idB = i;
                            continue;
                        }
                        if (idC < 0 && VectorEqualityComparer.EqualsVector3(position, c))
                        {
                            idC = i;
                            continue;
                        }
                    }
                }
                if (idA >= 0 && idB >= 0 && idC >= 0)
                {
                    for (int i = 0; i < startPart.Indices.Count; i += 3)
                    {
                        var v0 = startPart.Indices[i];
                        var v1 = startPart.Indices[i + 1];
                        var v2 = startPart.Indices[i + 2];
                        if ((v0 == idA || v0 == idB || v0 == idC) && (v1 == idA || v1 == idB || v1 == idC) && (v2 == idA || v2 == idB || v2 == idC))
                        {
                            startTriangleMirror = i;
                            break;
                        }
                    }
                }
            }
            if (!UpdateRadius(radius))
                return 0.0f;
            UpdateCoef(type, radius);
            return depth;
        }

        public bool UpdateRadius(float radius)
        {
            if (startPart == null || startTriangle < 0)
                return false;
            ClearSelection();
            shapeRadius = radius;

            var distance = new List<float>();
            var indices = new List<int>();
            if (!GetStartShapeData(startTriangle, ref distance, ref indices))
                return false;
            
            ProcessPart(startPart, indices, distance);

            if (startTriangleMirror != -1)
            {
                var x = ShapePoint.X;
                ShapePoint.X = -x;
                if (!GetStartShapeData(startTriangleMirror, ref distance, ref indices))
                    startTriangleMirror = -1;
                else
                {
                    var tmp = shapedParts.ToArray();
                    shapedParts.Clear();
                    ProcessPart(startPart, indices, distance, true);
                    foreach (var t in tmp)
                        if (!shapedParts.ContainsKey(t.Key))
                            shapedParts.Add(t.Key, t.Value);
                }
                ShapePoint.X = x;
            }

            foreach (var p in shapePoints)
                p.Value.UpdateColor();
            foreach (var part in shapedParts)
                part.Value.UpdateBuffers();
            return true;
        }

        private bool GetStartShapeData(int st, ref List<float> distance, ref List<int> indices)
        {
            distance.Clear();
            indices.Clear();
            var triangle = new[] { startPart.Indices[st], startPart.Indices[st + 1], startPart.Indices[st + 2] };
            foreach (var t in triangle)
            {
                var p = startPart.Points.FindIndex(r => r.Indices.Contains(t));
                if (p < 0)
                    return false;
                var dist = (startPart.Vertices[t].Position - ShapePoint).Length;
                if (dist >= shapeRadius)
                    continue;
                distance.Add(dist);
                indices.Add(p);
            }
            if (distance.Count == 0)
                return false;
            return true;
        }

        public void UpdateCoef(ShapeCoefType type, float radius)
        {
            if (startPart == null || startTriangle < 0)
                return;
            switch (type)
            {
                case ShapeCoefType.Grade4:
                    var grade4Radius = radius * radius * radius * radius;
                    foreach (var p in shapePoints)
                        p.Value.Coef = 1.0f - (p.Value.Distance * p.Value.Distance * p.Value.Distance * p.Value.Distance) / grade4Radius;
                    break;
                case ShapeCoefType.Qubed:
                    var qubedRadius = radius * radius * radius;
                    foreach (var p in shapePoints)
                        p.Value.Coef = 1.0f - (p.Value.Distance * p.Value.Distance * p.Value.Distance) / qubedRadius;
                    break;
                case ShapeCoefType.Squared:
                    var squaredRadius = radius * radius;
                    foreach (var p in shapePoints)
                        p.Value.Coef = 1.0f - (p.Value.Distance * p.Value.Distance) / squaredRadius;
                    break;
                case ShapeCoefType.SquarRoot:
                    var squaredRootRadius = (float)Math.Sqrt(radius);
                    foreach (var p in shapePoints)
                        p.Value.Coef = 1.0f - (float)Math.Sqrt(p.Value.Distance) / squaredRootRadius;
                    break;
            }
        }

        private void ClearSelection()
        {
            foreach (var part in headMeshController.RenderMesh.Parts)
            {
                for (var i = 0; i < part.Vertices.Length; i++)
                    part.Vertices[i].Color = Vector4.One;
                part.UpdateBuffers();
            }
            shapePoints.Clear();
            shapedParts.Clear();
        }

        public void EndShape()
        {
            ClearSelection();
            startPart = null;
            startTriangle = -1;
        }

        private void ProcessPart(RenderMeshPart part, List<int> startPoints, List<float> distance, bool isMirrored = false)
        {
            if (shapedParts.ContainsKey(part.Guid))
                return;
            shapedParts.Add(part.Guid, part);
            var nextParts = new Dictionary<Guid, ShapePartInfo>();
            for (var i = 0; i < startPoints.Count; i++)
            {
                var index = startPoints[i];
                var p = part.Points[index];
                if (!shapePoints.ContainsKey(p.Position))
                    shapePoints.Add(p.Position, new ShapePointData());
                var sp = shapePoints[p.Position];
                sp.PartsPoints.Add(new KeyValuePair<RenderMeshPart, int>(part, index));
                sp.Distance = Math.Min(sp.Distance, distance[i]);
                sp.IsMirrored = isMirrored;
            }
            for (var i = 0; i < startPoints.Count; i++)
            {
                ProcessPoint(part, startPoints[i], distance[i], ref nextParts, isMirrored);
            }
            foreach (var pt in nextParts)
            {
                var p = headMeshController.RenderMesh.Parts.First(r => r.Guid.Equals(pt.Key));
                ProcessPart(p, pt.Value.Points, pt.Value.Distance, isMirrored);
            }
        }

        private void ProcessPoint(RenderMeshPart part, int p, float distance, ref Dictionary<Guid, ShapePartInfo> nextParts, bool isMirrored = false)
        {
            var point = part.Points[p];
            var nearest = new Dictionary<int, float>();
            foreach (var n in point.Nearests)
            {
                var pos = part.Points[n].Position;
                if (isMirrored && pos.X * isLeft < 0.0f)
                    continue;
                var dist = (pos - point.Position).Length + distance;
                nearest.Add(n, dist);
            }
           
            //Берем все точки в радиусе заданном
            var where = nearest.Where(r => r.Value < shapeRadius).ToList();
            foreach (var n in where)
            {
                //оставляем только те у которых уменьшилась дистанция
                var pt = part.Points[n.Key];
                if (shapePoints.ContainsKey(pt.Position))
                {
                    var sp = shapePoints[pt.Position];
                    if (sp.PartsPoints.FindIndex(r => r.Value.Equals(n.Key) && r.Key.Guid.Equals(part.Guid)) < 0)
                    {
                        sp.PartsPoints.Add(new KeyValuePair<RenderMeshPart, int>(part, n.Key));
                    }
                    else
                    {
                        if (sp.Distance <= n.Value)
                            nearest[n.Key] = 10000.0f;
                    }
                    sp.Distance = Math.Min(sp.Distance, n.Value);
                    sp.IsMirrored = isMirrored;
                }
                //или новые точки
                else
                {
                    var sp = new ShapePointData();
                    sp.Distance = n.Value;
                    sp.IsMirrored = isMirrored;
                    sp.PartsPoints.Add(new KeyValuePair<RenderMeshPart, int>(part, n.Key));
                    shapePoints.Add(pt.Position, sp);
                }
            }

            foreach (var n in nearest.Where(r => r.Value < shapeRadius))
            {
                var np = part.Points[n.Key];
                foreach (var prt in headMeshController.RenderMesh.Parts.Where(r => r.Guid != part.Guid))
                {
                    int id = -1;

                    if (prt.PointsIndices.TryGetValue(np, out id))
                    {
                        if (nextParts.ContainsKey(prt.Guid))
                        {
                            var nextPart = nextParts[prt.Guid];
                            if (!nextPart.Points.Contains(id))
                            {
                                nextPart.Points.Add(id);
                                nextPart.Distance.Add(n.Value);
                            }
                            else
                            {
                                var i = nextPart.Points.IndexOf(id);
                                nextPart.Distance[i] = Math.Min(nextPart.Distance[i], n.Value);
                            }
                        }
                        else
                        {
                            nextParts.Add(prt.Guid, new ShapePartInfo {
                                Distance = new List<float> { n.Value },
                                Points = new List<int> { id }
                            });
                        }
                    }
                }
            }
            foreach (var n in nearest.Where(r => r.Value < shapeRadius))
                ProcessPoint(part, n.Key, n.Value, ref nextParts, isMirrored);
        }

        public void MoveShapePoint(Vector3 newPosition, out Dictionary<Guid, MeshUndoInfo> undoInfo)
        {
            undoInfo = new Dictionary<Guid, MeshUndoInfo>();
            var dv = newPosition - ShapePoint;
            ShapePoint = newPosition;
            foreach (var p in shapePoints)
                p.Value.Shape(dv, ref undoInfo);
            foreach (var part in shapedParts)
                part.Value.UpdateBuffers();
        }
    }
}
