using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using RH.HeadEditor.Data;
using RH.HeadEditor.Helpers;

namespace RH.HeadEditor
{
    public class HeadMeshesController
    {
        #region Var
        public RenderMesh RenderMesh = new RenderMesh();
        public TexturingInfo TexturingInfo = new TexturingInfo();
        #endregion

        #region Public
        public void Destroy()
        {
            RenderMesh.Destroy();
        }

        public void UpdateShape(ref TexturingInfo shapeInfo)
        {
            foreach (var p in RenderMesh.Parts)
                p.UpdateShape(ref shapeInfo);
        }

        public void UpdateProfileShape(ref TexturingInfo shapeInfo)
        {
            foreach (var p in RenderMesh.Parts)
                p.UpdateProfileShape(ref shapeInfo);
        }

        public void UpdateTexCoors(IEnumerable<Vector2> texCoords)
        {
            TexturingInfo.TexCoords = texCoords.ToArray();
            foreach (var p in RenderMesh.Parts)
                p.UpdateTexCoords(ref TexturingInfo);
        }

        public void InitializeShapingProfile(ref TexturingInfo shapeInfo)
        {
            foreach (var p in RenderMesh.Parts)
                p.FillPointsInfo(ref shapeInfo, true, true);
        }

        public void InitializeShaping(ref TexturingInfo shapeInfo)
        {
            foreach (var p in RenderMesh.Parts)
                p.FillPointsInfo(ref shapeInfo, true, false);
        }

        public void InitializeTexturing(IEnumerable<HeadPoint> points, IEnumerable<Int32> indices)
        {
            TexturingInfo.Points = new HeadPoints<HeadPoint>();
            TexturingInfo.Points.AddRange(points);

            TexturingInfo.Indices = indices.ToArray();
            foreach (var p in RenderMesh.Parts)
                p.FillPointsInfo(ref TexturingInfo, false, false);
        }

        public void FinishCreating()
        {
            foreach (var part in RenderMesh.Parts)
                part.UpdateBuffers();
        }

        public float SetSize(float diagonal)
        {
            float scale = RenderMesh.SetSize(diagonal);
            foreach (var part in RenderMesh.Parts)
                part.UpdateBuffers();
            return scale;
        }

        public Vector3 FinishCreating(float widthToHeight)
        {
            var center = RenderMesh.Transform(widthToHeight);
            foreach (var part in RenderMesh.Parts)
                part.UpdateBuffers();
            return center;
        }

        public bool CreateMeshPart(MeshPartInfo info)
        {
            var part = new RenderMeshPart();
            if (part.Create(info))
            {
                RenderMesh.AddPart(part);
                return true;
            }
            return false;
        }

        public void Draw(bool debug)
        {
            RenderMesh.Draw(debug);
        }

        public void Mirror(bool leftToRight, float axis)
        {
            foreach (var p in RenderMesh.Parts)
                p.Mirror(leftToRight, axis);
        }

        public void UndoMirror()
        {
            foreach (var p in RenderMesh.Parts)
                p.UndoMirror();
        }

        public void GetUndoInfo(out Dictionary<Guid, MeshUndoInfo> undoInfo)
        {
            undoInfo = RenderMesh.Parts.ToDictionary(part => part.Guid, part => part.GetUndoInfo());
        }

        public void Undo(Dictionary<Guid, MeshUndoInfo> undoInfo)
        {
            MeshUndoInfo info = null;
            foreach (var part in RenderMesh.Parts.Where(part => undoInfo.TryGetValue(part.Guid, out info)))
            {
                part.Undo(info);
            }
        }

        public void UpdateNormals()
        {
            foreach (var p in RenderMesh.Parts)
                p.UpdateNormals();
        }

        #endregion
    }
}
