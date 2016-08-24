using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using RH.HeadEditor.Helpers;
using RH.HeadShop.Helpers;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Helpers;

namespace RH.HeadShop.Render
{
    public partial class ctrlTemplateImage : UserControlEx
    {
        #region Var

        public Vector2 ModelAdaptParam
        {
            get
            {
                var v = new Vector2(pictureTemplate.Width, pictureTemplate.Height);
                var p0 = new Vector2(EyesMouthRectTransformed.X, EyesMouthRectTransformed.Y);
                var p1 = new Vector2(EyesMouthRectTransformed.X + EyesMouthRectTransformed.Width, EyesMouthRectTransformed.Y + EyesMouthRectTransformed.Height);
                return new Vector2(v.Length / (p1 - p0).Length, 1.0f - ((p0.Y + p1.Y) * 0.5f) / v.Y);
            }
        }

        public Vector2 ModelAdaptParamProfile
        {
            get
            {
                var v = new Vector2(pictureTemplate.Width, pictureTemplate.Height);
                var point = ProgramCore.MainForm.ctrlTemplateImage.profileControlPoints[0].ValueMirrored;
                var p1 = new Vector2(point.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                              point.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                point = ProgramCore.MainForm.ctrlTemplateImage.profileControlPoints[3].ValueMirrored;
                var p0 = new Vector2(point.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                              point.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                return new Vector2(v.Length / (p1 - p0).Length, 1.0f - ((p0.Y + p1.Y) * 0.5f) / v.Y);
            }
        }

        /// <summary> Реальная позиция картинки на контроле </summary>
        private int OriginalImageTemplateWidth;
        private int OriginalImageTemplateHeight;
        private int ImageTemplateCenterX;
        private int ImageTemplateCenterY;
        private int ImageTemplateOldWidth;
        public int ImageTemplateWidth;
        public int ImageTemplateHeight;
        public int ImageTemplateOffsetX;
        public int ImageTemplateOffsetY;

        private bool startMove;
        private Vector2 startMousePoint;
        private bool leftMousePressed;
        private bool shiftKeyPressed;
        private bool dblClick;

        private float imageScale = 1.0f;
        private int moveRectIndex = -1;
        private Vector2 tempMoveRectCenter;         // старые значения прямоугольника, для сжимания-расжимания точек
        private float tempMoveRectWidth;
        private float tempMoveRectHeight;

        private Vector2 headLastPointRelative = Vector2.Zero;
        private Vector2 headLastPoint = Vector2.Zero;
        private Vector2 tempOffsetPoint;

        private readonly List<MirroredHeadPoint> headTempPoints = new List<MirroredHeadPoint>();

        /// <summary> прямоугольник глаза-рот, Подогнанный под контрол </summary>
        public RectangleF EyesMouthRectTransformed;

        public Vector2 MouthTransformed;
        public Vector2 LeftEyeTransformed;
        public Vector2 RightEyeTransformed;
        public Vector2 NoseTransformed;

        /// <summary> Прямоугольник, охватывающий все автоточки. Нужен для изменения всех точек сразу (сжатие/расжатие) </summary>
        public Rectangle FaceRectTransformed;
        /// <summary> Центральная точка на лбу. Нужна для выделения прямоугольинка автоточек. </summary>
        public Vector2 CentralFacePoint;

        public RectangleF ProfileFaceRect;
        public Rectangle ProfileFaceRectTransformed;

        /// <summary> Точки лассо для автоточек </summary>
        private readonly List<Vector2> headAutodotsLassoPoints = new List<Vector2>();
        /// <summary> Точки лассо для шейпа зеркального </summary>
        private readonly List<Vector2> headShapedotsLassoPoints = new List<Vector2>();

        public bool RectTransformMode;
        public bool LineSelectionMode;     // режим таскания точек при отрисовке линий


        private int profileControlPointIndex = 0;
        public List<MirroredHeadPoint> profileControlPoints = new List<MirroredHeadPoint>();
        public ProfileControlPointsMode ControlPointsMode = ProfileControlPointsMode.None;

        public Image DrawingImage; //!!!

        #endregion

        public ctrlTemplateImage()
        {
            InitializeComponent();

            PreviewKeyDown += ctrlTemplateImage_PreviewKeyDown;
        }

        /// <summary> Заполнить опорные точки для правой модели. Они константы </summary>
        public void InitializeProfileControlPoints()
        {
            var headController = ProgramCore.MainForm.ctrlRenderControl.headController;
            profileControlPoints.Clear();

            if (headController.ShapeDots.Count == 0)
                return;

            profileControlPoints.Add(new MirroredHeadPoint(new Vector2(0.0f, headController.ShapeDots[0].Value.Y), Vector2.Zero, false));       // верх
            profileControlPoints.Add(new MirroredHeadPoint(new Vector2(-6.8f, (headController.ShapeDots[18].Value.Y + headController.ShapeDots[40].Value.Y) * 0.5f), Vector2.Zero, false)); // глаз
            profileControlPoints.Add(new MirroredHeadPoint(new Vector2(-8f, headController.ShapeDots[51].Value.Y), Vector2.Zero, false));      //рот
            profileControlPoints.Add(new MirroredHeadPoint(new Vector2(-3.0f, (headController.ShapeDots[11].Value.Y + headController.ShapeDots[33].Value.Y) * 0.5f), Vector2.Zero, false));

            for (var i = 0; i < 4; i++)
            {
                var sprite = ProgramCore.MainForm.ctrlRenderControl.profilePointSprites[i];
                sprite.Position = new Vector2(-profileControlPoints[i].Value.X, profileControlPoints[i].Value.Y);
            }

        }

        #region Supported void's

        /// <summary> Пересчитать положение прямоугольника в зависимост от размера картинки на picturetemplate </summary>
        public void RecalcRealTemplateImagePosition()
        {
            var pb = pictureTemplate;
            if (DrawingImage == null)
            {
                ImageTemplateWidth = ImageTemplateHeight = 0;
                ImageTemplateOffsetX = ImageTemplateOffsetY = -1;
                EyesMouthRectTransformed = RectangleF.Empty;
                return;
            }

            if (pb.Width / (double)pb.Height < DrawingImage.Width / (double)DrawingImage.Height)
            {
                ImageTemplateWidth = pb.Width;
                ImageTemplateHeight = DrawingImage.Height * ImageTemplateWidth / DrawingImage.Width;
            }
            else if (pb.Width / (double)pb.Height > DrawingImage.Width / (double)DrawingImage.Height)
            {
                ImageTemplateHeight = pb.Height;
                ImageTemplateWidth = DrawingImage.Width * ImageTemplateHeight / DrawingImage.Height;
            }
            else
            {
                ImageTemplateWidth = pb.Width;
                ImageTemplateHeight = pb.Height;
            }
            OriginalImageTemplateWidth = ImageTemplateWidth;
            OriginalImageTemplateHeight = ImageTemplateHeight;

            ImageTemplateOffsetX = (pb.Width - ImageTemplateWidth) / 2;
            ImageTemplateOffsetY = (pb.Height - ImageTemplateHeight) / 2;

            imageScale = 1.0f;

            RecalcEyeMouthRect();
        }
        private void RecalcEyeMouthRect()
        {
            EyesMouthRectTransformed = new RectangleF(ProgramCore.Project.FaceRectRelative.X * ImageTemplateWidth + ImageTemplateOffsetX,
                                          ProgramCore.Project.FaceRectRelative.Y * ImageTemplateHeight + ImageTemplateOffsetY,
                                           ProgramCore.Project.FaceRectRelative.Width * ImageTemplateWidth,
                                           ProgramCore.Project.FaceRectRelative.Height * ImageTemplateHeight);

            RecalcUserCenters();
        }
        private void RecalcUserCenters()
        {
            MouthTransformed = new Vector2(ProgramCore.Project.MouthUserCenter.X * ImageTemplateWidth + ImageTemplateOffsetX,
                                         ProgramCore.Project.MouthUserCenter.Y * ImageTemplateHeight + ImageTemplateOffsetY);
            LeftEyeTransformed = new Vector2(ProgramCore.Project.LeftEyeUserCenter.X * ImageTemplateWidth + ImageTemplateOffsetX,
                                           ProgramCore.Project.LeftEyeUserCenter.Y * ImageTemplateHeight + ImageTemplateOffsetY);
            RightEyeTransformed = new Vector2(ProgramCore.Project.RightEyeUserCenter.X * ImageTemplateWidth + ImageTemplateOffsetX,
                                           ProgramCore.Project.RightEyeUserCenter.Y * ImageTemplateHeight + ImageTemplateOffsetY);
            NoseTransformed = new Vector2(ProgramCore.Project.NoseUserCenter.X * ImageTemplateWidth + ImageTemplateOffsetX,
                               ProgramCore.Project.NoseUserCenter.Y * ImageTemplateHeight + ImageTemplateOffsetY);
        }

        /// <summary> Смасштабировать и повернуть изображение профиля по опорным точкам </summary>
        private void UpdateProfileImageByControlPoints()
        {
            #region Обрезание

            var image = ProgramCore.Project.ProfileImage;
            var leftX = (int)(profileControlPoints.Min(x => x.ValueMirrored.X) * image.Width);
            var topY = (int)(profileControlPoints.Min(x => x.ValueMirrored.Y) * image.Height);
            var bottomY = (int)(profileControlPoints.Max(x => x.ValueMirrored.Y) * image.Height);

            leftX = leftX - 100 < 0 ? 0 : leftX - 100;          // ширину хз как определять, ой-вей
            topY = topY - 10 < 0 ? 0 : topY - 10;
            bottomY = bottomY + 10 > image.Height ? image.Height : bottomY + 10;
            var height = bottomY - topY;
            var faceRectangle = new Rectangle(leftX, topY, image.Width - leftX, height);

            var croppedImage = ImageEx.Crop(image, faceRectangle);
            for (var i = 0; i < profileControlPoints.Count; i++)            // смещаем все точки, чтобы учесть обрезанное
            {
                var point = profileControlPoints[i];
                var pointK = new Vector2(point.ValueMirrored.X * image.Width, point.ValueMirrored.Y * image.Height);        // по старой ширине-высоте

                pointK.X -= leftX;
                pointK.Y -= topY;

                profileControlPoints[i] = new MirroredHeadPoint(point.Value, new Vector2(pointK.X / (croppedImage.Width * 1f), pointK.Y / (croppedImage.Height * 1f)), false);    // и в новые
            }

            #endregion

            #region Поворот

            var xVector = new Vector2(1, 0);

            var vectorLeft = profileControlPoints[2].ValueMirrored - profileControlPoints[1].ValueMirrored; // из глаза рот
            vectorLeft = new Vector2(vectorLeft.X * croppedImage.Width, vectorLeft.Y * croppedImage.Height);
            vectorLeft.Normalize();
            float xDiff = xVector.X - vectorLeft.X;
            float yDiff = xVector.Y - vectorLeft.Y;
            var angleLeft = Math.Atan2(yDiff, xDiff);

            var vectorRight = profileControlPoints[2].Value - profileControlPoints[1].Value;
            vectorRight.Normalize();
            xDiff = xVector.X - vectorRight.X;
            yDiff = xVector.Y - vectorRight.Y;
            var angleRight = -Math.Atan2(yDiff, xDiff);

            var angleDiffRad = angleRight - angleLeft;
            var angleDiff = angleDiffRad * 180.0 / Math.PI;

            using (var ii = ImageEx.RotateImage(croppedImage, (float)angleDiff))
            {
                ProgramCore.Project.ProfileImage = new Bitmap(ii);
                SetTemplateImage(ProgramCore.Project.ProfileImage, false);
            }

            var center = new Vector2(ProgramCore.Project.ProfileImage.Width * 0.5f, ProgramCore.Project.ProfileImage.Height * 0.5f);
            var cosAngle = Math.Cos(angleDiffRad);
            var sinAngle = Math.Sin(angleDiffRad);
            for (var i = 0; i < profileControlPoints.Count; i++)            // смещаем все точки, чтобы учесть обрезанное
            {
                var point = profileControlPoints[i];
                var pointAbsolute = new Vector2(point.ValueMirrored.X * ProgramCore.Project.ProfileImage.Width, point.ValueMirrored.Y * ProgramCore.Project.ProfileImage.Height);        // по старой ширине-высоте

                var newPoint = pointAbsolute - center;
                newPoint = new Vector2((float)(newPoint.X * cosAngle - newPoint.Y * sinAngle),
                                           (float)(newPoint.Y * cosAngle + newPoint.X * sinAngle));
                newPoint += center;

                profileControlPoints[i] = new MirroredHeadPoint(point.Value, new Vector2(newPoint.X / (ProgramCore.Project.ProfileImage.Width * 1f), newPoint.Y / (ProgramCore.Project.ProfileImage.Height * 1f)), false);    // и в новые
            }

            ProgramCore.MainForm.ctrlRenderControl.InitializeProfileCamera(ModelAdaptParamProfile);

            #endregion

            var projectPath = Path.Combine(ProgramCore.Project.ProjectPath, "ProfileImage.jpg");
            ProgramCore.Project.ProfileImage.Save(projectPath);

            ControlPointsMode = ProfileControlPointsMode.UpdateRightLeft;
        }

        private void DrawAutodotsGroupPoints(Graphics g)
        {
            var pointRect = new RectangleF(MouthTransformed.X - 2.5f, MouthTransformed.Y - 2.5f, 5f, 5f);
            g.FillRectangle(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.Lip ? DrawingTools.YellowSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
            pointRect = new RectangleF(NoseTransformed.X - 2.5f, NoseTransformed.Y - 2.5f, 5f, 5f);
            g.FillRectangle(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.Nose ? DrawingTools.YellowSolidBrush : DrawingTools.BlueSolidBrush, pointRect);

            pointRect = new RectangleF(LeftEyeTransformed.X - 2.5f, LeftEyeTransformed.Y - 2.5f, 5f, 5f);
            g.FillRectangle(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.LEye ? DrawingTools.YellowSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
            pointRect = new RectangleF(RightEyeTransformed.X - 2.5f, RightEyeTransformed.Y - 2.5f, 5f, 5f);
            g.FillRectangle(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.REye ? DrawingTools.YellowSolidBrush : DrawingTools.BlueSolidBrush, pointRect);


            pointRect = new RectangleF(CentralFacePoint.X - 2.5f, CentralFacePoint.Y - 2.5f, 5f, 5f);
            g.FillRectangle(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.Head ? DrawingTools.YellowSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
        }
        public void DrawLassoOnPictureBox(Graphics g, bool autodots)
        {
            var points = autodots ? headAutodotsLassoPoints : headShapedotsLassoPoints;
            for (var i = points.Count - 2; i >= 0; i--)
            {
                var pointA = points[i];
                var pointB = points[i + 1];

                g.DrawLine(DrawingTools.GreenPen, pointA.X, pointA.Y, pointB.X, pointB.Y);
            }
            foreach (var point in points)
            {
                var pointRect = new RectangleF(point.X - 2.5f, point.Y - 2.5f, 5f, 5f);
                g.FillRectangle(DrawingTools.BlueSolidBrush, pointRect);
            }

        }
        public void SetTemplateImage(Bitmap image, bool needCameraInitialize = true)
        {
            DrawingImage = image;
            RecalcRealTemplateImagePosition();

            if (needCameraInitialize)
                ProgramCore.MainForm.ctrlRenderControl.InitialiseCamera(ModelAdaptParam);

            pictureTemplate.Refresh();
        }
        public void RefreshPictureBox()
        {
            pictureTemplate.Refresh();
        }

        public void SelectAutodotsByLasso()
        {
            ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.ClearSelection();
            foreach (var point in ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots)
                point.CheckLassoSelection(headAutodotsLassoPoints);

            headAutodotsLassoPoints.Clear();
        }
        public void SelectShapedotsByLasso()
        {
            ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.ClearSelection();
            foreach (var point in ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots)
                point.CheckLassoSelection(headShapedotsLassoPoints);

            headShapedotsLassoPoints.Clear();
        }

        public new void KeyDown(KeyEventArgs e)
        {           // приходится так делать, ибо у нас все перекрывается
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.HeadAutodots)
            {
                if (e.KeyData == (Keys.A))
                    ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.SelectAll();
                else if (e.KeyData == (Keys.D))
                {
                    ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.ClearSelection();
                }
            }
        }

        private void UpdateFaceRect()
        {
            var indicies = ProgramCore.MainForm.ctrlRenderControl.headController.GetFaceIndexes();
            List<MirroredHeadPoint> faceDots;
            switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
            {
                case Mode.HeadShapedots:
                case Mode.HeadLine:
                case Mode.HeadShapeFirstTime:
                case Mode.HeadShape:
                    faceDots = ProgramCore.MainForm.ctrlRenderControl.headController.GetSpecialShapedots(indicies);
                    break;
                default:
                    faceDots = ProgramCore.MainForm.ctrlRenderControl.headController.GetSpecialAutodots(indicies);
                    break;
            }

            if (faceDots.Count == 0)
                return;

            var minX = faceDots.Min(point => point.ValueMirrored.X) * ImageTemplateWidth + ImageTemplateOffsetX;
            var maxX = faceDots.Max(point => point.ValueMirrored.X) * ImageTemplateWidth + ImageTemplateOffsetX;
            var minY = faceDots.Min(point => point.ValueMirrored.Y) * ImageTemplateHeight + ImageTemplateOffsetY;
            var maxY = faceDots.Max(point => point.ValueMirrored.Y) * ImageTemplateHeight + ImageTemplateOffsetY;

            FaceRectTransformed = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));

            CentralFacePoint = new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) / 3f);
        }
        public void UpdateUserCenterPositions(bool onlySelected, bool updateRect)
        {
            var center = UpdateUserCenterPositions(ProgramCore.MainForm.ctrlRenderControl.headController.GetLeftEyeIndexes(), onlySelected);  // Left eye
            if (center != Vector2.Zero)
                ProgramCore.Project.LeftEyeUserCenter = center;

            center = UpdateUserCenterPositions(ProgramCore.MainForm.ctrlRenderControl.headController.GetRightEyeIndexes(), onlySelected);  // Right eye
            if (center != Vector2.Zero)
                ProgramCore.Project.RightEyeUserCenter = center;

            center = UpdateUserCenterPositions(ProgramCore.MainForm.ctrlRenderControl.headController.GetMouthIndexes(), onlySelected);  // Mouth
            if (center != Vector2.Zero)
                ProgramCore.Project.MouthUserCenter = center;

            center = UpdateUserCenterPositions(ProgramCore.MainForm.ctrlRenderControl.headController.GetNoseIndexes(), onlySelected);  // Nose
            if (center != Vector2.Zero)
                ProgramCore.Project.NoseUserCenter = center;

            #region Определяем прямоугольник, охватывающий все автоточки

            if (updateRect)
                UpdateFaceRect();

            #endregion

            RecalcUserCenters();
        }
        private Vector2 UpdateUserCenterPositions(IEnumerable<int> indexes, bool onlySelected)
        {
            List<MirroredHeadPoint> sourcePoints;
            switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
            {
                case Mode.HeadShapedots:
                case Mode.HeadLine:
                case Mode.HeadShapeFirstTime:
                case Mode.HeadShape:
                    sourcePoints = ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots;
                    break;
                default:
                    sourcePoints = ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots;
                    break;
            }
            if (sourcePoints.Count == 0)
                return Vector2.Zero;

            var hasSelected = false;
            var dots = new List<MirroredHeadPoint>();
            foreach (var index in indexes)
            {
                var dot = sourcePoints[index];
                dots.Add(dot);

                if (!onlySelected || dot.Selected)
                    hasSelected = true;
            }

            if (!hasSelected)
                return Vector2.Zero;

            var minX = dots.Min(point => point.ValueMirrored.X);
            var maxX = dots.Max(point => point.ValueMirrored.X);
            var minY = dots.Min(point => point.ValueMirrored.Y);
            var maxY = dots.Max(point => point.ValueMirrored.Y);

            return new Vector2((maxX + minX) * 0.5f, (maxY + minY) * 0.5f);
        }

