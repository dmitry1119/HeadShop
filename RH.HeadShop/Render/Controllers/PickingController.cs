using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadEditor.Data;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;
using System.IO;

namespace RH.HeadShop.Render.Controllers
{
    /// <summary> Контроллер для выделения мешей </summary>
    public class PickingController
    {
        #region Var
        public ObjExport ObjExport = null;
        private const int BufferSize = 256;

        public delegate void SelectedMeshChanged();
        /// <summary> User click to a specific mesh to modify it </summary>
        public event SelectedMeshChanged OnSelectedMeshChanged;

        private readonly Camera camera;
        public DynamicRenderMeshes AccesoryMeshes;
        public DynamicRenderMeshes HairMeshes;
        public List<DynamicRenderMesh> HeadMeshes = new List<DynamicRenderMesh>();

        public readonly DynamicRenderMeshes SelectedMeshes;    // only one attributes may selected, but hairs - maybe more than one. should check it

        public Dictionary<DynamicRenderMesh, Vector4> SelectedColor = new Dictionary<DynamicRenderMesh, Vector4>();
        private static readonly Vector4 selectionColor = new Vector4(1f, 1f, 0f, 1f);

        #endregion

        public PickingController(Camera c)
        {
            camera = c;

            AccesoryMeshes = new DynamicRenderMeshes();
            HairMeshes = new DynamicRenderMeshes();

            SelectedMeshes = new DynamicRenderMeshes();
            SelectedMeshes.CollectionChanged += SelectedMeshes_CollectionChanged;
            SelectedMeshes.BeforeClear += SelectedMeshes_BeforeClear;
            SelectedMeshes.ItemsRemoved += SelectedMeshes_ItemsRemoved;
        }

        #region Event's

        private void SelectedMeshes_ItemsRemoved(DynamicRenderMesh[] items)
        {
            foreach (var item in items)
                SetPointColor(item);
        }
        private void SelectedMeshes_BeforeClear(object sender, System.EventArgs e)
        {
            SetPointColor(Vector4.Zero);
        }
        private void SelectedMeshes_CollectionChanged(object sender, System.EventArgs e)
        {
            if (SelectedMeshes.Count != 0)
                SetPointColor(Vector4.Zero);

            SelectedColor.Clear();
            foreach (var mesh in SelectedMeshes)
                SelectedColor.Add(mesh, mesh.Material.DiffuseColor);
            SetPointColor(selectionColor);

            if (OnSelectedMeshChanged != null)
                OnSelectedMeshChanged();
        }

        #endregion

        #region Supported void's

        public void PickMatrix(double x, double y, double deltax, double deltay, int[] viewport)
        {
            if (deltax <= 0 || deltay <= 0)
            {
                return;
            }

            GL.Translate((viewport[2] - 2 * (x - viewport[0])) / deltax, (viewport[3] - 2 * (y - viewport[1])) / deltay, 0);
            GL.Scale(viewport[2] / deltax, viewport[3] / deltay, 1.0);
        }

        public void SetPointColor(Vector4 color)
        {
            if (color == Vector4.Zero)
            {
                foreach (var mesh in SelectedMeshes)
                {
                    if (!SelectedColor.ContainsKey(mesh))
                        continue;

                    mesh.Material.DiffuseColor = SelectedColor[mesh];
                }
            }
            else
            {
                foreach (var mesh in SelectedMeshes)
                    mesh.Material.DiffuseColor = color;
            }
        }

        private void SetPointColor(DynamicRenderMesh mesh)
        {
            if (SelectedColor.ContainsKey(mesh))
                mesh.Material.DiffuseColor = SelectedColor[mesh];
        }

