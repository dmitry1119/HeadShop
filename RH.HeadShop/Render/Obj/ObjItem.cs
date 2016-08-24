using System.Collections.Generic;
using OpenTK;

namespace RH.HeadShop.Render.Obj
{
    public class ObjItem
    {
        public ObjExport ObjExport;
        /// <summary> Vertices </summary>
        public List<Vector3> Vertices;
        /// <summary> Texture coords </summary>
        public List<Vector2> TextureCoords;

        public List<Vector3> Normals;

        public Dictionary<ObjMaterial, ObjGroup> Groups;

        /// <summary> List of materials </summary>
        public Dictionary<string, ObjMaterial> Materials;

        public bool accessoryByHeadShop;       // special flag. we should devide items by groups, because it's different accessories.
        public bool modelByHeadShop;

        public ObjItem(bool needExporter)
        {
            ObjExport = needExporter ? new ObjExport() : null;
            
            Vertices = new List<Vector3>();
            TextureCoords = new List<Vector2>();
            Normals = new List<Vector3>();

            Groups = new Dictionary<ObjMaterial, ObjGroup>();

            Materials = new Dictionary<string, ObjMaterial>();
        }
    }
}
