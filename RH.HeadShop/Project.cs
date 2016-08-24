using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using RH.HeadEditor.Data;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;

namespace RH.HeadShop
{
    /// <summary> HeadShop project </summary>
    public class Project
    {
        /// <summary> Название проекта </summary>
        public string ProjectName;
        /// <summary> Путь к проекта </summary>
        public string ProjectPath;

        public string HeadModelPath;

        /// <summary> Относительный путь до левой картинке (шаблона)</summary>
        private string frontImagePath = string.Empty;
        public string FrontImagePath
        {
            get
            {
                return frontImagePath;
            }
            set
            {
                frontImagePath = value;
                if (!string.IsNullOrEmpty(frontImagePath))
                {
                    var fileName = Path.Combine(ProjectPath, frontImagePath);
                    var fi = new FileInfo(fileName);
                    if (fi.Exists)
                    {
                        FrontImage = new Bitmap(fi.FullName);
                    }
                    else
                        frontImagePath = string.Empty;
                }

            }
        }
        public Bitmap FrontImage;

        public Bitmap ProfileImage;

        /// <summary> Было ли назначение автоточек </summary>
        public bool AutodotsUsed;

        public FlipType TextureFlip = FlipType.None;        // отражено ли или нет
        public FlipType ShapeFlip = FlipType.None;

        public float AgeCoefficient = 0;            // коэффициенты features для сохранения возраста и толщины.
        public float FatCoefficient = 0;

        #region Все нужное, для работы с моделью головы

        public HeadPoints<HeadPoint> BaseDots; // опорные точки для произвольной башки. по дефолту - женские точки 
        public HeadPoints<HeadPoint> ProfileBaseDots;

        public bool CustomHeadNeedProfileSetup; // в случае произвольной башки, нужно ли вызывать настройку точек или нет

        #endregion

        #region Face recognized position

        /// <summary> Прямоугольник лица, рассчитанный по точкам глаз и рта, определенных распознаванием. Относительные координаты.</summary>
        public RectangleF FaceRectRelative;

        /// <summary> Центр рта, опредееленный распознаванием лица. Относительные координаты </summary>
        private Vector2 mouthCenter;
        public Vector2 MouthCenter
        {
            get
            {
                return mouthCenter;
            }
            set
            {
                mouthCenter = value;
                if (MouthUserCenter == Vector2.Zero)
                    MouthUserCenter = mouthCenter;

                RecalcRect();
            }
        }

        /// <summary> Центр левого глаза, опредееленный распознаванием лица. Относительные координаты </summary>
        private Vector2 leftEyeCenter;
        public Vector2 LeftEyeCenter
        {
            get
            {
                return leftEyeCenter;
            }
            set
            {
                leftEyeCenter = value;
                if (LeftEyeUserCenter == Vector2.Zero)
                    LeftEyeUserCenter = leftEyeCenter;

                RecalcRect();
            }
        }

        /// <summary> Центр правого глаза, опредееленный распознаванием лица. Относительные координаты </summary>
        private Vector2 rightEyeCenter;
        public Vector2 RightEyeCenter
        {
            get
            {
                return rightEyeCenter;
            }
            set
            {
                rightEyeCenter = value;
                if (RightEyeUserCenter == Vector2.Zero)
                    RightEyeUserCenter = rightEyeCenter;

                RecalcRect();
            }
        }

        #endregion

        #region User face position

        /// <summary> Поцизии рта и глаз, определенные пользователем в автоточках (учитывая изменения положения точек). Относительные координаты </summary>
        public Vector2 MouthUserCenter;
        public Vector2 LeftEyeUserCenter;
        public Vector2 RightEyeUserCenter;
        public Vector2 NoseUserCenter = new Vector2(0, 0);       // так мы его никак не определим. ждем первых автоточек, Тогда будет пересчет

        #endregion

        /// <summary> Тип лица (мужское, женское, ребенок)</summary>
        public ManType ManType;

        /// <summary> используются сглаженные текстуры </summary>
        public bool SmoothedTextures;

