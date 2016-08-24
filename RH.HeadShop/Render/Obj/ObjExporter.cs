using System;
using System.Collections.Generic;
using Assimp;
using OpenTK;

namespace RH.HeadShop.Render.Obj
{
    public class ObjExportFace
    {
        public int VertexCount = 3;
        public int TriangleIndex0 = -1;
        public int TriangleIndex1 = -1;
        public List<ObjFaceVertex> FaceVertices = new List<ObjFaceVertex>(); 

        public ObjExportFace(int vc, IEnumerable<ObjFaceVertex> faceVertices)
        {
            VertexCount = vc;
            FaceVertices.AddRange(faceVertices);
        }
    }

    public class ObjExportGroup
    {
        public int StartFaceIndex = 0;
        public int EndFaceIndex = 0;
        public String Group = String.Empty;
        public bool IsValid { get { return EndFaceIndex >= StartFaceIndex; } }
    }

    public class ObjExportMaterial
    {
        public String Material = String.Empty;
        public List<ObjExportGroup> Groups = new List<ObjExportGroup>();
    }

    public class ObjExport
    {
        public List<Vector3> Vertices = new List<Vector3>();
        public List<Vector2> TexCoords = new List<Vector2>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<ObjExportMaterial> MaterialsGroups = new List<ObjExportMaterial>();
        public List<ObjExportFace> Faces = new List<ObjExportFace>();
        public float Scale = 1.0f;
        public Vector3 Delta
        {
            get
            {
                return _delta;
            }
            set
            {
                _delta = value;
                _delta.Y += 0.0060975609f;
            }
        }
        private Vector3 _delta = Vector3.Zero;

        public void SetData(Vector3 v, Vector2 vt, Vector3 vn, ObjExportFace face, int l)
        {
            Vertices[face.FaceVertices[l].VertexIndex - 1] = v * Scale + Delta;
            if (Normals.Count > 0)
                Normals[face.FaceVertices[l].NormalIndex - 1] = vn;
            if (TexCoords.Count > 0)
                TexCoords[face.FaceVertices[l].TextureIndex - 1] = vt;
        }
    }
}
