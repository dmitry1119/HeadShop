using System;
using System.Collections.Generic;
using OpenTK;
using RH.HeadEditor.Data;
using RH.HeadEditor.Helpers;
using RH.HeadShop.Helpers;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Meshes;

namespace RH.HeadShop.Render.Controllers
{
    public class IHistoryItem
    {
        public int Group = -1;
        public virtual void Undo() { }
    }

    public class HistoryMeshes : IHistoryItem
    {
        public List<HistoryMesh> Meshes = new List<HistoryMesh>();

        public HistoryMeshes()
        {

        }
        public HistoryMeshes(IEnumerable<DynamicRenderMesh> meshes)
        {
            foreach (var mesh in meshes)
            {
                var historyMesh = new HistoryMesh(mesh);
                Meshes.Add(historyMesh);
            }
        }

        public HistoryMesh AddMesh(DynamicRenderMesh mesh)
        {
            var historyMesh = new HistoryMesh(mesh);
            Meshes.Add(historyMesh);
            return historyMesh;
        }

        public void Undo()
        {
            foreach (var mesh in Meshes)
                mesh.Undo();
        }
    }
    /// <summary> Undo for cutting  </summary>
    public class HistoryMesh : IHistoryItem
    {
        public List<Guid> ToDelete = new List<Guid>();

        private readonly Vector3 position;
        private readonly List<Vector3> vertexPositions;
        private readonly List<Vector2> textureCoordinates;
        private readonly List<int> vertexIndices;
        private readonly string title;
        private readonly Matrix4 transform;
        private readonly string texturePath;
        private readonly string alphaTexturePath;
        private readonly float textureAngle;
        private readonly float textureSize;
        private readonly Guid id;

        private readonly bool inPartsLibrary;
        private readonly bool isVisible;

        public HistoryMesh(DynamicRenderMesh mesh)
        {
            if (mesh != null)           // if create mesh duplicate - should just remove it
            {
                vertexPositions = new List<Vector3>();
                textureCoordinates = new List<Vector2>();
                vertexIndices = new List<int>();
                var index = 0;
                foreach (var vertex in mesh.vertexArray)
                {
                    vertexPositions.Add(vertex.Position);
                    textureCoordinates.Add(vertex.TexCoord);
                    vertexIndices.Add(index++);
                }

                title = mesh.Title;
                transform = mesh.Transform;
                texturePath = mesh.Material.DiffuseTextureMap;
                alphaTexturePath = mesh.Material.TransparentTextureMap;
                textureAngle = mesh.TextureAngle;
                textureSize = mesh.TextureSize;
                id = mesh.Id;
                position = mesh.Position;

                inPartsLibrary = ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(mesh.Title);
                isVisible = mesh.IsVisible;
            }
        }

        public override void Undo()
        {
            foreach (var meshId in ToDelete)
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Contains(meshId))
                {
                    var mesh = ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes[meshId];
                    ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Remove(mesh);

                    if (ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(mesh.Title))
                        ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes[mesh.Title].Remove(mesh);

                    mesh.Destroy();
                }
            }

            if (vertexPositions != null)
            {
                var newMesh = new DynamicRenderMesh(MeshType.Hair);
                newMesh.Create(vertexPositions, textureCoordinates, vertexIndices, texturePath, alphaTexturePath, textureAngle, textureSize);
                newMesh.Title = title;
                newMesh.groupName = title;
                newMesh.Transform = transform;
                newMesh.Position = position;
                newMesh.Material.DiffuseTextureMap = texturePath;
                newMesh.Material.TransparentTextureMap = alphaTexturePath;
                newMesh.TextureAngle = textureAngle;
                newMesh.TextureSize = textureSize;
                newMesh.Id = id;
                newMesh.IsVisible = isVisible;

                if (inPartsLibrary)
                    ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes[newMesh.Title].Add(newMesh);

                ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Add(newMesh);
            }