        public void UpdateSelectedFace(int x, int y)
        {
            var selectedId = -1;
            var viewport = new int[4];
            var selectBuffer = new uint[BufferSize * 4];
            GL.SelectBuffer(BufferSize * 4, selectBuffer);
            GL.RenderMode(RenderingMode.Select);
            GL.InitNames();
            GL.PushName(-0x01);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            GL.GetInteger(GetPName.Viewport, viewport);

            var doubleArray = new double[16];
            GL.GetDouble(GetPName.ProjectionMatrix, doubleArray);

            GL.LoadIdentity();
            PickMatrix(x, viewport[3] - y, 0.001f, 0.001f, viewport);
            GL.MultMatrix(doubleArray);

            camera.PutCamera();

            GL.PushMatrix();
            GL.Enable(EnableCap.DepthTest);
            var name = 0;

            foreach (var mesh in HairMeshes)
            {
                if (!mesh.IsVisible)
                {
                    ++name;
                    continue;
                }

                GL.LoadName(++name);
                GL.PushMatrix();
                GL.MultMatrix(ref mesh.Transform);
                mesh.SimpleDraw();
                GL.PopMatrix();
            }

            name = 99999;
            foreach (var mesh in AccesoryMeshes)
            {
                if (!mesh.IsVisible)
                {
                    ++name;
                    continue;
                }

                GL.LoadName(++name);
                GL.PushMatrix();
                GL.MultMatrix(ref mesh.Transform);
                mesh.SimpleDraw();
                GL.PopMatrix();
            }

            GL.PopMatrix();

            GL.Flush();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Flush();

            var hits = GL.RenderMode(RenderingMode.Render);
            var closest = uint.MaxValue;

            for (var i = 0; i < hits; i++)
            {
                var distance = selectBuffer[i * 4 + 1];

                if (closest >= distance)
                {
                    closest = distance;
                    selectedId = (int)selectBuffer[i * 4 + 3];
                }
            }
            if (selectedId > 0)
            {
                if (selectedId >= 100000)
                {
                    SelectedMeshes.Clear();
                    var mesh = AccesoryMeshes[selectedId - 100000];
                    SelectedMeshes.Add(mesh);
                }
                else
                {
                    if (SelectedMeshes.Any(z => z.meshType == MeshType.Accessory))
                        SelectedMeshes.Clear();

                    var mesh = HairMeshes[selectedId - 1];
                    if (!SelectedMeshes.Contains(mesh))
                        SelectedMeshes.Add(mesh);
                    else
                    {
                        SetPointColor(mesh);
                        SelectedMeshes.Remove(mesh);
                    }
                }
            }
            else
                SelectedMeshes.Clear();
        }

        public List<DynamicRenderMesh> AddMehes(string path, MeshType type, bool fromDragAndDrop, ManType manType, bool needExporter)
        {
            return AddMehes(path, type, fromDragAndDrop, manType, string.Empty, needExporter);
        }

