using System.Collections.Generic;

namespace RH.HeadShop.Render.Obj
{
    public class ObjGroup
    {
        private readonly List<ObjFace> _faces = new List<ObjFace>();

        public ObjGroup(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public IList<ObjFace> Faces { get { return _faces; } }

        public void AddFace(ObjFace face)
        {
            _faces.Add(face);
        }
    }
}