        /// <summary> Создание нового проекта </summary>
        /// <param name="projectName"></param>
        /// <param name="projectPath"></param>
        /// <param name="templateImageName"></param>
        /// <param name="manType"></param>
        /// <param name="headModelPath">Указываем путь до модели головы (в случае если выбрали import OBJ). Иначе - пустая строка</param>
        /// <param name="needCopy"></param>
        public Project(string projectName, string projectPath, string templateImageName, ManType manType, string headModelPath, bool needCopy)
        {
            ProjectName = projectName;
            ProjectPath = projectPath;

            ManType = manType;
            switch (manType)
            {
                case ManType.Male:
                case ManType.Female:        // если это обычные модели - копируем их из папки с прогой - в папку с проектом
                case ManType.Child:
                    if (!ProgramCore.PluginMode)        //тогда хед модел пас оставляем какой был! пиздец важно!
                        headModelPath = Path.Combine(Application.StartupPath, "Models\\Model", manType.GetObjPath());
                    break;
                case ManType.Custom:
                    {
                        BaseDots = new HeadPoints<HeadPoint>();
                        foreach (var vector in HeadController.GetBaseDots(ManType.Female))
                            BaseDots.Add(new HeadPoint(vector));

                        ProfileBaseDots = new HeadPoints<HeadPoint>();
                        foreach (var vector in HeadController.GetProfileBaseDots(ManType.Female))
                            ProfileBaseDots.Add(new HeadPoint(vector));
                    }
                    break;
            }

            if (needCopy)
            {
                var di = new DirectoryInfo(projectPath);
                if (!di.Exists)
                    di.Create();

                if (!string.IsNullOrEmpty(templateImageName))
                {
                    var fi = new FileInfo(templateImageName);
                    if (fi.Exists)
                    {
                        var newImagePath = Path.Combine(projectPath, fi.Name);
                        File.Copy(templateImageName, newImagePath, true);
                        FrontImagePath = fi.Name;
                    }
                }

                #region Копируем модель

                var directoryPath = Path.Combine(ProjectPath, "Model");
                FolderEx.CreateDirectory(directoryPath);
                var oldFileName = Path.GetFileNameWithoutExtension(headModelPath);
                var newFileName = oldFileName;
                var filePath = Path.Combine(directoryPath, newFileName + ".obj");

                File.Copy(headModelPath, filePath, true);           // сама модель
                HeadModelPath = filePath;

                #region Обрабатываем mtl файл и папку с текстурами

                var mtl = oldFileName + ".mtl";
                using (var ms = new StreamReader(headModelPath))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        if (ms.EndOfStream)
                            break;
                        var line = ms.ReadLine();
                        if (line.ToLower().Contains("mtllib"))          // ищем ссылку в obj файле на mtl файл (у них могут быть разные названия, но всегда в одной папке
                        {
                            var lines = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Length > 1)
                            {
                                mtl = lines[1];
                                break;
                            }
                        }
                    }
                }

                ObjLoader.CopyMtl(mtl, mtl, Path.GetDirectoryName(headModelPath), "", directoryPath);

                #endregion