        private void UpdateProfileRectangle()
        {
            ProfileFaceRect = ProfileFaceRectTransformed = Rectangle.Empty;
            if (!ProgramCore.MainForm.HeadProfile || profileControlPoints.Count == 0)
                return;

            var pointUp = profileControlPoints[0];
            var pointBottom = profileControlPoints[3];

            var width = Math.Max(pointUp.ValueMirrored.X, pointBottom.ValueMirrored.X) - Math.Min(pointUp.ValueMirrored.X, pointBottom.ValueMirrored.X);
            var height = Math.Max(pointUp.ValueMirrored.Y, pointBottom.ValueMirrored.Y) - Math.Min(pointUp.ValueMirrored.Y, pointBottom.ValueMirrored.Y);

            var center = (pointUp.ValueMirrored + pointBottom.ValueMirrored) * 0.5f;


            ProfileFaceRect = new RectangleF(center.X - width * 0.5f, center.Y - height * 0.5f, width, height);
            ProfileFaceRectTransformed = new Rectangle((int)(ProfileFaceRect.X * ImageTemplateWidth + ImageTemplateOffsetX),
                                                       (int)(ProfileFaceRect.Y * ImageTemplateHeight + ImageTemplateOffsetY),
                                                        (int)(ProfileFaceRect.Width * ImageTemplateWidth),
                                                       (int)(ProfileFaceRect.Height * ImageTemplateHeight));

            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 2)
            {
                var currentLine = ProgramCore.MainForm.ctrlRenderControl.headController.Lines[0];
                foreach (var point in currentLine)
                    point.UpdateWorldPoint();
            }
        }
        public void ResetProfileRects()
        {
            ProfileFaceRectTransformed = Rectangle.Empty;
            ProfileFaceRect = RectangleF.Empty;
        }