            ProgramCore.MainForm.frmParts.UpdateList();
        }
    }

    /// <summary> Undo for shaping  </summary>
    public class HistoryShape : IHistoryItem
    {
        private readonly Dictionary<Guid, ShapeMeshInfo> shapeInfo = new Dictionary<Guid, ShapeMeshInfo>();
        public HistoryShape(Dictionary<Guid, ShapeMeshInfo> shapeInfos)
        {
            foreach (var info in shapeInfos)
                shapeInfo.Add(info.Key, info.Value.Clone());
        }

        public override void Undo()
        {
            foreach (var meshInfo in shapeInfo)
            {
                if (!ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Contains(meshInfo.Key))
                    continue;

                var mesh = ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes[meshInfo.Key];
                mesh.SetVertices(meshInfo.Value.Vertices, meshInfo.Value.Indices);
            }
        }
    }

    /// <summary> Откат изменения головы (точки) </summary>
    public class HistoryHeadShapeDots : IHistoryItem
    {
        private readonly Dictionary<Guid, MeshUndoInfo> partsInfo = new Dictionary<Guid, MeshUndoInfo>();
        private readonly List<MirroredHeadPoint> shapeDotsInfo = new List<MirroredHeadPoint>();

        public HistoryHeadShapeDots(Dictionary<Guid, MeshUndoInfo> partsInfo, IEnumerable<MirroredHeadPoint> shapeDots)
        {
            foreach (var info in partsInfo)
                this.partsInfo.Add(info.Key, info.Value.Clone());

            foreach (var dot in shapeDots)
                shapeDotsInfo.Add(dot.Clone() as MirroredHeadPoint);
        }

        public override void Undo()
        {
            ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Clear();         // все точки нужны
            foreach (var dot in shapeDotsInfo)
            {
                var newDot = dot.Clone() as MirroredHeadPoint;
                ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Add(newDot);
            }
            ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.Transform(ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.ToArray(), false);

            foreach (var meshInfo in partsInfo)
            {
                if (!ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts.Contains(meshInfo.Key))
                    continue;

                var mesh = ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts[meshInfo.Key];
                mesh.Undo(meshInfo.Value);
            }

            ProgramCore.MainForm.ctrlTemplateImage.UpdateUserCenterPositions(false, true);
        }
    }

    /// <summary> Откат изменения головы (линии) </summary>
    public class HistoryHeadShapeLines : IHistoryItem
    {
        private readonly Dictionary<Guid, MeshUndoInfo> partsInfo = new Dictionary<Guid, MeshUndoInfo>();
        private readonly Collection<HeadLine> Lines = new Collection<HeadLine>();
        private readonly TexturingInfo TexturingInfo;
        private readonly bool isProfile;

        public HistoryHeadShapeLines(Dictionary<Guid, MeshUndoInfo> partsInfo, IEnumerable<HeadLine> lines, TexturingInfo texturingInfo, bool isProfile)
        {
            if (partsInfo != null)
                foreach (var info in partsInfo)
                    this.partsInfo.Add(info.Key, info.Value.Clone());
            else this.partsInfo = null;

            if (lines != null)
                foreach (var line in lines)
                {
                    var currentLine = new HeadLine();
                    Lines.Add(currentLine);
                    foreach (var point in line)
                        currentLine.Add(point.Clone() as MirroredHeadPoint);
                }

            TexturingInfo = texturingInfo.Clone();
            this.isProfile = isProfile;
        }

        public override void Undo()
        {
            if (partsInfo == null)
            {
                ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Clear();
                foreach (var line in Lines)
                {
                    var currentLine = new HeadLine();
                    ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(currentLine);
                    foreach (var point in line)
                        currentLine.Add(point.Clone() as MirroredHeadPoint);
                }
            }
            else
            {
                foreach (var meshInfo in partsInfo)
                {
                    if (!ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts.Contains(meshInfo.Key))
                        continue;

                    var mesh = ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts[meshInfo.Key];
                    mesh.Undo(meshInfo.Value);
                }

                if (isProfile)
                    ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeProfileInfo = TexturingInfo.Clone();
                else
                    ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeInfo = TexturingInfo.Clone();

                ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.TransformRects();
            }

            ProgramCore.MainForm.ctrlTemplateImage.UpdateUserCenterPositions(false, true);
        }
    }

    /// <summary> Откат изменения головы (инструмент шейп) </summary>
    public class HistoryHeadAutoDots : IHistoryItem
    {
        private readonly List<MirroredHeadPoint> autoDotsInfo = new List<MirroredHeadPoint>();

        public HistoryHeadAutoDots(IEnumerable<MirroredHeadPoint> autoDots)
        {
            foreach (var dot in autoDots)
                autoDotsInfo.Add(dot.Clone() as MirroredHeadPoint);
        }

        public override void Undo()
        {
            ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.Clear();         // все точки нужны
            foreach (var dot in autoDotsInfo)
                ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.Add(dot.Clone() as MirroredHeadPoint);

            if (ProgramCore.Project.AutodotsUsed)
            {
                ProgramCore.MainForm.ctrlRenderControl.CalcReflectedBitmaps();
                ProgramCore.MainForm.ctrlRenderControl.headController.EndAutodots(false);
                ProgramCore.MainForm.ctrlRenderControl.ApplySmoothedTextures();
            }
            ProgramCore.MainForm.ctrlTemplateImage.UpdateUserCenterPositions(false, true);
        }
    }

    public class HistoryHeadShape : IHistoryItem
    {
        private readonly Dictionary<Guid, MeshUndoInfo> partsInfo = new Dictionary<Guid, MeshUndoInfo>();

        public HistoryHeadShape(Dictionary<Guid, MeshUndoInfo> partsInfo)
        {
            foreach (var info in partsInfo)
                this.partsInfo.Add(info.Key, info.Value.Clone());
        }

        public override void Undo()
        {
            foreach (var meshInfo in partsInfo)
            {
                if (!ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts.Contains(meshInfo.Key))
                    continue;

                var mesh = ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts[meshInfo.Key];
                mesh.Undo(meshInfo.Value);
            }
        }
    }

    /// <summary> Контроллер истории изменений </summary>
    public class HistoryController
    {
        public int currentGroup = -1;

        readonly List<IHistoryItem> items = new List<IHistoryItem>();
        int currentIndex = -1;

        public bool CanUndo
        {
            get
            {
                return currentIndex >= 0;
            }
        }
        public void Undo()
        {
            if (!CanUndo)
                return;
            var currentGroup = items[currentIndex].Group;

            items[currentIndex].Undo();
            currentIndex--;

            while (CanUndo && currentGroup != -1 && currentGroup == items[currentIndex].Group)      // для отката линий у башки. что бы вместе с трансформом откатились и линии
                currentIndex--;
        }
        public void Add(IHistoryItem item)
        {
            CutOffHistory();

            if (items.Count > 20)
            {
                items.RemoveAt(0);
                currentIndex--;
            }

            items.Add(item);
            currentIndex++;
        }

        void CutOffHistory()
        {
            var index = currentIndex + 1;
            if (index < items.Count)
                items.RemoveRange(index, items.Count - index);
        }
    }
}