        public List<DynamicRenderMesh> AddMehes(string path, MeshType type, bool fromDragAndDrop, ManType manType, string animationPath, bool needExporter)
        {
            var result = new List<DynamicRenderMesh>();

            var objModel = ObjLoader.LoadObjFile(path, needExporter);
            if (objModel == null)
            {
                ProgramCore.EchoToLog(string.Format("Can't load obj model '{0}'", path), EchoMessageType.Error);
                return result;
            }

            switch (type)
            {
                case MeshType.Hair:
                    {
                        var fi = new FileInfo(path);
                        var objModelNull = ObjLoader.LoadObjFile(Path.Combine(fi.DirectoryName, fi.Name.Replace(fi.Extension, string.Format("_null{0}", fi.Extension))));
                        if (objModelNull != null &&
                            (objModelNull.Groups.Count != objModel.Groups.Count ||
                            objModel.Vertices.Count != objModelNull.Vertices.Count))
                            //  objModel.TextureCoords.Count != objModelNull.TextureCoords.Count))
                            objModelNull = null;

                        result = LoadHairMeshes(objModel, objModelNull, fromDragAndDrop, manType, MeshType.Hair);
                        foreach (var renderMesh in result)
                            HairMeshes.Add(renderMesh);
                        break;
                    }
                case MeshType.Accessory:
                    return objModel.accessoryByHeadShop ? LoadSpecialAccessoryMesh(objModel) : new List<DynamicRenderMesh> { LoadAccessoryMesh(objModel) };
                case MeshType.Head:
                    {
                        var tempPluginTexture = string.Empty;
                        if (ProgramCore.PluginMode)
                        {
                            var folderPath = Path.Combine(Application.StartupPath, "Models\\Model", manType.GetObjDirPath());
                            switch (ProgramCore.Project.ManType)
                            {
                                case ManType.Male:
                                    tempPluginTexture = Path.Combine(folderPath, "Maps", "RyNevio_faceB.jpg");
                                    break;
                                case ManType.Female:
                                    tempPluginTexture = Path.Combine(folderPath, "Maps", "RyBelle_face.jpg");
                                    break;
                                case ManType.Child:
                                    tempPluginTexture = Path.Combine(folderPath, "Maps", "AC_KidsRRHBy.jpg");
                                    break;
                                default:
                                    tempPluginTexture = Path.Combine(Application.StartupPath, "Plugin", "fsRndColor.png");
                                    break;
                            }
                        }

                        foreach (var mesh in HeadMeshes)
                            mesh.Destroy();
                        HeadMeshes.Clear();

                        var objModelFull = animationPath == string.Empty ? null : ObjLoader.LoadObjFile(animationPath);
                        if (objModelFull != null &&
                            (objModelFull.Groups.Count != objModel.Groups.Count ||
                            objModel.Vertices.Count != objModelFull.Vertices.Count ||
                            objModel.TextureCoords.Count != objModelFull.TextureCoords.Count))
                            objModelFull = null;
                        LastTriangleIndex = 0;
                        result = LoadHairMeshes(objModel, objModelFull, fromDragAndDrop, manType, MeshType.Head);
                        var meshPartInfos = new List<MeshPartInfo>();
                        var a = new Vector3(99999.0f, 99999.0f, 99999.0f);
                        var b = new Vector3(-99999.0f, -99999.0f, -99999.0f);
                        foreach (var renderMesh in result)
                        {
                            HeadMeshes.Add(renderMesh);

                            if (ProgramCore.PluginMode &&  ProgramCore.MainForm.PluginUvGroups.Contains(renderMesh.Material.Name))
                            {
                                if (string.IsNullOrEmpty(renderMesh.Material.DiffuseTextureMap))
                                    renderMesh.Material.DiffuseTextureMap = tempPluginTexture;
                                else if (!File.Exists(renderMesh.Material.DiffuseTextureMap))
                                    renderMesh.Material.DiffuseTextureMap = tempPluginTexture;
                            }

                            var meshPartInfo = new MeshPartInfo
                            {
                                VertexPositions = renderMesh.GetVertices(),
                                TextureCoords = renderMesh.GetTexCoords(),
                                PartName = renderMesh.Title,
                                Color = renderMesh.Material.DiffuseColor,
                                Texture = renderMesh.Material.Texture,
                                TransparentTexture = renderMesh.Material.TransparentTexture,
                                TextureName = renderMesh.Material.DiffuseTextureMap,
                                TransparentTextureName = renderMesh.Material.TransparentTextureMap
                            };                  // создаем инфу о голове. для работы с headshop
                            GetAABB(ref a, ref b, meshPartInfo.VertexPositions);
                            meshPartInfos.Add(meshPartInfo);

                        }
                        Vector3 dv = Vector3.Zero;
                        foreach (var meshPartInfo in meshPartInfos)
                        {
                            dv = MoveToPosition(ref meshPartInfo.VertexPositions, a, b, Vector3.Zero);
                            ProgramCore.MainForm.ctrlRenderControl.headMeshesController.CreateMeshPart(meshPartInfo);
                        }

                        ObjExport = objModel.ObjExport;
                        if (ObjExport != null)
                            ObjExport.Delta = -dv;
                        ProgramCore.MainForm.ctrlRenderControl.headMeshesController.FinishCreating();

                        // ProgramCore.MainForm.ctrlRenderControl.headMeshesController.InitializeTexturing(HeadController.GetDots(ProgramCore.Project.ManType), HeadController.GetIndices(ProgramCore.Project.ManType));
                        break;
                    }
                default:
                    return result;
            }
            return result;
        }

        private int LastTriangleIndex = 0;

