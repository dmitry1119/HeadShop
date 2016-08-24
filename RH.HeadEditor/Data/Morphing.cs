using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using RH.HeadEditor.Helpers;

namespace RH.HeadEditor.Data
{
    public class PartMorphInfo
    {
        public float Delta = 0.0f;
        public List<Vector3> PointsMorph = new List<Vector3>();

        public static bool CreatePartMorphInfo(List<Vector3> vertices, RenderMeshPart part, float scale, out PartMorphInfo info)
        {
            var result = new PartMorphInfo();

            var pointnsDict = new Dictionary<Vector3, int>(new VectorEqualityComparer());
            foreach (var v in vertices)
            {
                if (!pointnsDict.ContainsKey(v))
                {
                    pointnsDict.Add(v, result.PointsMorph.Count);
                    result.PointsMorph.Add(v);
                }
            }
            if (result.PointsMorph.Count != part.Points.Count)
            {
                info = null;
                return false;
            }

            for (int i = 0; i < result.PointsMorph.Count; i++)
            {
                var index = part.Points[i].Indices[0];
                var p = (part.BaseVertices ?? part.Vertices)[index].OriginalPosition;
                result.PointsMorph[i] = result.PointsMorph[i] - p / scale;
            }

            info = result;
            return true;
        }
    }

    public class Morphing
    {
        private const float MinFloat = 0.000001f;

        private static void MorphPart(RenderMeshPart part, IList<PartMorphInfo> morphInfos)
        {
            var count = morphInfos.Any() ? 1.0f / morphInfos.Count() : 1.0f;
            if (part.IsMirrored)
            {
                var verticesDictionary = new Dictionary<uint, Vector3>();
                for (int i = 0; i < part.Points.Count; i++)
                {
                    var point = part.Points[i];
                    var delta = morphInfos.Aggregate(Vector3.Zero, (current, mi) => current + mi.PointsMorph[i] * mi.Delta) * count;
                    foreach (var index in point.Indices)
                    {
                        if (!verticesDictionary.ContainsKey(index))
                            verticesDictionary.Add(index, delta);
                    }
                }
                for (int i = 0; i < part.Vertices.Length; i++)
                {
                    var vertex = part.Vertices[i];
                    if (vertex.OriginalPosition.X >= 0.0f && vertex.OriginalPosition.X <= 1.0f)
                    {
                        var a = (uint)Math.Abs(vertex.OriginalPosition.Y);
                        var b = (uint)Math.Abs(vertex.OriginalPosition.Z);
                        var point0 = part.BaseVertices[a].Position + verticesDictionary[a];
                        var point1 = part.BaseVertices[b].Position + verticesDictionary[b];
                        vertex.Position = point0 + (point1 - point0) * vertex.OriginalPosition.X;
                    }
                    else
                    {
                        var p = (uint)Math.Abs(vertex.OriginalPosition.X) - 2;
                        var delta = verticesDictionary[p];
                        vertex.Position = part.BaseVertices[p].Position + delta;
                        if (vertex.Position.X > 0.0f == part.IsLeftToRight)
                            vertex.Position.X = 0.0f;
                        if (vertex.OriginalPosition.X < 0.0f)
                            vertex.Position.X *= -1.0f;
                    }
                    part.Vertices[i] = vertex;
                }
            }
            else
            {
                for (int i = 0; i < part.Points.Count; i++)
                {
                    var point = part.Points[i];
                    var delta = morphInfos.Aggregate(Vector3.Zero, (current, mi) => current + mi.PointsMorph[i] * mi.Delta) * count;
                    foreach (var index in point.Indices)
                        part.Vertices[index].Position = point.Position + delta;
                }
            }
            part.UpdateNormals();
        }

        public static void Morph(List<Dictionary<Guid, PartMorphInfo>> morphingList, RenderMesh mesh)
        {
            var morphInfos = new List<PartMorphInfo>();
            foreach (var part in mesh.Parts)
            {
                morphInfos.Clear();
                morphInfos.AddRange(from m in morphingList
                                    where m.ContainsKey(part.Guid)
                                    select m[part.Guid]);
                MorphPart(part, morphInfos.Where(mi => Math.Abs(mi.Delta) > MinFloat).ToList());
            }
        }
    }
}
