using System.Collections.Generic;

namespace RH.HeadShop.Render.Obj
{
    public class ObjFace
    {
        public int ObjExportIndex { get; set; }

        private readonly List<ObjFaceVertex> _vertices = new List<ObjFaceVertex>();

        public List<ObjFaceVertex> Vertices  { get { return _vertices; }}

        public void AddVertex(ObjFaceVertex vertex)
        {
            _vertices.Add(vertex);
        }

        public ObjFaceVertex this[int i]
        {
            get { return _vertices[i]; }
        }

        public int Count
        {
            get { return _vertices.Count; }
        }
    }

    public struct ObjFaceVertex
    {
        public ObjFaceVertex(int vertexIndex, int textureIndex, int normalIndex)
            : this()
        {
            VertexIndex = vertexIndex;
            TextureIndex = textureIndex;
            NormalIndex = normalIndex;
        }

        public int VertexIndex { get; set; }
        public int TextureIndex { get; set; }
        public int NormalIndex { get; set; }
    }
}
