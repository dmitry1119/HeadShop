using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RH.HeadEditor.Data;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Helpers;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace RH.HeadShop.Render.Controllers
{
    public class HeadController
    {
        #region Var

        /// <summary> Флаг, означающий что коллекция линий/точек изменена и нужно будет вызвать инициализацию шейпа. </summary>
        public bool LinesChanged
        {
            get;
            private set;
        }

        public Collection<HeadLine> Lines = new Collection<HeadLine>();
        public List<MirroredHeadPoint> SelectedPoints
        {
            get
            {
                var points = new List<MirroredHeadPoint>();

                foreach (var line in Lines)
                    foreach (var point in line)
                    {
                        if (point.Selected)
                            points.Add(point);
                    }

                return points;
            }
        }
        public List<MirroredHeadPoint> AllPoints
        {
            get
            {
                var result = new List<MirroredHeadPoint>();
                foreach (var polyline in Lines)
                {
                    foreach (var point in polyline)
                    {
                        if (!point.Visible)
                            continue;
                        result.Add(point);
                    }
                }
                return result;
            }
        }

        public HeadPoints<MirroredHeadPoint> AutoDots = new HeadPoints<MirroredHeadPoint>();
        public HeadPoints<MirroredHeadPoint> ShapeDots = new HeadPoints<MirroredHeadPoint>();


        #endregion

        public HeadController()
        {
            Lines.CollectionChanged += Lines_CollectionChanged;

            Lines.ItemsAdded += Lines_ItemsAdded;
            Lines.ItemsRemoved += Lines_ItemsRemoved;
        }

        /// <summary> Уточнить положение глаз и рта справа, относительно центра, указанного пользователем </summary>
        private void ClarifyEyeAndMouth(IEnumerable<int> indexes, Vector2 originalCenter)
        {
            var dots = new List<MirroredHeadPoint>();
            foreach (var index in indexes)
                dots.Add(AutoDots[index]);

            var minX = dots.Min(point => point.ValueMirrored.X);
            var maxX = dots.Max(point => point.ValueMirrored.X);
            var minY = dots.Min(point => point.ValueMirrored.Y);
            var maxY = dots.Max(point => point.ValueMirrored.Y);

            var center = new Vector2((maxX + minX) * 0.5f, (maxY + minY) * 0.5f);
            var delta = originalCenter - center;

            foreach (var dot in dots)
                dot.ValueMirrored += delta;
        }

        /// <summary> Загрузить новое изображение как шаблон для профиля </summary>
        public void LoadNewProfileImage()
        {
            using (var ofd = new OpenFileDialogEx("Select template file", "JPG Files|*.jpg"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var newPath = Project.CopyImgToProject(ofd.FileName, "ProfileImage");
                using (var bmp = new Bitmap(newPath))
                    ProgramCore.Project.ProfileImage = new Bitmap(bmp);

                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.ProfileImage, false);
                ProgramCore.MainForm.ctrlTemplateImage.ControlPointsMode = ProfileControlPointsMode.SetControlPoints;
                foreach (var point in ProgramCore.MainForm.ctrlTemplateImage.profileControlPoints)     // подчищаем за собой каки
                    point.ValueMirrored = Vector2.Zero;
            }
        }

        #region Event's

        private void Lines_ItemsAdded(HeadLine[] items)
        {
            foreach (var point in items)
                point.CollectionChanged += points_CollectionChanged;
        }
        private void Lines_ItemsRemoved(HeadLine[] items)
        {
            foreach (var point in items)
                point.CollectionChanged -= points_CollectionChanged;
        }

        private void points_CollectionChanged(object sender, EventArgs e)
        {
            LinesChanged = true;
        }
        private void Lines_CollectionChanged(object sender, EventArgs e)
        {
            LinesChanged = true;
        }

        #endregion

        #region Drawing

        public void Draw()
        {
            switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
            {
                case Mode.HeadAutodots:
                case Mode.HeadAutodotsFirstTime:
                case Mode.HeadAutodotsLassoStart:
                case Mode.HeadAutodotsLassoActive:
                    if (ProgramCore.Debug)
                        DrawAutodots();
                    break;
                case Mode.HeadShapedots:
                case Mode.HeadShapedotsLassoStart:
                case Mode.HeadShapedotsLassoActive:
                    DrawShapedots();
                    break;
                case Mode.HeadLine:
                    if (ProgramCore.Debug && ProgramCore.MainForm.HeadFront)
                        DrawTmp();
                    break;
            }
        }

        private void DrawTmp()
        {
            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Points);
            foreach (var line in Lines)
                foreach (MirroredHeadPoint point in line)
                {
                    GL.Color3(0.0f, 1.0f, 0.0f);

                    GL.Vertex2(point.Value);
                }

            var center = ProgramCore.Project.LeftEyeUserCenter;
            center = MirroredHeadPoint.UpdateWorldPoint(center);

            GL.Color3(1.0f, 1.0f, 0.0f);

            GL.Vertex2(center);

            center = ProgramCore.Project.RightEyeUserCenter;
            center = MirroredHeadPoint.UpdateWorldPoint(center);
            GL.Vertex2(center);

            center = ProgramCore.Project.MouthUserCenter;
            center = MirroredHeadPoint.UpdateWorldPoint(center);
            GL.Vertex2(center);

            center = ProgramCore.Project.NoseUserCenter;
            center = MirroredHeadPoint.UpdateWorldPoint(center);
            GL.Vertex2(center);


            GL.End();
            GL.PointSize(1.0f);
        }
        private void DrawAutodots()
        {
            GL.Color3(0.0f, 1.0f, 0.0f);

            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Points);

            foreach (var point in AutoDots)
            {
                if (!point.Visible)
                    continue;

                if (point.Selected)
                    GL.Color3(1.0f, 0.0f, 0.0f);
                else
                    GL.Color3(0.0f, 1.0f, 0.0f);

                GL.Vertex2(point.Value);
            }

            GL.End();
            GL.PointSize(1.0f);
        }
        private void DrawShapedots()
        {
            GL.Color3(0.0f, 1.0f, 0.0f);

            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Points);

            foreach (var point in ShapeDots)
            {
                if (!point.Visible)
                    continue;

                if (point.Selected)
                    GL.Color3(1.0f, 0.0f, 0.0f);
                else
                    GL.Color3(0.0f, 1.0f, 0.0f);

                GL.Vertex2(point.Value);
            }

            GL.End();
            GL.PointSize(1.0f);
        }

        public void DrawOnPictureBox(Graphics g)
        {
            switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
            {
                case Mode.HeadAutodots:
                case Mode.HeadAutodotsFirstTime:
                case Mode.HeadAutodotsLassoStart:
                case Mode.HeadAutodotsLassoActive:
                    DrawAutodots(g);
                    break;
                case Mode.HeadShapedots:
                case Mode.HeadShapedotsLassoStart:
                case Mode.HeadShapedotsLassoActive:
                    DrawShapedots(g);
                    break;
                default:
                    DrawLines(g);
                    break;
            }
        }
        private void DrawLines(Graphics g)
        {
            foreach (var line in Lines)
            {
                for (var i = line.Count - 2; i >= 0; i--)
                {
                    var pointA = line[i];
                    var pointB = line[i + 1];

                    g.DrawLine(DrawingTools.GreenPen, pointA.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                      pointA.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY,
                                                      pointB.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                      pointB.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                }
                foreach (MirroredHeadPoint point in line)
                {
                    var v = point.ValueMirrored;
                    var pointRect = new RectangleF((v.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX) - 2.5f,
                        (v.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY) - 2.5f,
                        5f, 5f);
                    g.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.GreenSolidBrush, pointRect);
                }
            }
        }
        private void DrawAutodots(Graphics g)
        {
            foreach (var point in AutoDots)
            {
                if (!point.Visible)
                    continue;

                var v = point.ValueMirrored;
                var pointRect = new RectangleF((v.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX) - 2.5f,
                    (v.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY) - 2.5f,
                    5f, 5f);
                g.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.GreenSolidBrush, pointRect);
            }
        }
        private void DrawShapedots(Graphics g)
        {
            foreach (var point in ShapeDots)
            {
                if (!point.Visible)
                    continue;

                var v = point.ValueMirrored;
                var pointRect = new RectangleF((v.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX) - 2.5f,
                    (v.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY) - 2.5f,
                    5f, 5f);
                g.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.GreenSolidBrush, pointRect);
            }
        }

        #endregion

        #region Lines

        public void SelectAll()
        {
            ClearPointsSelection();
            foreach (var line in Lines)
            {
                foreach (var point in line)
                    point.Selected = true;
            }
        }
        /// <summary> Снять выделение</summary>
        public void ClearPointsSelection()
        {
            foreach (var line in Lines)
                foreach (var point in line)
                    point.Selected = false;
        }
        /// <summary> Удалить выделенные точки </summary>
        public void RemoveSelectedPoints()
        {
            var history = new HistoryHeadAutoDots(ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots);
            ProgramCore.MainForm.ctrlRenderControl.historyController.Add(history);

            for (var i = Lines.Count - 1; i >= 0; i--)
            {
                var line = Lines[i];
                for (var j = line.Count - 1; j >= 0; j--)
                {
                    var point = line[j];
                    if (point.Selected)
                        line.Remove(point);
                }
                if (line.Count == 0)
                    Lines.RemoveAt(i);
            }
        }
        /// <summary> Проверка является ли точка выделенной. Если да - добавляем в список выделенных </summary>
        /// Возвращаем - попали ли в точку или нет
        public bool UpdatePointSelection(float x, float y)
        {
            foreach (var line in Lines)
                foreach (var point in line)
                {
                    if (point.Value.X >= x - 0.5 && point.Value.X <= x + 0.5 && point.Value.Y >= y - 0.5 && point.Value.Y <= y + 0.5)
                    {
                        point.Selected = !point.Selected;
                        return true;
                    }
                }
            return false;
        }

        #endregion

        #region Point indicies

        public static List<Vector2> GetBaseDots(ManType manType)
        {
            switch (manType)
            {
                case ManType.Female:
                    return GetFemBaseDots();
                case ManType.Male:
                    return GetMaleBaseDots();
                case ManType.Child:
                    return GetChildBaseDots();
                case ManType.Custom:
                    return ProgramCore.Project.BaseDots.Select(x => x.Value).ToList();
                default:
                    return null;
            }
        }

        public static List<Int32> GetIndices()
        {
            return new List<int>
            {
                0, 18, 40,
                0, 23, 18,
                0, 3, 23,
                0, 40, 45,
                3, 22, 23,
                3, 4, 22,
                4, 5, 22,
                5, 21, 22,
                5, 6, 21,
                6, 7, 21,
                7, 8, 21,
                8, 24, 21,
                8, 19, 24,
                8, 15, 19,
                8, 9, 15,
                9, 17, 15,
                9, 12, 10,
                9, 10, 17,
                10, 1, 17,
                10, 13, 11,
                10, 11, 1,
                11, 33, 1,
                11, 14, 36,
                11, 36, 33,
                12, 13, 10,
                13, 14, 11,                
                15, 16, 20,
                15, 20, 19,
                16, 2, 20,
                16, 38, 2,                
                1, 33, 32,
                1, 32, 39,                
                18, 19, 20,
                18, 20, 2,
                18, 2, 40,
                23, 19, 18,
                24, 19, 23,
                25, 45, 44,
                25, 0, 45,
                26, 25, 44,
                27, 44, 43,
                27, 26, 44,
                28, 27, 43,
                29, 28, 43,
                30, 46, 41,
                30, 43, 46,
                30, 29, 43,
                31, 37, 39,
                31, 30, 37,
                32, 31, 39,
                34, 31, 32,
                35, 32, 33,
                35, 34, 32,
                36, 35, 33,
                37, 41, 42,
                37, 42, 38,
                37, 30, 41,
                38, 42, 2,                
                40, 2, 42,
                40, 42, 41,
                40, 41, 45,
                45, 41, 46,
                15, 51, 16,
                15, 17, 51,
                17, 1, 39,
                39, 51, 17,
                51, 39, 37,
                51, 37, 38,
                16, 51, 38,  
                23, 22, 21,   
                23, 21, 24,  
                43, 44, 45, 
                43, 45, 46
            };
        }

        private static List<Vector2> GetFemBaseDots()
        {
            return new List<Vector2>
            {
                new Vector2(-3.234859f, 0.1302203f),
                new Vector2(3.234859f, 0.1302203f),
                new Vector2(0f, -6.304063f),
                new Vector2(0f, 4.370024f),
            };
        }
        private static List<Vector2> GetMaleBaseDots()
        {
            return new List<Vector2>
            {
                new Vector2(-3.237928f, 0.587957f),
                new Vector2(3.237928f, 0.587957f),
                new Vector2(0f, -5.995237f),
                new Vector2(0f, 4.774317f),
            };
        }
        private static List<Vector2> GetChildBaseDots()
        {
            return new List<Vector2>
            {
                new Vector2(-2.785543f, -0.9262635f),
                new Vector2(2.785543f, -0.9262635f),
                new Vector2(0f, -6.104384f),
                new Vector2(0f, 2.81779f),
            };
        }

        public static List<Vector2> GetProfileBaseDots(ManType manType)
        {
            switch (manType)
            {
                case ManType.Female:
                    return GetFemProfileBaseDots();
                case ManType.Male:
                    return GetMaleProfileBaseDots();
                case ManType.Child:
                    return GetChildProfileBaseDots();
                case ManType.Custom:
                    return ProgramCore.Project.ProfileBaseDots.Select(x => x.Value).ToList();
                default:
                    return null;
            }
        }
        private static List<Vector2> GetFemProfileBaseDots()
        {
            var dots = GetDots(ManType.Female);
            return new List<Vector2>
            {
                new Vector2(0.0f, dots[0].Value.Y),
                new Vector2(9.295802f, dots[18].Value.Y),
                new Vector2(10.99835f, dots[19].Value.Y),
                new Vector2(9.6625f, dots[16].Value.Y),
                new Vector2(9.162499f, dots[51].Value.Y),
                new Vector2(8.993429f, dots[1].Value.Y),
                new Vector2(3.0f, dots[11].Value.Y)
            };
        }
        private static List<Vector2> GetMaleProfileBaseDots()
        {
            var dots = GetDots(ManType.Male);
            return new List<Vector2>
            {
                new Vector2(0.0f, dots[0].Value.Y),
                new Vector2(9.418807f, dots[18].Value.Y),
                new Vector2(11.02712f, dots[19].Value.Y),
                new Vector2(9.875745f, dots[16].Value.Y),
                new Vector2(9.433612f, dots[51].Value.Y),
                new Vector2(9.477399f, dots[1].Value.Y),
                new Vector2(3.0f, dots[11].Value.Y)
            };
        }
        private static List<Vector2> GetChildProfileBaseDots()
        {
            var dots = GetDots(ManType.Child);
            return new List<Vector2>
            {
                new Vector2(0.0f, dots[0].Value.Y),
                new Vector2(8.153842f, dots[18].Value.Y),
                new Vector2(9.431479f, dots[19].Value.Y),
                new Vector2(8.935352f, dots[16].Value.Y),
                new Vector2(8.376743f, dots[51].Value.Y),
                new Vector2(8.371612f, dots[1].Value.Y),
                new Vector2(3.0f, dots[11].Value.Y)
            };
        }

        public static HeadPoints<HeadPoint> GetDots(ManType manType)
        {
            switch (manType)
            {
                case ManType.Female:
                case ManType.Custom:            // для произвольной берем примерные точки
                    return GetFemDots();
                case ManType.Male:
                    return GetMaleDots();
                case ManType.Child:
                    return GetChildDots();
                default:
                    return null;
            }
        }

        private static HeadPoints<HeadPoint> GetFemDots()
        {
            var newPoints = new HeadPoints<HeadPoint>
            {
                new HeadPoint(0f, 11.67331f),                       //0        // центр лба
                new HeadPoint(0f, -7.552899f),                       //1        // Рот
                new HeadPoint(0f, -4.508208f),                       //2        // Центр носа
                new HeadPoint(-2.786755f, 11.16117f),                       //3        // Левые верхние точки головы
                new HeadPoint(-5.709689f, 9.299875f),                       //4        // Левые верхние точки головы
                new HeadPoint(-6.909634f, 7.514232f),                       //5        // Левые верхние точки головы    
                new HeadPoint(-7.634505f, 4.589076f),                       //6        // Левые верхние точки головы
                new HeadPoint(-7.394f, 0.1770133f),                       //7 +      // Начало левого уха
                new HeadPoint(-6.864106f, -4.398919f),                       //8 +      // Начало левого уха
                new HeadPoint(-5.42075f, -8.08329f),                       //9        // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-4.217142f, -9.371833f),                       //10       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-1.009934f, -10.85862f),                       //11       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-4.741577f, -9.973206f),                       //12       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-2.835503f, -11.6133f),                       //13       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-0.9294291f, -12.60291f),                       //14       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-2.505571f, -6.369632f),                       //15        // Рот    
                new HeadPoint(-1.022702f, -5.397916f),                       //16        // Рот  
                new HeadPoint(-1.616331f, -7.183366f),                       //17        // Рот 
                new HeadPoint(-0.9193307f, 0.1962141f),                       //18        // Верхняя левая часть носа  
                new HeadPoint(-1.713433f, -3.870751f),                            //19        // Нижняя левая часть носа
                new HeadPoint(-1.23173f, -4.339445f),                       //20        // Нижняя левая часть носа    
                new HeadPoint(-4.48324f, 0.133942f),                       //21        // Левый глаз
                new HeadPoint(-3.237061f, 0.6147068f),                       //22        // Левый глаз
                new HeadPoint(-1.940786f, -0.04412243f),                       //23        // Левый глаз
                new HeadPoint(-3.223283f, -0.27016f),                       //24        // Левый глаз
                new HeadPoint(2.786755f, 11.16117f),                           //25        // тоже самое для правой части
                new HeadPoint(5.709689f, 9.299875f),                           //26
                new HeadPoint(6.909634f, 7.514232f),                           //27
                new HeadPoint(7.634505f, 4.589076f),                           //28
                new HeadPoint(7.394f, 0.1770133f),                             //29 +
                new HeadPoint(6.864106f, -4.398919f),                          //30 +
                new HeadPoint(5.42075f, -8.08329f),                            //31
                new HeadPoint(4.217142f, -9.371833f),                          //32
                new HeadPoint(1.009934f, -10.85862f),                          //33
                new HeadPoint(4.741577f, -9.973206f),                          //34
                new HeadPoint(2.835503f, -11.6133f),                           //35
                new HeadPoint(0.9294291f, -12.60291f),                         //36
                new HeadPoint(2.505571f, -6.369632f),                          //37        // Рот
                new HeadPoint(1.022702f, -5.397916f),                          //38        // Рот
                new HeadPoint(1.616331f, -7.183366f),                          //39        // Рот
                new HeadPoint(0.9193307f, 0.1962141f),                         //40        // Верхняя правая часть носа  
                new HeadPoint(1.713433f, -3.870751f),                          //41        // Нижняя правая часть носа
                new HeadPoint(1.23173f, -4.339445f),                           //42        // Нижняя правая часть носа
                new HeadPoint(4.48324f, 0.133942f),                            //43        // Правый глаз
                new HeadPoint(3.237061f, 0.6147068f),                          //44        // Правый глаз
                new HeadPoint(1.940786f, -0.04412243f),                        //45        // Правый глаз
                new HeadPoint(3.223283f, -0.27016f),                           //46        // Правый глаз
                new HeadPoint(-8.405126f, 1.408247f)   { Visible = false },   //47 - 7
                new HeadPoint(-7.771821f, -4.666965f)   { Visible = false },   //48 - 8
                new HeadPoint(8.405126f, 1.408247f)     { Visible = false },    //49 - 29
                new HeadPoint(7.771821f, -4.666965f)    { Visible = false },     //50 - 30
                new HeadPoint(0f, -6.405003f)                                   //51            //центр рта
            };

            newPoints[7].LinkedPoints.Add(47);      // уши
            newPoints[8].LinkedPoints.Add(48);
            newPoints[29].LinkedPoints.Add(49);
            newPoints[30].LinkedPoints.Add(50);

            return newPoints;
        }
        private static HeadPoints<HeadPoint> GetMaleDots()
        {
            var newPoints = new HeadPoints<HeadPoint>
            {
                new HeadPoint(0f, 12.13549f),                      //0        // центр лба
                new HeadPoint(0f, -6.953846f),                      //1        // Рот
                new HeadPoint(0f, -3.985388f),                       //2        // Центр носа
                new HeadPoint(-2.735909f, 11.61245f),                      //3        // Левые верхние точки головы
                new HeadPoint(-5.541705f, 9.890492f),                      //4        // Левые верхние точки головы
                new HeadPoint(-6.81637f, 8.060863f),                      //5        // Левые верхние точки головы
                new HeadPoint(-7.582602f, 5.213355f),                      //6        // Левые верхние точки головы
                new HeadPoint(-7.459032f, 0.9717545f),                       //7 +      // Начало левого уха
                new HeadPoint(-6.759144f, -3.854883f),                       //8 +      // Начало левого уха
                new HeadPoint(-5.56878f, -7.898001f),                      //9        // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-4.181715f, -9.318408f),                       //10       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-0.9582672f, -10.97441f),                        //11       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-4.741577f, -9.973206f),                       //12       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-2.835503f, -11.6133f),                      //13       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-0.9294291f, -12.60291f),                        //14       // Левая нижняя часть (шея и подбородок)
                new HeadPoint(-2.718971f, -6.03537f),                      //15        // Рот    
                new HeadPoint(-1.26607f, -5.090575f),                      //16        // Рот  
                new HeadPoint(-1.703346f, -6.788739f),                       //17        // Рот 
                new HeadPoint(-0.8480585f, 1.063481f),                       //18        // Верхняя левая часть носа  
                new HeadPoint(-1.667018f, -3.7765f),                      //19        // Нижняя левая часть носа
                new HeadPoint(-1.221782f, -4.016049f),                       //20        // Нижняя левая часть носа
                new HeadPoint(-4.423392f, 0.5407493f),                       //21        // Левый глаз
                new HeadPoint(-3.348277f, 0.990295f),                      //22        // Левый глаз
                new HeadPoint(-2.00215f, 0.3559156f),                     //23        // Левый глаз
                new HeadPoint(-3.341507f, 0.120323f),                       //24        // Левый глаз
                new HeadPoint(2.735909f, 11.61245f),                          //25        // тоже самое для правой части
                new HeadPoint(5.541705f, 9.890492f),                          //26
                new HeadPoint(6.81637f, 8.060863f),                           //27
                new HeadPoint(7.582602f, 5.213355f),                          //28
                new HeadPoint(7.459032f, 0.9717545f),                         //29 +
                new HeadPoint(6.759144f, -3.854883f),                         //30 +
                new HeadPoint(5.56878f, -7.898001f),                          //31
                new HeadPoint(4.181715f, -9.318408f),                         //32
                new HeadPoint(0.9582672f, -10.97441f),                         //33
                new HeadPoint(4.741577f, -9.973206f),                         //34
                new HeadPoint(2.835503f, -11.6133f),                          //35
                new HeadPoint(0.9294291f, -12.60291f),                         //36
                new HeadPoint(2.718971f, -6.03537f),                          //37        // Рот
                new HeadPoint(1.26607f, -5.090575f),                          //38        // Рот
                new HeadPoint(1.703346f, -6.788739f),                         //39        // Рот
                new HeadPoint(0.8480585f, 1.063481f),                         //40        // Верхняя правая часть носа  
                new HeadPoint(1.667018f, -3.7765f),                           //41        // Нижняя правая часть носа
                new HeadPoint(1.221782f, -4.016049f),                         //42        // Нижняя правая часть носа
                new HeadPoint(4.423392f, 0.5407493f),                         //43        // Правый глаз
                new HeadPoint(3.348277f, 0.990295f),                          //44        // Правый глаз
                new HeadPoint(2.00215f, 0.3559156f),                          //45        // Правый глаз
                new HeadPoint(3.341507f, 0.120323f),                          //46        // Правый глаз
                new HeadPoint(-8.3811f, 1.760967f)      { Visible = false },   //47 - 7
                new HeadPoint(-7.651234f, -4.290586f) { Visible = false },   //48 - 8
                new HeadPoint(8.3811f, 1.760967f)   { Visible = false },    //49 - 29
                new HeadPoint(7.651234f, -4.290586f) { Visible = false },     //50 - 30

                new HeadPoint(0f, -6.009818f)              //51            //центр рта
            };

            newPoints[7].LinkedPoints.Add(47);      // уши
            newPoints[8].LinkedPoints.Add(48);
            newPoints[29].LinkedPoints.Add(49);
            newPoints[30].LinkedPoints.Add(50);

            return newPoints;
        }
        private static HeadPoints<HeadPoint> GetChildDots()
        {
            var newPoints = new HeadPoints<HeadPoint>
            {
               new HeadPoint(0f, 9.876677f),                           //0        // центр лба
               new HeadPoint(0f, -6.956298f),                           //1        // Рот
               new HeadPoint(0f, -4.286997f),                           //2        // Центр носа
               new HeadPoint(-2.436944f, 9.44833f),                           //3        // Левые верхние точки головы
               new HeadPoint(-5.149078f, 7.737719f),                           //4        // Левые верхние точки головы
               new HeadPoint(-6.353214f, 5.951503f),                           //5        // Левые верхние точки головы
               new HeadPoint(-7.049546f, 3.587214f),                           //6        // Левые верхние точки головы
               new HeadPoint(-6.950691f, 0.06600415f),                           //7 +      // Начало левого уха
               new HeadPoint(-6.213909f, -4.36259f),                           //8 +      // Начало левого уха
               new HeadPoint(-5.243114f, -6.8904f),                           //9        // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-4.022252f, -8.074624f),                           //10       // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-1.048055f, -9.454509f),                           //11       // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-4.741577f, -9.973206f),                           //12       // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-2.835503f, -11.6133f),                           //13       // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-0.9294291f, -12.60291f),                           //14       // Левая нижняя часть (шея и подбородок)
               new HeadPoint(-1.865658f, -6.201596f),                           //15        // Рот    
               new HeadPoint(-0.8154634f, -5.328717f),                           //16        // Рот  
               new HeadPoint(-1.162038f, -6.793632f),                           //17        // Рот 
               new HeadPoint(-0.6773607f, -0.6424349f),                           //18        // Верхняя левая часть носа  
               new HeadPoint(-1.328793f, -3.892325f),                           //19        // Нижняя левая часть носа
               new HeadPoint(-1.063392f, -4.242681f),                           //20        // Нижняя левая часть носа    
               new HeadPoint(-3.91729f, -1.073645f),                           //21        // Левый глаз
               new HeadPoint(-2.92597f, -0.5019711f),                           //22        // Левый глаз
               new HeadPoint(-1.645102f, -1.109842f),                           //23        // Левый глаз
               new HeadPoint(-2.791351f, -1.376829f),                           //24        // Левый глаз
                new HeadPoint(2.436944f, 9.44833f),                             //25        // тоже самое для правой части
                new HeadPoint(5.149078f, 7.737719f),                            //26
                new HeadPoint(6.353214f, 5.951503f),                            //27
                new HeadPoint(7.049546f, 3.587214f),                            //28
                new HeadPoint(6.950691f, 0.06600415f),                          //29 +
                new HeadPoint(6.213909f, -4.36259f),                            //30 +
                new HeadPoint(5.243114f, -6.8904f),                             //31
                new HeadPoint(4.022252f, -8.074624f),                           //32
                new HeadPoint(1.048055f, -9.454509f),                           //33
                new HeadPoint(4.741577f, -9.973206f),                           //34
                new HeadPoint(2.835503f, -11.6133f),                            //35
                new HeadPoint(0.9294291f, -12.60291f),                          //36
                new HeadPoint(1.865658f, -6.201596f),                           //37        // Рот
                new HeadPoint(0.8154634f, -5.328717f),                          //38        // Рот
                new HeadPoint(1.162038f, -6.793632f),                           //39        // Рот
                new HeadPoint(0.6773607f, -0.6424349f),                         //40        // Верхняя правая часть носа  
                new HeadPoint(1.328793f, -3.892325f),                           //41        // Нижняя правая часть носа
                new HeadPoint(1.063392f, -4.242681f),                           //42        // Нижняя правая часть носа
                new HeadPoint(3.91729f, -1.073645f),                            //43        // Правый глаз
                new HeadPoint(2.92597f, -0.5019711f),                           //44        // Правый глаз
                new HeadPoint(1.645102f, -1.109842f),                           //45        // Правый глаз
                new HeadPoint(2.791351f, -1.376829f),                           //46        // Правый глаз

                new HeadPoint(-7.784023f, 0.5661672f){ Visible = false },       //47 - 7
                new HeadPoint(-6.888096f, -4.781533f) { Visible = false },      //48 - 8
                new HeadPoint(7.784023f, 0.5661672f){ Visible = false },        //49 - 29
                new HeadPoint(6.888096f, -4.781533f) { Visible = false },       //50 - 30

                new HeadPoint(0f, -6.046123f),                        //51            //центр рта
            };

            newPoints[7].LinkedPoints.Add(47);      // уши
            newPoints[8].LinkedPoints.Add(48);
            newPoints[29].LinkedPoints.Add(49);
            newPoints[30].LinkedPoints.Add(50);

            return newPoints;
        }

        public List<int> GetFaceIndexes()
        {
            return new List<int> { 0, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 47, 48, 49, 50 };
        }
        public List<int> GetMouthIndexes()
        {
            return new List<int> { 1, 15, 16, 17, 37, 38, 39, 51 };
        }
        public List<int> GetLeftEyeIndexes()
        {
            return new List<int> { 21, 22, 23, 24 };
        }
        public List<int> GetRightEyeIndexes()
        {
            return new List<int> { 43, 44, 45, 46 };
        }
        public List<int> GetNoseIndexes()
        {
            return new List<int> { 2, 18, 19, 20, 40, 41, 42 };
        }

        #endregion

        #region Autodots

        public void SelectAutodotsFaceEllipse()
        {
            if (AutoDots.Count == 0)
                return;
            AutoDots.ClearSelection();

            foreach (var index in GetFaceIndexes())
                AutoDots[index].Selected = true;
        }
        public void SelectAutdotsMouth()
        {
            if (AutoDots.Count == 0)
                return;
            AutoDots.ClearSelection();

            foreach (var index in GetMouthIndexes())
                AutoDots[index].Selected = true;
        }
        public void SelectAutodotsLeftEye()
        {
            if (AutoDots.Count == 0)
                return;
            AutoDots.ClearSelection();

            foreach (var index in GetLeftEyeIndexes())
                AutoDots[index].Selected = true;
        }
        public void SelectAutodotsRightEye()
        {
            if (AutoDots.Count == 0)
                return;
            AutoDots.ClearSelection();

            foreach (var index in GetRightEyeIndexes())
                AutoDots[index].Selected = true;
        }
        public void SelectAutodotsNose()
        {
            if (AutoDots.Count == 0)
                return;
            AutoDots.ClearSelection();

            foreach (var index in GetNoseIndexes())
                AutoDots[index].Selected = true;
        }

        public List<MirroredHeadPoint> GetSpecialAutodots(List<int> indexes)
        {
            var result = new List<MirroredHeadPoint>();
            if (AutoDots.Count == 0)
                return result;

            foreach (var index in indexes)
                result.Add(AutoDots[index]);

            return result;
        }
        public void UpdateAutodotsPointSelection(float x, float y)
        {
            foreach (var elem in AutoDots)
            {
                if (!elem.Visible)
                    continue;

                Vector2 absolutePoint;

                absolutePoint.X = (elem.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX) + 2.5f;
                absolutePoint.Y = (elem.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY) + 2.5f;

                if (absolutePoint.X >= x - 5 && absolutePoint.X <= x + 5 && absolutePoint.Y >= y - 5 && absolutePoint.Y <= y + 5)
                {
                    elem.Selected = !elem.Selected;
                    foreach (var linkedPointIndex in elem.LinkedPoints)
                    {
                        var linkedPoint = AutoDots[linkedPointIndex];
                        linkedPoint.Selected = !linkedPoint.Selected;
                    }

                    return;
                }
            }
        }

        /// <summary> Процедура автоточек </summary>
        public void StartAutodots()
        {
            if (AutoDots.Count != 0)
                return;

            ShapeDots.Clear();
            foreach (var dot in ProgramCore.MainForm.ctrlRenderControl.headMeshesController.TexturingInfo.Points)
            {
                var newPoint = new MirroredHeadPoint(dot.Value, dot.Value);
                newPoint.Visible = dot.Visible;
                newPoint.LinkedPoints.AddRange(dot.LinkedPoints);
                AutoDots.Add(newPoint);

                ShapeDots.Add(newPoint.Clone() as MirroredHeadPoint);        // по дефолту шейп-точки берутся из автодотсов и потом уже сами по себе
            }

            // Уточняем положение глаз и рта, относительно центра указанного пользователем
            ClarifyEyeAndMouth(new List<int> { 21, 22, 23, 24 }, ProgramCore.Project.LeftEyeCenter);  // Left eye
            ClarifyEyeAndMouth(new List<int> { 43, 44, 45, 46 }, ProgramCore.Project.RightEyeCenter);  // Right eye

            ClarifyEyeAndMouth(new List<int> { 1, 15, 16, 17, 37, 38, 39, 51 }, ProgramCore.Project.MouthCenter);  // Mouth

            ProgramCore.MainForm.ctrlTemplateImage.UpdateUserCenterPositions(false, true);
        }
        public void EndAutodots(bool needClear = true)
        {
            var results = new List<Vector2>();
            foreach (var point in AutoDots)
                results.Add(point.ValueMirrored);

            ProgramCore.MainForm.ctrlRenderControl.headMeshesController.UpdateTexCoors(results);
            if (needClear)
                AutoDots.ClearSelection();
        }

        #endregion

        #region Shapedots

        public void SelectShapedotsFaceEllipse()
        {
            if (ShapeDots.Count == 0)
                return;
            ShapeDots.ClearSelection();

            foreach (var index in GetFaceIndexes())
                ShapeDots[index].Selected = true;
        }
        public void SelectShapedotsMouth()
        {
            if (ShapeDots.Count == 0)
                return;
            ShapeDots.ClearSelection();

            foreach (var index in GetMouthIndexes())
                ShapeDots[index].Selected = true;
        }
        public void SelectShapedotsLeftEye()
        {
            if (ShapeDots.Count == 0)
                return;
            ShapeDots.ClearSelection();

            foreach (var index in GetLeftEyeIndexes())
                ShapeDots[index].Selected = true;
        }
        public void SelectShapedotsRightEye()
        {
            if (ShapeDots.Count == 0)
                return;
            ShapeDots.ClearSelection();

            foreach (var index in GetRightEyeIndexes())
                ShapeDots[index].Selected = true;
        }
        public void SelectShapedotsNose()
        {
            if (ShapeDots.Count == 0)
                return;
            ShapeDots.ClearSelection();

            foreach (var index in GetNoseIndexes())
                ShapeDots[index].Selected = true;
        }

        public List<MirroredHeadPoint> GetSpecialShapedots(List<int> indexes)
        {
            var result = new List<MirroredHeadPoint>();
            if (ShapeDots.Count == 0)
                return result;

            foreach (var index in indexes)
                result.Add(ShapeDots[index]);

            return result;
        }
        public void UpdateShapedotsPointSelection(float x, float y)
        {
            foreach (var elem in ShapeDots)
            {
                if (!elem.Visible)
                    continue;

                Vector2 absolutePoint;

                absolutePoint.X = (elem.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX) + 2.5f;
                absolutePoint.Y = (elem.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY) + 2.5f;

                if (absolutePoint.X >= x - 5 && absolutePoint.X <= x + 5 && absolutePoint.Y >= y - 5 && absolutePoint.Y <= y + 5)
                {
                    elem.Selected = !elem.Selected;
                    foreach (var linkedPointIndex in elem.LinkedPoints)
                    {
                        var linkedPoint = ShapeDots[linkedPointIndex];
                        linkedPoint.Selected = !linkedPoint.Selected;
                    }

                    return;
                }
            }
        }

        #endregion
    }

    public class HeadLine : Collection<MirroredHeadPoint>
    {
        public bool Contains(Vector2 point)
        {
            foreach (var elem in this)
                if (elem.Value == point)
                    return true;
            return false;
        }
        public void Remove(Vector2 point)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                if (this[i].Value == point)
                {
                    RemoveAt(i);
                    break;
                }
            }
        }
    }

    /// <summary> Точка, используемая для выделения </summary>
    public class HeadSelectedPoint
    {
        public int GlobalIndex;
        public HeadPoint Point;
    }

    /// <summary> Точка, содержащая инфу о зеркальной (отраженной) точке </summary>
    public class MirroredHeadPoint : HeadPoint
    {
        /// <summary> Относительные координаты </summary>
        public Vector2 ValueMirrored;

        public MirroredHeadPoint(Vector2 point, Vector2 mirroredPoint, bool needTransform = true)
            : base(point)
        {
            if (needTransform)
            {
                if (ProgramCore.MainForm.HeadFront)
                {
                    var v = point - new Vector2(ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.A.X, ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.B.Y);
                    v.X /= ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.Size.X;
                    v.Y /= (-ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.Size.Y);

                    v.X = ProgramCore.Project.FaceRectRelative.Width * v.X + ProgramCore.Project.FaceRectRelative.X;
                    v.Y = ProgramCore.Project.FaceRectRelative.Height * v.Y + ProgramCore.Project.FaceRectRelative.Y;
                    ValueMirrored = v;
                }
                else if (ProgramCore.MainForm.HeadProfile)
                {
                    var v = point - new Vector2(ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.X, ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Y + ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Height);
                    v.X /= ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Width;
                    v.Y /= (-ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Height);

                    v.X = ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Width * v.X + ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.X;
                    v.Y = ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Height * v.Y + ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Y;
                    ValueMirrored = v;
                }
            }
            else
                ValueMirrored = mirroredPoint;
        }
        public MirroredHeadPoint(HeadPoint point)
            : base(point.Value)
        {
            Visible = point.Visible;
            LinkedPoints.AddRange(point.LinkedPoints);
        }

        public void UpdateWorldPoint()
        {
            Value = UpdateWorldPoint(ValueMirrored);
        }

        public static Vector2 UpdateWorldPoint(Vector2 valueMirrored)
        {
            var result = new Vector2();
            if (ProgramCore.MainForm.HeadFront)
            {
                Vector2 v;
                v.X = (valueMirrored.X - ProgramCore.Project.FaceRectRelative.X) / ProgramCore.Project.FaceRectRelative.Width;
                v.Y = (valueMirrored.Y - ProgramCore.Project.FaceRectRelative.Y) / ProgramCore.Project.FaceRectRelative.Height;

                result.X = v.X * ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.Size.X + ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.A.X;
                result.Y = v.Y * (-ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.Size.Y) + ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh.AABB.B.Y;
            }
            else if (ProgramCore.MainForm.HeadProfile)
            {
                Vector2 v;
                v.X = (valueMirrored.X - ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.X) / ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Width;
                v.Y = (valueMirrored.Y - ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Y) / ProgramCore.MainForm.ctrlTemplateImage.ProfileFaceRect.Height;

                result.X = v.X * ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Width + ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.X;
                result.Y = v.Y * (-ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Height) + (ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Y + ProgramCore.MainForm.ctrlRenderControl.ProfileFaceRect.Height);
            }
            return result;
        }

        public override HeadPoint Clone()
        {
            var newPoint = new MirroredHeadPoint(Value, ValueMirrored, false);
            newPoint.Visible = Visible;
            newPoint.LinkedPoints.AddRange(LinkedPoints);
            return newPoint;
        }

        /// <summary> Проверить входит ли точка в лассо выделение </summary>
        /// <param name="lassoPoints"></param>
        public override void CheckLassoSelection(List<Vector2> lassoPoints)
        {
            Selected = false;

            Vector2 absolutePoint;
            absolutePoint.X = (ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX);
            absolutePoint.Y = (ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);

            var count = 0;
            for (var i = 0; i < lassoPoints.Count; i++)
            {
                var j = (i + 1) % lassoPoints.Count;
                var p0 = lassoPoints[i];
                var p1 = lassoPoints[j];

                if (p0.Y == lassoPoints[j].Y)
                    continue;
                if (p0.Y > absolutePoint.Y && p1.Y > absolutePoint.Y)
                    continue;
                if (p0.Y < absolutePoint.Y && p1.Y < absolutePoint.Y)
                    continue;
                if (Math.Max(p0.Y, p1.Y) == absolutePoint.Y)
                    count++;
                else
                {
                    if (Math.Min(p0.Y, p1.Y) == absolutePoint.Y)
                        continue;

                    var t = (absolutePoint.Y - p0.Y) / (p1.Y - p0.Y);
                    if (p0.X + t * (p1.X - p0.X) >= absolutePoint.X)
                        count++;
                }
            }
            if (count % 2 == 1)
                Selected = true;
        }

        public void ToStreamM(BinaryWriter bw)
        {
            ToStream(bw);

            bw.Write(ValueMirrored.X);
            bw.Write(ValueMirrored.Y);
        }
        public static MirroredHeadPoint FromStreamW(BinaryReader br)
        {
            var pt = FromStream(br);
            var result = new MirroredHeadPoint(pt);

            result.ValueMirrored = new Vector2(br.ReadSingle(), br.ReadSingle());
            return result;
        }
    }


}