        private void SetFaceTriangleIndex(ObjFace face, ObjItem objModel)
        {
            if (objModel.ObjExport != null && face.ObjExportIndex > -1)
            {
                objModel.ObjExport.Faces[face.ObjExportIndex].TriangleIndex0 = LastTriangleIndex++;
                if (face.Count == 4)
                    objModel.ObjExport.Faces[face.ObjExportIndex].TriangleIndex1 = LastTriangleIndex++;
            }
        }

        private void GetObjFace(ObjFace face, ObjItem objModel,
                                             ref List<float> vertexPositions, ref List<float> vertexNormals, ref List<float> vertexTextureCoordinates, ref List<float> vertexBoneWeights, ref List<float> vertexBoneIndices, ref List<uint> indeces)
        {
            if (face.Count == 3)
            {
                for (var i = 0; i < face.Count; i++)
                {
                    var faceVertex = face[i];
                    ObjLoader.AppendObjTriangle(objModel, faceVertex, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                }
                SetFaceTriangleIndex(face, objModel);
            }
            else if (face.Count == 4)
            {
                var faceVertex0 = face[0];
                var faceVertex1 = face[1];
                var faceVertex2 = face[2];
                var faceVertex3 = face[3];

                ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                ObjLoader.AppendObjTriangle(objModel, faceVertex1, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);

                ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                ObjLoader.AppendObjTriangle(objModel, faceVertex3, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                SetFaceTriangleIndex(face, objModel);
            }
        }


        public static float ScaleChild = 1.2f;
        public static float ScaleMan = 1f;

        public static float GetHeadScale(ManType manType)
        {
            switch (manType)
            {
                case ManType.Child:
                    return ScaleChild;
                case ManType.Male:
                case ManType.Female:
                    return ScaleMan;
            }
            return 1f;
        }
        public static float GetHairScale(ManType manType)
        {
            switch (manType)
            {
                case ManType.Child:
                    return 246f;
                case ManType.Male:
                case ManType.Female:
                    return 246f;
            }
            return 246f;
        }
        private List<DynamicRenderMesh> LoadHairMeshes(ObjItem objModel, ObjItem objModelNull, bool fromDragAndDrop, ManType manType, MeshType meshType)
        {
            var result = new List<DynamicRenderMesh>();
            var vertexPositions = new List<float>();
            var vertexNormals = new List<float>();
            var vertexTextureCoordinates = new List<float>();
            var vertexBoneIndices = new List<float>();
            var vertexBoneWeights = new List<float>();
            var indeces = new List<uint>();

            var vertexPositionsNull = new List<float>();
            var vertexNormalsNull = new List<float>();
            var vertexTextureCoordinatesNull = new List<float>();
            var vertexBoneIndicesNull = new List<float>();
            var vertexBoneWeightsNull = new List<float>();
            var indecesNull = new List<uint>();

            var groupsNull = objModelNull == null ? new Dictionary<ObjMaterial, ObjGroup>.Enumerator() : objModelNull.Groups.GetEnumerator();
            ObjGroup groupNull;
            foreach (var modelGroup in objModel.Groups)          // one group - one mesh
            {
                vertexPositions.Clear();
                vertexNormals.Clear();
                vertexTextureCoordinates.Clear();
                vertexBoneIndices.Clear();
                vertexBoneWeights.Clear();
                indeces.Clear();

                foreach (var face in modelGroup.Value.Faces)          //  combine all meshes in group - to one mesh.
                    GetObjFace(face, objModel, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);

                var renderMesh = new DynamicRenderMesh(meshType);
                renderMesh.groupName = modelGroup.Value.Name;

                if (!objModel.modelByHeadShop)
                {
                    for (var i = 0; i < vertexPositions.Count / 3; i++)
                        vertexPositions[i * 3 + 1] = vertexPositions[i * 3 + 1] - 0.0060975609f;
                }

                if (renderMesh.Create(vertexPositions, vertexTextureCoordinates, vertexBoneIndices, vertexBoneWeights, indeces, string.Empty, string.Empty))
                {
                    if (objModelNull != null)
                    {
                        groupsNull.MoveNext();
                        groupNull = groupsNull.Current.Value;
                        vertexPositionsNull.Clear();
                        vertexNormalsNull.Clear();
                        vertexTextureCoordinatesNull.Clear();
                        vertexBoneWeightsNull.Clear();
                        vertexBoneIndicesNull.Clear();
                        indecesNull.Clear();
                        foreach (var face in groupNull.Faces)
                        {
                            GetObjFace(face, objModelNull, ref vertexPositionsNull, ref vertexNormalsNull, ref vertexTextureCoordinatesNull, ref vertexBoneWeightsNull, ref vertexBoneIndicesNull, ref indecesNull);
                        }
                        renderMesh.SetNullPoints(vertexPositionsNull, vertexNormalsNull, vertexTextureCoordinatesNull);
                    }

                    renderMesh.Title = modelGroup.Key.Name == "default" ? string.Empty : modelGroup.Key.Name;
                    renderMesh.Material = modelGroup.Key;
                    renderMesh.Material.DiffuseColor = new Vector4(modelGroup.Key.DiffuseColor.X, modelGroup.Key.DiffuseColor.Y, modelGroup.Key.DiffuseColor.Z, modelGroup.Key.Transparency);

                    if (!string.IsNullOrEmpty(modelGroup.Key.DiffuseTextureMap))
                        renderMesh.Material.DiffuseTextureMap = modelGroup.Key.DiffuseTextureMap;
                    if (!string.IsNullOrEmpty(modelGroup.Key.TransparentTextureMap))
                        renderMesh.Material.TransparentTextureMap = modelGroup.Key.TransparentTextureMap;

                    var scale = meshType == MeshType.Head ? GetHeadScale(manType) : GetHairScale(manType);                       // перегруз сделан потому, что на этапе загрузки проекта самого проекта еще может не быть. поэтому лучше передавать
                    renderMesh.Transform = Matrix4.CreateScale(scale);

                    var center = Vector3.Zero;
                    var count = vertexPositions.Count / 3;
                    for (var i = 0; i < count; i++)
                    {
                        center.X += vertexPositions[i * 3] * scale;
                        center.Y += vertexPositions[i * 3 + 1] * scale;
                        center.Z += vertexPositions[i * 3 + 2] * scale;
                    }

                    if (fromDragAndDrop)
                    {
                        center /= count;
                        renderMesh.Transform = Matrix4.CreateScale(scale);
                        renderMesh.Transform[3, 0] = -center.X;
                        renderMesh.Transform[3, 1] = -center.Y;
                        renderMesh.Transform[3, 2] = -center.Z;
                        renderMesh.Position = center;
                    }

                    if (vertexTextureCoordinates.Count > 0 && vertexTextureCoordinates.All(x => x == 0))
                        renderMesh.UpdateTextureCoordinates(0, 1);

                    if (renderMesh.vertexArray.Length > 0)
                        result.Add(renderMesh);
                }
            }
            return result;
        }

        private void GetAABB(ref Vector3 a, ref Vector3 b, IEnumerable<Vector3> points)
        {
            foreach (var p in points)
            {
                a.X = Math.Min(a.X, p.X);
                a.Y = Math.Min(a.Y, p.Y);
                a.Z = Math.Min(a.Z, p.Z);
                b.X = Math.Max(b.X, p.X);
                b.Y = Math.Max(b.Y, p.Y);
                b.Z = Math.Max(b.Z, p.Z);
            }
        }

        private Vector3 MoveToPosition(ref List<Vector3> points, Vector3 a, Vector3 b, Vector3 position)
        {
            var dv = position - (a + b) * 0.5f;
            for (int i = 0; i < points.Count; i++)
                points[i] = points[i] + dv;
            return dv;
        }

        public Dictionary<Guid, PartMorphInfo> LoadPartsMorphInfo(string path, RenderMesh renderMesh)
        {
            var vertexPositions = new List<float>();
            List<float> tmp = null;
            List<uint> uitmp = null;
            var vertices = new List<Vector3>();

            var objModel = ObjLoader.LoadObjFile(path);
            if (objModel == null)
            {
                ProgramCore.EchoToLog(string.Format("Can't load obj model '{0}'", path), EchoMessageType.Error);
                return null;
            }

            var a = new Vector3(99999.0f, 99999.0f, 99999.0f);
            var b = new Vector3(-99999.0f, -99999.0f, -99999.0f);
            var result = new Dictionary<Guid, PartMorphInfo>();
            foreach (var modelGroup in objModel.Groups) // one group - one mesh
            {
                vertexPositions.Clear();

                foreach (var face in modelGroup.Value.Faces) //  combine all meshes in group - to one mesh.
                    GetObjFace(face, objModel, ref vertexPositions, ref tmp, ref tmp,
                        ref tmp, ref tmp, ref uitmp);
                vertices.Clear();
                for (int i = 0; i < vertexPositions.Count; i += 3)
                    vertices.Add(new Vector3(vertexPositions[i], vertexPositions[i + 1], vertexPositions[i + 2]));


                PartMorphInfo morphInfo = null;
                float scale = ProgramCore.PluginMode &&
                              ProgramCore.MainForm.ctrlRenderControl.pickingController.ObjExport != null
                    ? ProgramCore.MainForm.ctrlRenderControl.pickingController.ObjExport.Scale
                    : 1.0f;
                var part =
                    renderMesh.Parts.FirstOrDefault(
                        p =>
                            (p.Name.ToLower().Contains(modelGroup.Value.Name.ToLower()) || modelGroup.Value.Name.ToLower().Contains(p.Name.ToLower())) &&
                            PartMorphInfo.CreatePartMorphInfo(vertices, p, scale, out morphInfo));
                if (part != null)
                {
                    result.Add(part.Guid, morphInfo);
                    GetAABB(ref a, ref b, vertices);
                }
            }
            foreach (var r in result)
                MoveToPosition(ref r.Value.PointsMorph, a, b, Vector3.Zero);
            return result;
        }

        /// <summary> Accessory file by default. Usually here is one accessory devided for differen groups. But it's not correct, so we combine it to one mesh</summary>
        private DynamicRenderMesh LoadAccessoryMesh(ObjItem objModel)
        {
            var vertexPositions = new List<float>();
            var vertexNormals = new List<float>();
            var vertexTextureCoordinates = new List<float>();
            var vertexBoneIndices = new List<float>();
            var vertexBoneWeights = new List<float>();
            var indeces = new List<uint>();
            var defaultMaterial = default(ObjMaterial);
            foreach (var modelGroup in objModel.Groups)
            {
                defaultMaterial = modelGroup.Key;
                foreach (var face in modelGroup.Value.Faces)
                {
                    if (face.Count == 3)
                    {
                        for (var i = 0; i < face.Count; i++)
                        {
                            var faceVertex = face[i];
                            ObjLoader.AppendObjTriangle(objModel, faceVertex, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        }
                    }
                    else if (face.Count == 4)
                    {
                        var faceVertex0 = face[0];
                        var faceVertex1 = face[1];
                        var faceVertex2 = face[2];
                        var faceVertex3 = face[3];

                        ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex1, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);

                        ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex3, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                    }
                }
            }

            var renderMesh = new DynamicRenderMesh(MeshType.Accessory);
            if (renderMesh.Create(vertexPositions, vertexTextureCoordinates, vertexBoneIndices, vertexBoneWeights, indeces, string.Empty, string.Empty))
            {
                renderMesh.Material.DiffuseColor = new Vector4(defaultMaterial.DiffuseColor.X, defaultMaterial.DiffuseColor.Y, defaultMaterial.DiffuseColor.Z, defaultMaterial.Transparency);

                if (!string.IsNullOrEmpty(defaultMaterial.DiffuseTextureMap))
                    renderMesh.Material.DiffuseTextureMap = defaultMaterial.DiffuseTextureMap;
                if (!string.IsNullOrEmpty(defaultMaterial.TransparentTextureMap))
                    renderMesh.Material.TransparentTextureMap = defaultMaterial.TransparentTextureMap;

                var center = Vector3.Zero;
                var count = vertexPositions.Count / 3;
                const float scale = 246f;
                for (var i = 0; i < count; i++)
                {
                    center.X += vertexPositions[i * 3] * scale;
                    center.Y += vertexPositions[i * 3 + 1] * scale;
                    center.Z += vertexPositions[i * 3 + 2] * scale;
                }
                center /= count;
                renderMesh.Transform = Matrix4.CreateScale(scale);
                renderMesh.Transform[3, 0] = -center.X;                
                renderMesh.Transform[3, 1] = -center.Y;
                renderMesh.Transform[3, 2] = -center.Z;
                renderMesh.Position = center;
                AccesoryMeshes.Add(renderMesh);
            }
            return renderMesh;
        }
        /// <summary> Accessory file saved in program. Here is a few accessories devided by groups </summary>
        private List<DynamicRenderMesh> LoadSpecialAccessoryMesh(ObjItem objModel)
        {
            var result = new List<DynamicRenderMesh>();
            foreach (var modelGroup in objModel.Groups)
            {
                var vertexPositions = new List<float>();
                var vertexNormals = new List<float>();
                var vertexTextureCoordinates = new List<float>();
                var vertexBoneIndices = new List<float>();
                var vertexBoneWeights = new List<float>();
                var indeces = new List<uint>();
                foreach (var face in modelGroup.Value.Faces)
                {
                    if (face.Count == 3)
                    {
                        for (var i = 0; i < face.Count; i++)
                        {
                            var faceVertex = face[i];
                            ObjLoader.AppendObjTriangle(objModel, faceVertex, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        }
                    }
                    else if (face.Count == 4)
                    {
                        var faceVertex0 = face[0];
                        var faceVertex1 = face[1];
                        var faceVertex2 = face[2];
                        var faceVertex3 = face[3];

                        ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex1, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);

                        ObjLoader.AppendObjTriangle(objModel, faceVertex2, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex3, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                        ObjLoader.AppendObjTriangle(objModel, faceVertex0, ref vertexPositions, ref vertexNormals, ref vertexTextureCoordinates, ref vertexBoneWeights, ref vertexBoneIndices, ref indeces);
                    }
                }

                if (vertexPositions.Count == 0)
                    continue;

                var renderMesh = new DynamicRenderMesh(MeshType.Accessory);
                if (renderMesh.Create(vertexPositions, vertexTextureCoordinates, vertexBoneIndices, vertexBoneWeights, indeces, string.Empty, string.Empty))
                {
                    renderMesh.Title = modelGroup.Key.Name == "default" ? string.Empty : modelGroup.Key.Name;
                    renderMesh.Material.DiffuseColor = new Vector4(modelGroup.Key.DiffuseColor.X, modelGroup.Key.DiffuseColor.Y, modelGroup.Key.DiffuseColor.Z, modelGroup.Key.Transparency);

                    if (!string.IsNullOrEmpty(modelGroup.Key.DiffuseTextureMap))
                        renderMesh.Material.DiffuseTextureMap = modelGroup.Key.DiffuseTextureMap;
                    if (!string.IsNullOrEmpty(modelGroup.Key.TransparentTextureMap))
                        renderMesh.Material.TransparentTextureMap = modelGroup.Key.TransparentTextureMap;

                    var center = Vector3.Zero;
                    var count = vertexPositions.Count / 3;
                    const float scale = 246f;
                    for (var i = 0; i < count; i++)
                    {
                        center.X += vertexPositions[i * 3] * scale;
                        center.Y += vertexPositions[i * 3 + 1] * scale;
                        center.Z += vertexPositions[i * 3 + 2] * scale;
                    }
                    center /= count;
                    renderMesh.Transform = Matrix4.CreateScale(scale);
                    /*   renderMesh.Transform[3, 0] = -center.X;
                       renderMesh.Transform[3, 1] = -center.Y;
                       renderMesh.Transform[3, 2] = -center.Z;*/
                    renderMesh.Position = center;
                    AccesoryMeshes.Add(renderMesh);


                    result.Add(renderMesh);
                }
            }

            return result;
        }

        #endregion

    }
}