        public void FinishLine()
        {
            switch (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode)
            {
                case MeshPartType.Nose:
                    break;
                case MeshPartType.LEye: // все, кроме носа. нос замыкать нельзя!
                case MeshPartType.REye:
                case MeshPartType.Head:
                case MeshPartType.ProfileTop:
                case MeshPartType.ProfileBottom:
                case MeshPartType.Lip:
                case MeshPartType.None:
                    if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count != 0)
                    {
                        if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.Head && ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 1)
                            break; // рисуем вторую линию, ее замыкать нельзя!

                        if (ProgramCore.MainForm.HeadProfile)
                        {
                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 1)
                            {
                                var line = ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Last();
                                if (line.Count >= 1)
                                    ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());
                            }

                            ProgramCore.MainForm.ctrlRenderControl.UpdateProfileRectangle();
                            UpdateProfileRectangle();
                        }
                        else
                        {
                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 1)
                            {
                                var line = ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Last();
                                if (line.Count > 1)
                                {
                                    line.Add(line.First());

                                    ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());
                                }
                            }
                        }
                    }
                    break;
            }
            LineSelectionMode = false;
        }

        #endregion

        #region Form's event

        private void ctrlTemplateImage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == (Keys.A))
                ProgramCore.MainForm.ctrlRenderControl.headController.SelectAll();
            else if (e.KeyData == (Keys.D))
                ProgramCore.MainForm.ctrlRenderControl.headController.ClearPointsSelection();
            else if (e.KeyData == (Keys.ShiftKey | Keys.Shift))
                shiftKeyPressed = true;
            else if (e.KeyData == Keys.Enter)
            {
                switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                {
                    case Mode.HeadLine:
                        if (ProgramCore.MainForm.HeadProfile)
                        {
                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 2)
                            {
                                if (ProgramCore.MainForm.ctrlRenderControl.headController.AllPoints.Count > 3)
                                {
                                    foreach (var point in ProgramCore.MainForm.ctrlRenderControl.headController.AllPoints)
                                        point.UpdateWorldPoint();

                                    #region История (undo)

                                    Dictionary<Guid, MeshUndoInfo> undoInfo;
                                    ProgramCore.MainForm.ctrlRenderControl.headMeshesController.GetUndoInfo(out undoInfo);
                                    var isProfile = ProgramCore.MainForm.HeadProfile;
                                    var teInfo = isProfile ? ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeProfileInfo : ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeInfo;
                                    var historyElem = new HistoryHeadShapeLines(undoInfo, null, teInfo, isProfile);
                                    ProgramCore.MainForm.ctrlRenderControl.historyController.Add(historyElem);

                                    #endregion

                                    var userPoints = ProgramCore.MainForm.ctrlRenderControl.headController.AllPoints.Select(x => x.Value).ToList();
                                    ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.Transform(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode, userPoints.Select(p => new Vector2(-p.X, p.Y)).ToList(), Vector2.Zero);
                                }

                                ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Clear();
                                ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.ProfileTop ? MeshPartType.ProfileBottom : MeshPartType.ProfileTop;
                                ProgramCore.MainForm.ctrlRenderControl.UpdateProfileRectangle();
                            }
                            else
                                FinishLine();
                        }
                        else
                            FinishLine();
                        break;
                    case Mode.None:
                        if (ProgramCore.MainForm.HeadProfile)
                            switch (ControlPointsMode)
                            {
                                case ProfileControlPointsMode.MoveControlPoints:
                                    UpdateProfileImageByControlPoints();

                                    UpdateProfileRectangle();
                                    ProgramCore.MainForm.ctrlRenderControl.UpdateProfileRectangle();
                                    ControlPointsMode = ProfileControlPointsMode.UpdateRightLeft;
                                    break;
                                case ProfileControlPointsMode.UpdateRightLeft:
                                    UpdateProfileRectangle();
                                    foreach (var point in profileControlPoints)
                                        point.Selected = false;

                                    ControlPointsMode = ProfileControlPointsMode.None;
                                    break;
                            }

                        break;
                }
            }
        }

        private void ctrlTemplateImage_KeyUp(object sender, KeyEventArgs e)
        {
            shiftKeyPressed = false;
        }

        private void pictureTemplate_Resize(object sender, EventArgs e)
        {
            RecalcRealTemplateImagePosition();
            if (ProgramCore.Project != null)
            {
                UpdateUserCenterPositions(true, true);
                UpdateProfileRectangle();
            }
        }
        private void pictureTemplate_Paint(object sender, PaintEventArgs e)
        {
            if (ProgramCore.MainForm == null)
                return;

            e.Graphics.DrawImage(DrawingImage, ImageTemplateOffsetX, ImageTemplateOffsetY, ImageTemplateWidth, ImageTemplateHeight);

            if (ProgramCore.Debug && ProgramCore.MainForm.HeadFront)
                e.Graphics.DrawRectangle(DrawingTools.GreenPen, EyesMouthRectTransformed.X, EyesMouthRectTransformed.Y, EyesMouthRectTransformed.Width, EyesMouthRectTransformed.Height);


            if (ImageTemplateOffsetX != -1)
                ProgramCore.MainForm.ctrlRenderControl.headController.DrawOnPictureBox(e.Graphics);

            switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
            {
                case Mode.HeadShapedots:
                case Mode.HeadAutodots:
                case Mode.HeadAutodotsFirstTime:
                    DrawAutodotsGroupPoints(e.Graphics);
                    if (RectTransformMode)
                    {
                        e.Graphics.DrawRectangle(DrawingTools.RedPen, FaceRectTransformed);

                        var pointRect = new RectangleF(FaceRectTransformed.X - 5f, FaceRectTransformed.Y - 5f, 10f, 10f);
                        e.Graphics.FillRectangle(DrawingTools.BlueSolidBrush, pointRect);

                        pointRect = new RectangleF(FaceRectTransformed.X + FaceRectTransformed.Width - 5f, FaceRectTransformed.Y - 5f, 10f, 10f);
                        e.Graphics.FillRectangle(DrawingTools.BlueSolidBrush, pointRect);

                        pointRect = new RectangleF(FaceRectTransformed.X + FaceRectTransformed.Width - 5f, FaceRectTransformed.Y + FaceRectTransformed.Height - 5f, 10f, 10f);
                        e.Graphics.FillRectangle(DrawingTools.BlueSolidBrush, pointRect);

                        pointRect = new RectangleF(FaceRectTransformed.X - 5f, FaceRectTransformed.Y + FaceRectTransformed.Height - 5f, 10f, 10f);
                        e.Graphics.FillRectangle(DrawingTools.BlueSolidBrush, pointRect);

                        if (ProgramCore.Project.TextureFlip != FlipType.None)
                        {
                            var centerX = FaceRectTransformed.X + (FaceRectTransformed.Width * 0.5f);
                            e.Graphics.DrawLine(DrawingTools.BluePen, centerX, FaceRectTransformed.Y, centerX, FaceRectTransformed.Bottom);
                        }
                    }

                    break;
                case Mode.HeadAutodotsLassoStart:
                case Mode.HeadAutodotsLassoActive:
                    DrawLassoOnPictureBox(e.Graphics, true);
                    break;
                case Mode.HeadShapedotsLassoStart:
                case Mode.HeadShapedotsLassoActive:
                    DrawLassoOnPictureBox(e.Graphics, false);
                    break;
                case Mode.HeadLine:
                    if (ProgramCore.MainForm.HeadFront)
                    {
                        #region вид спереди

                        DrawAutodotsGroupPoints(e.Graphics);

                        #endregion
                    }
                    else
                    {
                        #region Вид сбоку

                        if (ProgramCore.Debug)
                            e.Graphics.DrawRectangle(DrawingTools.RedPen, ProfileFaceRectTransformed);

                        #region Верхняя и нижняя точки

                        for (var i = 0; i < profileControlPoints.Count; i += 3)
                        {
                            var point = profileControlPoints[i];
                            if (point.ValueMirrored == Vector2.Zero)
                                continue;

                            var pointK = new Vector2(point.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                     point.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                            var pointRect = new RectangleF(pointK.X - 5f, pointK.Y - 5f, 10f, 10f);
                            e.Graphics.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
                        }

                        #endregion

                        #endregion
                    }
                    break;
                case Mode.None:
                    switch (ControlPointsMode)
                    {
                        case ProfileControlPointsMode.SetControlPoints:
                        case ProfileControlPointsMode.MoveControlPoints:
                            {
                                foreach (var point in profileControlPoints)
                                {
                                    if (point.ValueMirrored == Vector2.Zero)
                                        continue;

                                    var pointK = new Vector2(point.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                             point.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                                    var pointRect = new RectangleF(pointK.X - 5f, pointK.Y - 5f, 10f, 10f);
                                    e.Graphics.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
                                }
                            }
                            break;
                        case ProfileControlPointsMode.UpdateRightLeft:

                            #region Верхняя и нижняя точки

                            for (var i = 0; i < profileControlPoints.Count; i += 3)
                            {
                                var point = profileControlPoints[i];
                                if (point.ValueMirrored == Vector2.Zero)
                                    continue;

                                var pointK = new Vector2(point.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                         point.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                                var pointRect = new RectangleF(pointK.X - 5f, pointK.Y - 5f, 10f, 10f);
                                e.Graphics.FillRectangle(point.Selected ? DrawingTools.RedSolidBrush : DrawingTools.BlueSolidBrush, pointRect);
                            }

                            #endregion

                            #region Линии лица

                            foreach (var rect in ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ProfileRects)
                                if (rect.LinkedShapeRect != null)
                                {
                                    for (var i = 1; i < rect.Points.Length; i++)
                                    {
                                        var point1 = new Vector2(-rect.Points[i - 1].X, rect.Points[i - 1].Y);
                                        var point2 = new Vector2(-rect.Points[i].X, rect.Points[i].Y);
                                        var pointM = new MirroredHeadPoint(point1, point1);
                                        var pointB = new MirroredHeadPoint(point2, point2);
                                        e.Graphics.DrawLine(DrawingTools.GreenPen,
                                            (pointM.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX),
                                             pointM.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY,
                                            (pointB.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX),
                                           pointB.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                                    }
                                }

                            #endregion

                            break;
                    }
                    break;
            }

        }

        private void pictureTemplate_MouseDown(object sender, MouseEventArgs e)
        {
            Focus();

            dblClick = e.Clicks == 2;
            if (e.Button == MouseButtons.Left)
            {
                leftMousePressed = true;
                startMove = false;
                headLastPoint = new Vector2(e.X, e.Y);
                headLastPointRelative.X = (e.X - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f);
                headLastPointRelative.Y = (e.Y - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);
                headTempPoints.Clear();

                switch (ProgramCore.MainForm.ctrlRenderControl.ScaleMode)
                {
                    case ScaleMode.Zoom:
                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadShapedots:
                            case Mode.HeadLine:                 // эти моды только для этих режимов!
                            case Mode.HeadAutodots:
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodotsLassoStart:
                            case Mode.HeadAutodotsLassoActive:
                                ImageTemplateCenterX = pictureTemplate.Width / 2 - ImageTemplateOffsetX;
                                ImageTemplateCenterY = pictureTemplate.Height / 2 - ImageTemplateOffsetY;
                                ImageTemplateOldWidth = ImageTemplateWidth;
                                break;
                        }
                        break;
                    case ScaleMode.Move:
                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadShapedots:
                            case Mode.HeadLine: // эти моды только для этих режимов!
                            case Mode.HeadAutodots:
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodotsLassoStart:
                            case Mode.HeadAutodotsLassoActive:

                                tempOffsetPoint = new Vector2(ImageTemplateOffsetX, ImageTemplateOffsetY);
                                break;
                        }
                        break;
                    case ScaleMode.None:

                        #region Обычные режимы

                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodots:
                                {
                                    if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                        return;

                                    if (dblClick)
                                    {
                                        RectTransformMode = false;
                                        ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.ClearSelection();
                                    }

                                    #region Rectangle transforM ?

                                    moveRectIndex = -1;
                                    if (e.X >= FaceRectTransformed.X - 5 && e.X <= FaceRectTransformed.X + 5 && e.Y >= FaceRectTransformed.Y - 5 && e.Y <= FaceRectTransformed.Y + 5)
                                        moveRectIndex = 1;
                                    else if (e.X >= FaceRectTransformed.X + FaceRectTransformed.Width - 5 && e.X <= FaceRectTransformed.X + FaceRectTransformed.Width + 5
                                             && e.Y >= FaceRectTransformed.Y - 5 && e.Y <= FaceRectTransformed.Y + 5)
                                        moveRectIndex = 2;
                                    else if (e.X >= FaceRectTransformed.X + FaceRectTransformed.Width - 5 && e.X <= FaceRectTransformed.X + FaceRectTransformed.Width + 5
                                             && e.Y >= FaceRectTransformed.Y + FaceRectTransformed.Height - 5 && e.Y <= FaceRectTransformed.Y + FaceRectTransformed.Height + 5)
                                        moveRectIndex = 3;
                                    else if (e.X >= FaceRectTransformed.X - 5 && e.X <= FaceRectTransformed.X + 5 && e.Y >= FaceRectTransformed.Y + FaceRectTransformed.Height - 5
                                             && e.Y <= FaceRectTransformed.Y + FaceRectTransformed.Height + 5)
                                        moveRectIndex = 4;

                                    #endregion

                                    if (moveRectIndex == -1) // если таскаем не прямоугольник, а точки
                                    {
                                        foreach (var item in ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.SelectedPoints)
                                            headTempPoints.Add(item.Clone() as MirroredHeadPoint);
                                    }
                                    else
                                    {
                                        var temp = FaceRectTransformed.X + FaceRectTransformed.Width * 0.5f;
                                        tempMoveRectCenter.X = (temp - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f);
                                        temp = FaceRectTransformed.Y + FaceRectTransformed.Height * 0.5f;
                                        tempMoveRectCenter.Y = (temp - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);

                                        tempMoveRectWidth = (FaceRectTransformed.Width) / (ImageTemplateWidth * 1f);
                                        tempMoveRectHeight = (FaceRectTransformed.Height) / (ImageTemplateHeight * 1f);
                                    }
                                }
                                break;
                            case Mode.HeadAutodotsLassoStart:
                                if (dblClick)
                                {
                                    ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HeadAutodotsLassoActive;
                                    headAutodotsLassoPoints.Add(headAutodotsLassoPoints.First());
                                }
                                break;
                            case Mode.HeadAutodotsLassoActive:
                                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HeadAutodotsLassoStart;
                                headAutodotsLassoPoints.Clear();
                                headAutodotsLassoPoints.Add(new Vector2(e.X, e.Y));
                                break;
                            case Mode.HeadShapedotsLassoStart:
                                if (dblClick)
                                {
                                    ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HeadShapedotsLassoActive;
                                    headShapedotsLassoPoints.Add(headShapedotsLassoPoints.First());
                                }
                                break;
                            case Mode.HeadShapedotsLassoActive:
                                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HeadShapedotsLassoStart;
                                headShapedotsLassoPoints.Clear();
                                headShapedotsLassoPoints.Add(new Vector2(e.X, e.Y));
                                break;
                            case Mode.HeadLine:
                                if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                    return;

                                if (ProgramCore.MainForm.HeadFront)
                                {
                                    #region вид спереди

                                    if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.None)
                                        return;

                                    if (dblClick)
                                        FinishLine();
                                    else
                                    {
                                        foreach (var item in ProgramCore.MainForm.ctrlRenderControl.headController.SelectedPoints)
                                            headTempPoints.Add(item.Clone() as MirroredHeadPoint);
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Вид сбоку

                                    if (dblClick)
                                        FinishLine();
                                    else
                                    {
                                        foreach (var item in ProgramCore.MainForm.ctrlRenderControl.headController.SelectedPoints)
                                            headTempPoints.Add(item.Clone() as MirroredHeadPoint);
                                    }

                                    #endregion
                                }
                                break;
                            case Mode.HeadShapedots:
                                {
                                    if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                        return;

                                    if (dblClick)
                                    {
                                        RectTransformMode = false;
                                        ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.ClearSelection();
                                    }

                                    #region Rectangle transforM ?

                                    moveRectIndex = -1;
                                    if (e.X >= FaceRectTransformed.X - 5 && e.X <= FaceRectTransformed.X + 5 && e.Y >= FaceRectTransformed.Y - 5 && e.Y <= FaceRectTransformed.Y + 5)
                                        moveRectIndex = 1;
                                    else if (e.X >= FaceRectTransformed.X + FaceRectTransformed.Width - 5 && e.X <= FaceRectTransformed.X + FaceRectTransformed.Width + 5
                                             && e.Y >= FaceRectTransformed.Y - 5 && e.Y <= FaceRectTransformed.Y + 5)
                                        moveRectIndex = 2;
                                    else if (e.X >= FaceRectTransformed.X + FaceRectTransformed.Width - 5 && e.X <= FaceRectTransformed.X + FaceRectTransformed.Width + 5
                                             && e.Y >= FaceRectTransformed.Y + FaceRectTransformed.Height - 5 && e.Y <= FaceRectTransformed.Y + FaceRectTransformed.Height + 5)
                                        moveRectIndex = 3;
                                    else if (e.X >= FaceRectTransformed.X - 5 && e.X <= FaceRectTransformed.X + 5 && e.Y >= FaceRectTransformed.Y + FaceRectTransformed.Height - 5
                                             && e.Y <= FaceRectTransformed.Y + FaceRectTransformed.Height + 5)
                                        moveRectIndex = 4;

                                    #endregion

                                    if (moveRectIndex == -1) // если таскаем не прямоугольник, а точки
                                    {
                                        foreach (var item in ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.SelectedPoints)
                                            headTempPoints.Add(item.Clone() as MirroredHeadPoint);
                                    }
                                    else
                                    {
                                        var temp = FaceRectTransformed.X + FaceRectTransformed.Width * 0.5f;
                                        tempMoveRectCenter.X = (temp - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f);
                                        temp = FaceRectTransformed.Y + FaceRectTransformed.Height * 0.5f;
                                        tempMoveRectCenter.Y = (temp - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);

                                        tempMoveRectWidth = (FaceRectTransformed.Width) / (ImageTemplateWidth * 1f);
                                        tempMoveRectHeight = (FaceRectTransformed.Height) / (ImageTemplateHeight * 1f);
                                    }
                                }
                                break;
                            case Mode.None:
                                {
                                    switch (ControlPointsMode)
                                    {
                                        case ProfileControlPointsMode.MoveControlPoints:
                                        case ProfileControlPointsMode.UpdateRightLeft:
                                            foreach (var item in profileControlPoints)
                                                headTempPoints.Add(item.Clone() as MirroredHeadPoint);
                                            break;
                                    }
                                }
                                break;
                        }

                        #endregion

                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                {
                    case Mode.HeadLine:
                        if (ProgramCore.Project.ShapeFlip != FlipType.None)
                            return;

                        if (ProgramCore.MainForm.HeadFront)
                        {
                            #region вид спереди

                            if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.None)
                                return;

                            ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());

                            #endregion
                        }
                        else
                        {
                            #region Вид сбоку

                            ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());

                            #endregion
                        }

                        break;
                }
            }
        }
        private void pictureTemplate_MouseMove(object sender, MouseEventArgs e)
        {
            if (startMousePoint == Vector2.Zero)
                startMousePoint = new Vector2(e.X, e.Y);

            var firstMove = false;
            if (Math.Abs(startMousePoint.X - e.X) > 1 || Math.Abs(startMousePoint.Y - e.Y) > 1) // small exp
            {
                if (!startMove)
                    firstMove = true;

                startMove = true;
            }

            if (leftMousePressed)
            {
                Vector2 newPoint;
                Vector2 delta2;
                newPoint.X = (e.X - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f);
                newPoint.Y = (e.Y - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);
                switch (ProgramCore.MainForm.ctrlRenderControl.ScaleMode)
                {
                    case ScaleMode.Move:
                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadShapedots:
                            case Mode.HeadLine: // эти моды только для этих режимов!
                            case Mode.HeadAutodots:
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodotsLassoStart:
                            case Mode.HeadAutodotsLassoActive:

                                newPoint = new Vector2(e.X, e.Y);
                                delta2 = newPoint - headLastPoint;
                                ImageTemplateOffsetX = (int)(tempOffsetPoint.X + delta2.X);
                                ImageTemplateOffsetY = (int)(tempOffsetPoint.Y + delta2.Y);
                                RecalcEyeMouthRect();
                                UpdateFaceRect();
                                break;
                        }
                        break;
                    case ScaleMode.Zoom:

                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadShapedots:
                            case Mode.HeadLine: // эти моды только для этих режимов!
                            case Mode.HeadAutodots:
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodotsLassoStart:
                            case Mode.HeadAutodotsLassoActive:
                                if (startMove)
                                {
                                    var s = imageScale + (headLastPoint.Y - e.Y) * 0.01f;
                                    if (s < 1.0f)
                                        imageScale = 1.0f;
                                    else if (s > 5.0f)
                                        imageScale = 5.0f;
                                    else
                                    {
                                        imageScale = s;
                                        var w = OriginalImageTemplateWidth * imageScale;
                                        var h = OriginalImageTemplateHeight * imageScale;
                                        ImageTemplateWidth = (int)w;
                                        ImageTemplateHeight = (int)h;
                                        var k = ImageTemplateWidth * 1f / ImageTemplateOldWidth;
                                        var cx = (int)(ImageTemplateCenterX * k);
                                        var cy = (int)(ImageTemplateCenterY * k);
                                        ImageTemplateOffsetX = pictureTemplate.Width / 2 - cx;
                                        ImageTemplateOffsetY = pictureTemplate.Height / 2 - cy;
                                        RefreshPictureBox();
                                    }
                                    headLastPoint = new Vector2(e.X, e.Y);
                                    RecalcEyeMouthRect();
                                    UpdateFaceRect();
                                }
                                break;
                        }
                        break;
                    case ScaleMode.None:

                        #region Если нет зума - обрабатываем обычные режимы

                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodots:
                                if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                    return;

                                if (firstMove && ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.SelectedPoints.Count > 0)
                                {
                                    var history = new HistoryHeadAutoDots(ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots);
                                    ProgramCore.MainForm.ctrlRenderControl.historyController.Add(history);
                                }

                                if (startMove)
                                {
                                    delta2 = newPoint - headLastPointRelative;
                                    if (moveRectIndex != -1)          //таскаем прямоугольничек
                                    {
                                        var deltaX = (int)(e.X - headLastPoint.X);
                                        var deltaY = (int)(e.Y - headLastPoint.Y);
                                        switch (moveRectIndex)
                                        {
                                            case 1:
                                                FaceRectTransformed.X += deltaX;
                                                FaceRectTransformed.Width -= deltaX;
                                                FaceRectTransformed.Y += deltaY;
                                                FaceRectTransformed.Height -= deltaY;
                                                break;
                                            case 2:
                                                FaceRectTransformed.Width += deltaX;
                                                FaceRectTransformed.Y += deltaY;
                                                FaceRectTransformed.Height -= deltaY;
                                                break;
                                            case 3:
                                                FaceRectTransformed.Width += deltaX;
                                                FaceRectTransformed.Height += deltaY;
                                                break;
                                            case 4:
                                                FaceRectTransformed.Width -= deltaX;
                                                FaceRectTransformed.X += deltaX;
                                                FaceRectTransformed.Height += deltaY;
                                                break;
                                        }
                                        headLastPoint = new Vector2(e.X, e.Y);

                                        Vector2 center;
                                        var temp = FaceRectTransformed.X + FaceRectTransformed.Width * 0.5f;
                                        center.X = ((temp - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f));
                                        temp = FaceRectTransformed.Y + FaceRectTransformed.Height * 0.5f;
                                        center.Y = (temp - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);

                                        var newWidth = (FaceRectTransformed.Width) / (ImageTemplateWidth * 1f);
                                        var newHeight = (FaceRectTransformed.Height) / (ImageTemplateHeight * 1f);
                                        var kx = newWidth / tempMoveRectWidth;
                                        var ky = newHeight / tempMoveRectHeight;
                                        foreach (var point in ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.SelectedPoints)
                                        {
                                            var p = point.ValueMirrored - tempMoveRectCenter;
                                            p.X *= kx;
                                            p.Y *= ky;
                                            point.ValueMirrored = p + center;
                                        }
                                        tempMoveRectCenter = center;
                                        tempMoveRectWidth = newWidth;
                                        tempMoveRectHeight = newHeight;
                                        UpdateUserCenterPositions(false, true);
                                    }
                                    else            // таскаем точки
                                    {
                                        var selectedPoints = ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.SelectedPoints;
                                        for (var i = 0; i < selectedPoints.Count; i++)
                                        {
                                            var headPoint = selectedPoints[i];
                                            headPoint.ValueMirrored = headTempPoints[i].ValueMirrored + delta2;

                                            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.HeadAutodots)
                                                headPoint.UpdateWorldPoint();
                                        }
                                        UpdateUserCenterPositions(true, true);
                                    }
                                }
                                break;
                            case Mode.HeadLine:
                                if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                    return;

                                if (LineSelectionMode)
                                {
                                    if (firstMove && ProgramCore.MainForm.ctrlRenderControl.headController.SelectedPoints.Count > 0)
                                    {
                                        var isProfile = ProgramCore.MainForm.HeadProfile;
                                        var teInfo = isProfile ? ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeProfileInfo : ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ShapeInfo;
                                        var historyElem = new HistoryHeadShapeLines(null, ProgramCore.MainForm.ctrlRenderControl.headController.Lines, teInfo, isProfile);
                                        historyElem.Group = ProgramCore.MainForm.ctrlRenderControl.historyController.currentGroup;
                                        ProgramCore.MainForm.ctrlRenderControl.historyController.Add(historyElem);
                                    }

                                    delta2 = newPoint - headLastPointRelative;
                                    for (var i = 0; i < ProgramCore.MainForm.ctrlRenderControl.headController.SelectedPoints.Count; i++)
                                    {
                                        var headPoint = ProgramCore.MainForm.ctrlRenderControl.headController.SelectedPoints[i];
                                        headPoint.ValueMirrored = headTempPoints[i].ValueMirrored + delta2;
                                        headPoint.UpdateWorldPoint();
                                    }
                                }
                                break;
                            case Mode.HeadShapedots:
                                if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                    return;

                                if (startMove)
                                {
                                    if (firstMove && ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.SelectedPoints.Count > 0)
                                    {
                                        Dictionary<Guid, MeshUndoInfo> undoInfo;
                                        ProgramCore.MainForm.ctrlRenderControl.headMeshesController.GetUndoInfo(out undoInfo);
                                        ProgramCore.MainForm.ctrlRenderControl.historyController.Add(new HistoryHeadShapeDots(undoInfo, ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots));
                                    }

                                    delta2 = newPoint - headLastPointRelative;
                                    if (moveRectIndex != -1)          //таскаем прямоугольничек
                                    {
                                        var deltaX = (int)(e.X - headLastPoint.X);
                                        var deltaY = (int)(e.Y - headLastPoint.Y);
                                        switch (moveRectIndex)
                                        {
                                            case 1:
                                                FaceRectTransformed.X += deltaX;
                                                FaceRectTransformed.Width -= deltaX;
                                                FaceRectTransformed.Y += deltaY;
                                                FaceRectTransformed.Height -= deltaY;
                                                break;
                                            case 2:
                                                FaceRectTransformed.Width += deltaX;
                                                FaceRectTransformed.Y += deltaY;
                                                FaceRectTransformed.Height -= deltaY;
                                                break;
                                            case 3:
                                                FaceRectTransformed.Width += deltaX;
                                                FaceRectTransformed.Height += deltaY;
                                                break;
                                            case 4:
                                                FaceRectTransformed.Width -= deltaX;
                                                FaceRectTransformed.X += deltaX;
                                                FaceRectTransformed.Height += deltaY;
                                                break;
                                        }
                                        headLastPoint = new Vector2(e.X, e.Y);

                                        Vector2 center;
                                        var temp = FaceRectTransformed.X + FaceRectTransformed.Width * 0.5f;
                                        center.X = ((temp - ImageTemplateOffsetX) / (ImageTemplateWidth * 1f));
                                        temp = FaceRectTransformed.Y + FaceRectTransformed.Height * 0.5f;
                                        center.Y = (temp - ImageTemplateOffsetY) / (ImageTemplateHeight * 1f);

                                        var newWidth = (FaceRectTransformed.Width) / (ImageTemplateWidth * 1f);
                                        var newHeight = (FaceRectTransformed.Height) / (ImageTemplateHeight * 1f);
                                        var kx = newWidth / tempMoveRectWidth;
                                        var ky = newHeight / tempMoveRectHeight;
                                        foreach (var point in ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.SelectedPoints)
                                        {
                                            var p = point.ValueMirrored - tempMoveRectCenter;
                                            p.X *= kx;
                                            p.Y *= ky;
                                            point.ValueMirrored = p + center;
                                            point.UpdateWorldPoint();
                                        }
                                        tempMoveRectCenter = center;
                                        tempMoveRectWidth = newWidth;
                                        tempMoveRectHeight = newHeight;
                                        UpdateUserCenterPositions(false, true);
                                    }
                                    else            // таскаем точки
                                    {
                                        var selectedPoints = ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.SelectedPoints;
                                        for (var i = 0; i < selectedPoints.Count; i++)
                                        {
                                            var headPoint = selectedPoints[i];
                                            headPoint.ValueMirrored = headTempPoints[i].ValueMirrored + delta2;
                                            headPoint.UpdateWorldPoint();
                                        }
                                        UpdateUserCenterPositions(true, true);
                                    }
                                }
                                break;
                            case Mode.None:
                                {
                                    switch (ControlPointsMode)
                                    {
                                        case ProfileControlPointsMode.MoveControlPoints:
                                        case ProfileControlPointsMode.UpdateRightLeft:
                                            {
                                                delta2 = newPoint - headLastPointRelative;
                                                for (var i = 0; i < profileControlPoints.Count; i++)
                                                {
                                                    var headPoint = profileControlPoints[i];
                                                    if (!headPoint.Selected)
                                                        continue;

                                                    headPoint.ValueMirrored = headTempPoints[i].ValueMirrored + delta2;
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                        }

                        #endregion

                        break;
                }

            }
        }
        private void pictureTemplate_MouseUp(object sender, MouseEventArgs e)
        {
            startMousePoint = Vector2.Zero;
            if (e.Button == MouseButtons.Left)
            {
                headLastPoint = Vector2.Zero;
                switch (ProgramCore.MainForm.ctrlRenderControl.ScaleMode)
                {
                    case ScaleMode.Zoom:
                        tempOffsetPoint = Vector2.Zero;
                        break;
                    case ScaleMode.None:

                        #region Обычные режимы

                        switch (ProgramCore.MainForm.ctrlRenderControl.Mode)
                        {
                            case Mode.HeadAutodotsFirstTime:
                            case Mode.HeadAutodots:
                                {
                                    if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                        return;

                                    if (!startMove && !dblClick)
                                    {
                                        if (!shiftKeyPressed)
                                            ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.ClearSelection();

                                        if (e.X >= MouthTransformed.X - 2.5 && e.X <= MouthTransformed.X + 2.5 && e.Y >= MouthTransformed.Y - 2.5 && e.Y <= MouthTransformed.Y + 2.5)       // рот
                                            ProgramCore.MainForm.ctrlRenderControl.headController.SelectAutdotsMouth();
                                        else if (e.X >= LeftEyeTransformed.X - 2.5 && e.X <= LeftEyeTransformed.X + 2.5 && e.Y >= LeftEyeTransformed.Y - 2.5 && e.Y <= LeftEyeTransformed.Y + 2.5)  // левый глаз
                                            ProgramCore.MainForm.ctrlRenderControl.headController.SelectAutodotsLeftEye();
                                        else if (e.X >= RightEyeTransformed.X - 2.5 && e.X <= RightEyeTransformed.X + 2.5 && e.Y >= RightEyeTransformed.Y - 2.5 && e.Y <= RightEyeTransformed.Y + 2.5)  // правый глаз
                                            ProgramCore.MainForm.ctrlRenderControl.headController.SelectAutodotsRightEye();
                                        else if (e.X >= NoseTransformed.X - 2.5 && e.X <= NoseTransformed.X + 2.5 && e.Y >= NoseTransformed.Y - 2.5 && e.Y <= NoseTransformed.Y + 2.5) // нос
                                            ProgramCore.MainForm.ctrlRenderControl.headController.SelectAutodotsNose();
                                        else if (e.X >= CentralFacePoint.X - 2.5 && e.X <= CentralFacePoint.X + 2.5 && e.Y >= CentralFacePoint.Y - 2.5 && e.Y <= CentralFacePoint.Y + 2.5) // прямоугольник и выделение всех точек
                                        {
                                            if (RectTransformMode)
                                            {
                                                RectTransformMode = false;
                                                ProgramCore.MainForm.ctrlRenderControl.headController.AutoDots.ClearSelection();
                                            }
                                            else
                                            {
                                                RectTransformMode = true;
                                                UpdateUserCenterPositions(true, true);

                                                ProgramCore.MainForm.ctrlRenderControl.headController.SelectAutodotsFaceEllipse();
                                            }
                                        }
                                        else
                                            ProgramCore.MainForm.ctrlRenderControl.headController.UpdateAutodotsPointSelection(e.X, e.Y);
                                    }
                                    else
                                    {
                                        RecalcEyeMouthRect();

                                        if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.HeadAutodots)
                                        {
                                            ProgramCore.MainForm.ctrlRenderControl.CalcReflectedBitmaps();
                                            ProgramCore.MainForm.ctrlRenderControl.headController.EndAutodots(false);
                                            ProgramCore.MainForm.ctrlRenderControl.ApplySmoothedTextures();
                                        }
                                    }
                                }
                                break;
                            case Mode.HeadAutodotsLassoStart:
                                headAutodotsLassoPoints.Add(new Vector2(e.X, e.Y));
                                break;
                            case Mode.HeadShapedotsLassoStart:
                                headShapedotsLassoPoints.Add(new Vector2(e.X, e.Y));
                                break;
                            case Mode.HeadLine:
                                {
                                    if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                        return;

                                    if (ProgramCore.MainForm.HeadFront)
                                    {
                                        #region вид спереди

                                        if (!startMove && !dblClick)
                                        {
                                            #region Проверяем, начали ли что-то обводить линиями

                                            var firstTime = false;
                                            if (e.X >= MouthTransformed.X - 2.5 && e.X <= MouthTransformed.X + 2.5 && e.Y >= MouthTransformed.Y - 2.5 && e.Y <= MouthTransformed.Y + 2.5) // рот
                                            {
                                                if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode != MeshPartType.Lip)
                                                {
                                                    firstTime = true;
                                                    ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = MeshPartType.Lip;
                                                }
                                            }
                                            else if (e.X >= LeftEyeTransformed.X - 2.5 && e.X <= LeftEyeTransformed.X + 2.5 && e.Y >= LeftEyeTransformed.Y - 2.5 && e.Y <= LeftEyeTransformed.Y + 2.5) // левый глаз
                                            {
                                                firstTime = true;
                                                ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = MeshPartType.LEye;
                                            }
                                            else if (e.X >= RightEyeTransformed.X - 2.5 && e.X <= RightEyeTransformed.X + 2.5 && e.Y >= RightEyeTransformed.Y - 2.5 && e.Y <= RightEyeTransformed.Y + 2.5) // правый глаз
                                            {
                                                firstTime = true;
                                                ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = MeshPartType.REye;
                                            }
                                            else if (e.X >= NoseTransformed.X - 2.5 && e.X <= NoseTransformed.X + 2.5 && e.Y >= NoseTransformed.Y - 2.5 && e.Y <= NoseTransformed.Y + 2.5) // нос
                                            {
                                                firstTime = true;
                                                ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = MeshPartType.Nose;
                                            }
                                            else if (e.X >= CentralFacePoint.X - 2.5 && e.X <= CentralFacePoint.X + 2.5 && e.Y >= CentralFacePoint.Y - 2.5 && e.Y <= CentralFacePoint.Y + 2.5)
                                            {
                                                firstTime = true;
                                                ProgramCore.MainForm.ctrlRenderControl.HeadLineMode = MeshPartType.Head;
                                            }

                                            #endregion

                                            if (firstTime)          // выбираем режим линии
                                            {
                                                ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Clear();
                                                ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.ResetPoints(ProgramCore.MainForm.ctrlRenderControl.HeadLineMode);
                                            }
                                            else if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode != MeshPartType.None)          // добавляем новые точки
                                            {
                                                var point = new MirroredHeadPoint(headLastPointRelative, headLastPointRelative, false);
                                                point.UpdateWorldPoint();

                                                #region Проверка на количество линий и режим выделения

                                                if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 1)
                                                {
                                                    var condition = false;
                                                    switch (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode)
                                                    {
                                                        case MeshPartType.Lip:
                                                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 2)
                                                                condition = true;
                                                            break;
                                                        default:
                                                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 1)

                                                                condition = true;
                                                            break;
                                                    }

                                                    if (condition) // если ничего не выделили - начинаем рисовать новую линию. иначе уходим в режим выделения и таскания точек
                                                    {
                                                        if (!shiftKeyPressed)
                                                            ProgramCore.MainForm.ctrlRenderControl.headController.ClearPointsSelection();

                                                        if (ProgramCore.MainForm.ctrlRenderControl.headController.UpdatePointSelection(point.Value.X, point.Value.Y))
                                                            LineSelectionMode = true;
                                                        else
                                                        {
                                                            if (LineSelectionMode)
                                                            {
                                                                LineSelectionMode = false;
                                                                ProgramCore.MainForm.ctrlRenderControl.headController.ClearPointsSelection();
                                                                break;
                                                            }
                                                            else
                                                                ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Clear();
                                                        }
                                                    }
                                                }

                                                #endregion

                                                if (!LineSelectionMode)
                                                {
                                                    #region Добавляем новые точки линии

                                                    if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 0)
                                                    {
                                                        var line = new HeadLine();
                                                        line.Add(point);
                                                        ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(line);
                                                    }
                                                    else
                                                    {
                                                        var currentLine = ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Last();
                                                        var hasIntersections = false;

                                                        if (currentLine.Count > 1) // проверка на пересечения линий
                                                        {
                                                            var lastPoint = currentLine.Last();

                                                            float ua, ub;
                                                            for (var i = currentLine.Count - 2; i >= 0; i--)
                                                            {
                                                                var pointA = currentLine[i];
                                                                var pointB = currentLine[i + 1];
                                                                if (AutodotsShapeHelper.GetUaUb(ref lastPoint.Value, ref point.Value, ref pointA.Value, ref pointB.Value, out ua, out ub))
                                                                {
                                                                    if (ua > 0 && ua < 1 && ub > 0 && ub < 1)
                                                                    {
                                                                        hasIntersections = true;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        var inAnotherPoint = false;
                                                        if (ProgramCore.MainForm.ctrlRenderControl.HeadLineMode == MeshPartType.Lip && ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 2)
                                                        {
                                                            // ЭТо вторая линия губ
                                                            foreach (var lPoint in ProgramCore.MainForm.ctrlRenderControl.headController.Lines.First())
                                                                if (point.Value.X >= lPoint.Value.X - 0.25 && point.Value.X <= lPoint.Value.X + 0.25 && point.Value.Y >= lPoint.Value.Y - 0.25 && point.Value.Y <= lPoint.Value.Y + 0.25 && !currentLine.Contains(lPoint))
                                                                {
                                                                    if (currentLine.Count == 0)
                                                                        currentLine.Add(lPoint);
                                                                    else
                                                                    {
                                                                        currentLine.Add(lPoint);
                                                                        ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());
                                                                    }
                                                                    inAnotherPoint = true;
                                                                    break;
                                                                }
                                                            if (currentLine.Count == 0) //первую точку добавляем всегда в пересечении с другой точкой.
                                                                inAnotherPoint = true;
                                                        }

                                                        // прочие случаи
                                                        if (!hasIntersections && !inAnotherPoint)
                                                        {
                                                            var firstPoint = currentLine.First();
                                                            if (point.Value.X >= firstPoint.Value.X - 0.25 && point.Value.X <= firstPoint.Value.X + 0.25 && point.Value.Y >= firstPoint.Value.Y - 0.25 && point.Value.Y <= firstPoint.Value.Y + 0.25)
                                                            {
                                                                currentLine.Add(firstPoint);
                                                                ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(new HeadLine());
                                                            }
                                                            else
                                                                currentLine.Add(point);
                                                        }
                                                    }

                                                    #endregion
                                                }
                                            }
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Вид сбоку

                                        if (!startMove && !dblClick)
                                        {
                                            var point = new MirroredHeadPoint(headLastPointRelative, headLastPointRelative, false);
                                            point.UpdateWorldPoint();

                                            #region Проверка на количество линий и режим выделения

                                            if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 1)
                                            {
                                                if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count > 1) // если ничего не выделили - начинаем рисовать новую линию. иначе уходим в режим выделения и таскания точек
                                                {
                                                    if (!shiftKeyPressed)
                                                        ProgramCore.MainForm.ctrlRenderControl.headController.ClearPointsSelection();

                                                    if (ProgramCore.MainForm.ctrlRenderControl.headController.UpdatePointSelection(point.Value.X, point.Value.Y))
                                                        LineSelectionMode = true;
                                                    else
                                                    {
                                                        if (LineSelectionMode)
                                                        {
                                                            LineSelectionMode = false;
                                                            ProgramCore.MainForm.ctrlRenderControl.headController.ClearPointsSelection();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion

                                            if (!LineSelectionMode)
                                            {
                                                #region Добавляем новые точки линии

                                                if (ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Count == 0)
                                                {
                                                    var line = new HeadLine();
                                                    line.Add(point);
                                                    ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Add(line);
                                                }
                                                else
                                                {
                                                    var currentLine = ProgramCore.MainForm.ctrlRenderControl.headController.Lines.Last();
                                                    var hasIntersections = false;

                                                    if (currentLine.Count > 1) // проверка на пересечения линий
                                                    {
                                                        var lastPoint = currentLine.Last();

                                                        float ua, ub;
                                                        for (var i = currentLine.Count - 2; i >= 0; i--)
                                                        {
                                                            var pointA = currentLine[i];
                                                            var pointB = currentLine[i + 1];
                                                            if (AutodotsShapeHelper.GetUaUb(ref lastPoint.Value, ref point.Value, ref pointA.Value, ref pointB.Value, out ua, out ub))
                                                            {
                                                                if (ua > 0 && ua < 1 && ub > 0 && ub < 1)
                                                                {
                                                                    hasIntersections = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    // прочие случаи
                                                    if (!hasIntersections)
                                                        currentLine.Add(point);
                                                }

                                                #endregion
                                            }
                                        }

                                        #endregion
                                    }

                                }
                                break;
                            case Mode.HeadShapedots:
                                if (ProgramCore.Project.ShapeFlip != FlipType.None)
                                    return;

                                if (!startMove && !dblClick)
                                {
                                    if (!shiftKeyPressed)
                                        ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.ClearSelection();

                                    if (e.X >= MouthTransformed.X - 2.5 && e.X <= MouthTransformed.X + 2.5 && e.Y >= MouthTransformed.Y - 2.5 && e.Y <= MouthTransformed.Y + 2.5) // рот
                                        ProgramCore.MainForm.ctrlRenderControl.headController.SelectShapedotsMouth();
                                    else if (e.X >= LeftEyeTransformed.X - 2.5 && e.X <= LeftEyeTransformed.X + 2.5 && e.Y >= LeftEyeTransformed.Y - 2.5 && e.Y <= LeftEyeTransformed.Y + 2.5) // левый глаз
                                        ProgramCore.MainForm.ctrlRenderControl.headController.SelectShapedotsLeftEye();
                                    else if (e.X >= RightEyeTransformed.X - 2.5 && e.X <= RightEyeTransformed.X + 2.5 && e.Y >= RightEyeTransformed.Y - 2.5 && e.Y <= RightEyeTransformed.Y + 2.5) // правый глаз
                                        ProgramCore.MainForm.ctrlRenderControl.headController.SelectShapedotsRightEye();
                                    else if (e.X >= NoseTransformed.X - 2.5 && e.X <= NoseTransformed.X + 2.5 && e.Y >= NoseTransformed.Y - 2.5 && e.Y <= NoseTransformed.Y + 2.5) // нос
                                        ProgramCore.MainForm.ctrlRenderControl.headController.SelectShapedotsNose();
                                    else if (e.X >= CentralFacePoint.X - 2.5 && e.X <= CentralFacePoint.X + 2.5 && e.Y >= CentralFacePoint.Y - 2.5 && e.Y <= CentralFacePoint.Y + 2.5) // прямоугольник и выделение всех точек
                                    {
                                        if (RectTransformMode)
                                        {
                                            RectTransformMode = false;
                                            ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.ClearSelection();
                                        }
                                        else
                                        {
                                            RectTransformMode = true;
                                            UpdateUserCenterPositions(true, true);

                                            ProgramCore.MainForm.ctrlRenderControl.headController.SelectShapedotsFaceEllipse();
                                        }
                                    }
                                    else
                                        ProgramCore.MainForm.ctrlRenderControl.headController.UpdateShapedotsPointSelection(e.X, e.Y);
                                }
                                else
                                {
                                    for (var i = 0; i < ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots.Count; i++)
                                    {
                                        var p = ProgramCore.MainForm.ctrlRenderControl.headController.ShapeDots[i];

                                        if (p.Selected)
                                            ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.Transform(p.Value, i); // точка в мировых координатах
                                    }
                                }
                                break;
                            case Mode.None:
                                {
                                    if (ProgramCore.MainForm.HeadProfile)
                                    {
                                        switch (ControlPointsMode)
                                        {
                                            case ProfileControlPointsMode.SetControlPoints:  // в профиле. расставляем опорные точки
                                                {
                                                    if (headLastPointRelative != Vector2.Zero)
                                                    {
                                                        profileControlPoints[profileControlPointIndex].ValueMirrored = headLastPointRelative;
                                                        ++profileControlPointIndex;

                                                        if (profileControlPointIndex == 4)
                                                        {
                                                            ControlPointsMode = ProfileControlPointsMode.MoveControlPoints;
                                                            profileControlPointIndex = 0;
                                                        }
                                                    }
                                                }
                                                break;
                                            case ProfileControlPointsMode.MoveControlPoints:  // выделяем и двигаем опорные точки
                                                {
                                                    if (!startMove && !dblClick)
                                                    {
                                                        if (!shiftKeyPressed)
                                                            foreach (var point in profileControlPoints)
                                                                point.Selected = false;

                                                        foreach (var point in profileControlPoints)
                                                        {
                                                            var pointK = new Vector2(point.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                                                     point.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                                                            if (e.X >= pointK.X - 5 && e.X <= pointK.X + 5 && e.Y >= pointK.Y - 5 && e.Y <= pointK.Y + 5)
                                                            {
                                                                point.Selected = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                            case ProfileControlPointsMode.UpdateRightLeft:  // выделяем и двигаем опорные точки
                                                {
                                                    if (!startMove && !dblClick)
                                                    {
                                                        if (!shiftKeyPressed)
                                                            foreach (var point in profileControlPoints)
                                                                point.Selected = false;

                                                        for (var i = 0; i < profileControlPoints.Count; i += 3)
                                                        {
                                                            var point = profileControlPoints[i];
                                                            var pointK = new Vector2(point.ValueMirrored.X * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateWidth + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetX,
                                                                                     point.ValueMirrored.Y * ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateHeight + ProgramCore.MainForm.ctrlTemplateImage.ImageTemplateOffsetY);
                                                            if (e.X >= pointK.X - 5 && e.X <= pointK.X + 5 && e.Y >= pointK.Y - 5 && e.Y <= pointK.Y + 5)
                                                            {
                                                                point.Selected = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else if (startMove)
                                                    {
                                                        UpdateProfileRectangle();
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                        }

                        #endregion

                        break;
                }
            }

            moveRectIndex = -1;

            startMove = false;
            leftMousePressed = false;
            dblClick = false;
            headLastPointRelative = Vector2.Zero;
            headTempPoints.Clear();
        }

        private void btnCopyProfileImg_MouseDown(object sender, MouseEventArgs e)
        {
            btnCopyProfileImg.Image = Properties.Resources.copyArrowPressed;
        }
        public void btnCopyProfileImg_MouseUp(object sender, MouseEventArgs e)
        {
            btnCopyProfileImg.Image = Properties.Resources.copyArrowNormal;

            var projectPath = Path.Combine(ProgramCore.Project.ProjectPath, "ProfileImage.jpg");
            using (var img = ProgramCore.MainForm.ctrlRenderControl.GrabScreenshot(string.Empty, ProgramCore.MainForm.ctrlRenderControl.ClientSize.Width, ProgramCore.MainForm.ctrlRenderControl.ClientSize.Height))
            {
                img.Save(projectPath);
                ProgramCore.Project.ProfileImage = new Bitmap(img);
            }
            SetTemplateImage(ProgramCore.Project.ProfileImage, false);

            #region Пересчитываем точки справа на лево

            var width = ProgramCore.MainForm.ctrlRenderControl.camera.WindowWidth * ProgramCore.MainForm.ctrlRenderControl.camera.Scale;
            var height = ProgramCore.MainForm.ctrlRenderControl.camera.WindowHeight * ProgramCore.MainForm.ctrlRenderControl.camera.Scale;

            var centerPosition = new Vector2(0, ProgramCore.MainForm.ctrlRenderControl.camera.Position.Y + ProgramCore.MainForm.ctrlRenderControl.camera.dy);
            var offsetX = centerPosition.X - width * 0.5f;
            var offsetY = centerPosition.Y - height * 0.5f;

            foreach (var point in profileControlPoints)
                point.ValueMirrored = new Vector2((point.Value.X - offsetX) / width, 1 - ((point.Value.Y - offsetY) / height));

            #endregion

        }

        #endregion
    }

    public enum ProfileControlPointsMode
    {
        SetControlPoints,  // режим выставления опорных точек в профиле
        MoveControlPoints,     // опорные точки выставлены, можем таскать их
        UpdateRightLeft,    // сопоставляем правую и левую картинки
        None
    }
}