                #endregion
            }
            else
            {
                FrontImagePath = templateImageName;
                HeadModelPath = headModelPath;
            }

            var modelPath = Path.Combine(projectPath, "OBJ", "hair.obj");
            ProgramCore.MainForm.ctrlRenderControl.LoadModel(modelPath, true, ManType, MeshType.Hair);

            var acDirPath = Path.GetDirectoryName(modelPath);
            var acName = Path.GetFileNameWithoutExtension(modelPath) + "_accessory.obj";
            var accessoryPath = Path.Combine(acDirPath, acName);
            if (File.Exists(accessoryPath))
                ProgramCore.MainForm.ctrlRenderControl.LoadModel(accessoryPath, false, ManType, MeshType.Accessory);
        }

        /// <summary> Save current project to file </summary>
        public void ToStream()
        {
            try
            {
                if (ProgramCore.Project.AutodotsUsed)
                    ProgramCore.MainForm.ctrlRenderControl.SaveSmoothedTextures();

                var path = Path.Combine(ProjectPath, string.Format("{0}.hds", ProjectName));
                using (var bw = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
                {
                    bw.Write(ProjectName);
                    bw.Write(FrontImagePath);

                    bw.Write(HeadModelPath);

                    bw.Write((int)ManType);
                    bw.Write((int)TextureFlip);
                    bw.Write((int)ShapeFlip);

                    var fiName = Path.Combine(ProjectPath, "OBJ", "hair.obj");
                    FolderEx.CreateDirectory(Path.GetDirectoryName(fiName));
                    if (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Count > 0) // save hair file
                        ObjSaver.SaveObjFile(fiName, ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes, MeshType.Hair);
                    else
                        FolderEx.DeleteSafety(fiName);      // если раньше были волосы , а сейчас удалили - надо их грохнуть из папки тоже

                    var acDirPath = Path.GetDirectoryName(fiName);
                    var acName = Path.GetFileNameWithoutExtension(fiName) + "_accessory.obj";
                    var accessoryPath = Path.Combine(acDirPath, acName);
                    if (ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes.Count > 0) // save accessory file
                        ObjSaver.SaveObjFile(accessoryPath, ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes, MeshType.Accessory);
                    else
                        FolderEx.DeleteSafety(accessoryPath);

                    var partsLibraryPath = Path.Combine(ProjectPath, "Parts Library");
                    FolderEx.CreateDirectory(partsLibraryPath);

                    bw.Write(ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.Count); // save list of meshes to part's library
                    foreach (var part in ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes)
                    {
                        bw.Write(part.Key);
                        bw.Write(part.Value[0].meshType == MeshType.Accessory);
                        bw.Write(part.Value[0].IsVisible);

                        bw.Write(part.Value.Count);
                        foreach (var selMesh in part.Value)
                            bw.Write(selMesh.Title);

                        var fileName = part.Key + ".obj";
                        ObjSaver.SaveObjFile(Path.Combine(partsLibraryPath, fileName), part.Value, part.Value[0].meshType);
                    }

                    bw.Write(AutodotsUsed);

                    // сохраняем прямоугольник лица
                    bw.Write(FaceRectRelative.X);
                    bw.Write(FaceRectRelative.Y);
                    bw.Write(FaceRectRelative.Width);
                    bw.Write(FaceRectRelative.Height);

                    // сохраняем центры рта и глаз распознаные
                    bw.Write(MouthCenter.X);
                    bw.Write(MouthCenter.Y);

                    bw.Write(LeftEyeCenter.X);
                    bw.Write(LeftEyeCenter.Y);
                    bw.Write(RightEyeCenter.X);
                    bw.Write(RightEyeCenter.Y);

                    // сохраняем центры рта и глаз пользовательские (после таскания и прочего)
                    bw.Write(NoseUserCenter.X);
                    bw.Write(NoseUserCenter.Y);
                    bw.Write(MouthUserCenter.X);
                    bw.Write(MouthUserCenter.Y);

                    bw.Write(LeftEyeUserCenter.X);
                    bw.Write(LeftEyeUserCenter.Y);
                    bw.Write(RightEyeUserCenter.X);
                    bw.Write(RightEyeUserCenter.Y);

                    bw.Write(AgeCoefficient);
                    bw.Write(FatCoefficient);

                    #region Информация о модели головы

                    var rmPath = Path.Combine(ProjectPath, "Model", "MeshParts.rm");

                    #region Сохранение RenderMesh

                    if (ManType != ManType.Custom)
                    {
                        foreach (var m in ProgramCore.MainForm.ctrlRenderControl.OldMorphing) // перед сохранением сбрасываем морфинги на 0. 
                            m.Value.Delta = 0;
                        foreach (var m in ProgramCore.MainForm.ctrlRenderControl.FatMorphing)
                            m.Value.Delta = 0;
                        ProgramCore.MainForm.ctrlRenderControl.DoMorth();
                    }

                    ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Save(rmPath);

                    if (ManType != ManType.Custom)
                    {
                        foreach (var m in ProgramCore.MainForm.ctrlRenderControl.OldMorphing) // перед сохранением сбрасываем морфинги на 0. 
                            m.Value.Delta = AgeCoefficient;
                        foreach (var m in ProgramCore.MainForm.ctrlRenderControl.FatMorphing)
                            m.Value.Delta = FatCoefficient;
                        ProgramCore.MainForm.ctrlRenderControl.DoMorth();
                    }

                    #endregion

                    if (BaseDots != null)
                    {
                        bw.Write(BaseDots.Count);
                        foreach (var point in BaseDots)
                            point.ToStream(bw);
                    }
                    else
                        bw.Write(0);

                    if (ProfileBaseDots != null)
                    {
                        bw.Write(ProfileBaseDots.Count);
                        foreach (var point in ProfileBaseDots)
                            point.ToStream(bw);
                    }
                    else
                        bw.Write(0);

                    ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeInfo.ToStream(bw);
                    ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeProfileInfo.ToStream(bw);

                    ProgramCore.MainForm.ctrlRenderControl.headMeshesController.TexturingInfo.ToStream(bw);


                    bw.Write(ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Count);
                    foreach (var dot in ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots)
                        dot.ToStreamM(bw);
                    bw.Write(ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.Count);
                    foreach (var dot in ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots)
                        dot.ToStreamM(bw);

                    bw.Write(CustomHeadNeedProfileSetup);

                    #endregion
                }
            }
            catch (Exception e)
            {
                ProgramCore.EchoToLog(e, true);
            }
        }
        /// <summary> Load project from path </summary>
        public static Project FromStream(string path)
        {
            Project result;
            using (var br = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                var projectFi = new FileInfo(path);

                var projectName = br.ReadString();

                #region template image

                var templateImagePath = br.ReadString();
                if (!string.IsNullOrEmpty(templateImagePath))
                {
                    var fiName = Path.Combine(projectFi.DirectoryName, templateImagePath);
                    var fi = new FileInfo(fiName);
                    if (!fi.Exists)
                    {
                        ProgramCore.EchoToLog("Can't find template image in project.", EchoMessageType.Warning);
                        templateImagePath = string.Empty;
                    }
                }

                #endregion

                var headModelPath = br.ReadString();

                var manType = (ManType)br.ReadInt32();
                var textureFlip = (FlipType)br.ReadInt32();
                var shapeFlip = (FlipType)br.ReadInt32();

                result = new Project(projectName, projectFi.DirectoryName, templateImagePath, manType, headModelPath, false);
                result.TextureFlip = textureFlip;
                result.ShapeFlip = shapeFlip;

                var partsCount = br.ReadInt32(); //part's library
                for (var i = 0; i < partsCount; i++)
                {
                    var title = br.ReadString();
                    var meshType = br.ReadBoolean() ? MeshType.Accessory : MeshType.Hair;
                    var meshVisible = br.ReadBoolean();

                    var meshCounts = br.ReadInt32();
                    for (var j = 0; j < meshCounts; j++)
                    {
                        var meshTitle = br.ReadString();
                        DynamicRenderMesh mesh;
                        if (meshType == MeshType.Accessory)
                            mesh = ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes[meshTitle];
                        else
                            mesh = ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes[meshTitle];
                        if (mesh == null)
                            continue;

                        mesh.Title = title + "_" + j;
                        mesh.IsVisible = meshVisible;

                        if (!ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(title))
                            ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.Add(title, new DynamicRenderMeshes());
                        ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes[title].Add(mesh);
                    }
                }

                result.AutodotsUsed = br.ReadBoolean();

                // загружаем прямоугольник лица (фронт)
                result.FaceRectRelative = new RectangleF(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                result.MouthCenter = new Vector2(br.ReadSingle(), br.ReadSingle());
                result.LeftEyeCenter = new Vector2(br.ReadSingle(), br.ReadSingle());
                result.RightEyeCenter = new Vector2(br.ReadSingle(), br.ReadSingle());

                result.NoseUserCenter = new Vector2(br.ReadSingle(), br.ReadSingle());
                result.MouthUserCenter = new Vector2(br.ReadSingle(), br.ReadSingle());
                result.LeftEyeUserCenter = new Vector2(br.ReadSingle(), br.ReadSingle());
                result.RightEyeUserCenter = new Vector2(br.ReadSingle(), br.ReadSingle());

                result.AgeCoefficient = br.ReadSingle();
                result.FatCoefficient = br.ReadSingle();

                #region Информация о модели головы


                var rmPath = Path.Combine(projectFi.DirectoryName, "Model", "MeshParts.rm");
                ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Load(rmPath);
                foreach (var part in ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.Parts)
                {
                    if (!string.IsNullOrEmpty(part.TextureName))
                        part.Texture = ProgramCore.MainForm.ctrlRenderControl.GetTexture(part.TextureName);
                    if (!string.IsNullOrEmpty(part.TransparentTextureName))
                        part.TransparentTexture = ProgramCore.MainForm.ctrlRenderControl.GetTexture(part.TransparentTextureName);
                }

                var baseDotsCount = br.ReadInt32();
                for (var i = 0; i < baseDotsCount; i++)
                    result.BaseDots.Add(HeadPoint.FromStream(br));

                var profileBaseDotsCount = br.ReadInt32();
                for (var i = 0; i < profileBaseDotsCount; i++)
                    result.ProfileBaseDots.Add(HeadPoint.FromStream(br));

                ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeInfo = TexturingInfo.FromStream(br);
                ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeProfileInfo = TexturingInfo.FromStream(br);

                ProgramCore.MainForm.ctrlRenderControl.headMeshesController.TexturingInfo = TexturingInfo.FromStream(br);

                ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.Clear();
                ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Clear();
                var cnt = br.ReadInt32();
                for (var i = 0; i < cnt; i++)
                    ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Add(MirroredHeadPoint.FromStreamW(br));
                cnt = br.ReadInt32();
                for (var i = 0; i < cnt; i++)
                    ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.Add(MirroredHeadPoint.FromStreamW(br));

                result.CustomHeadNeedProfileSetup = br.ReadBoolean();

                #endregion

                var fi1 = new FileInfo(Path.Combine(projectFi.DirectoryName, "ProfileImage.jpg"));
                if (fi1.Exists)
                {
                    using (var fs = new FileStream(fi1.FullName, FileMode.Open))
                    {
                        var bmp = new Bitmap(fs);
                        result.ProfileImage = (Bitmap)bmp.Clone();
                    }
                }
            }

            return result;
        }

        /// <summary> Simple function, that parce project file and return path to template image (require for preview when loading project) </summary>
        public static string LoadTempaltePath(string projectPath)
        {
            using (var br = new BinaryReader(File.Open(projectPath, FileMode.Open)))
            {
                var projectFi = new FileInfo(projectPath);

                var projectName = br.ReadString();
                var templateImagePath = br.ReadString();
                return Path.Combine(projectFi.DirectoryName, templateImagePath);
            }
        }

        private void RecalcRect()
        {
            var leftTop = new Vector2(LeftEyeCenter.X, Math.Max(LeftEyeCenter.Y, RightEyeCenter.Y));
            var rightBottom = new Vector2(RightEyeCenter.X, MouthCenter.Y);

            FaceRectRelative = new RectangleF(leftTop.X, leftTop.Y, rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y);
        }

        public static string CopyImgToProject(string imagePath, string imageType)
        {
            var fi = new FileInfo(imagePath);
            if (fi.Exists)
            {
                var newImagePath = Path.Combine(ProgramCore.Project.ProjectPath, imageType + Path.GetExtension(fi.Name));
                if (newImagePath != imagePath)
                    File.Copy(imagePath, newImagePath, true);
                return newImagePath;
            }
            return string.Empty;
        }
    }

    public enum ManType
    {
        Male = 1,
        Female = 0,
        Child = 2,
        Custom = 3         // произвольный тип ебала
    }
    public static class ManTypeEx
    {
        public static string GetCaption(this ManType manType)
        {
            switch (manType)
            {
                case ManType.Male:
                    return "Man";
                case ManType.Female:
                    return "Woman";
                case ManType.Child:
                    return "Child";
                default:
                    return "Custom";
            }
        }

        public static string GetObjDirPath(this ManType manType)
        {
            switch (manType)
            {
                case ManType.Male:
                    return "Male";
                case ManType.Female:
                    return "Fem";
                case ManType.Child:
                    return "Child";
                default:
                    return string.Empty;
            }
        }
        public static string GetObjPath(this ManType manType)
        {
            switch (manType)
            {
                case ManType.Male:
                    return "Male\\Male.obj";
                case ManType.Female:
                    return "Fem\\Fem.obj";
                case ManType.Child:
                    return "Child\\Child.obj";
                default:
                    return string.Empty;
            }
        }
    }

    public enum FlipType
    {
        LeftToRight,
        RightToLeft,
        None
    }
}
