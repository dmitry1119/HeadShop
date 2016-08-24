using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenTK;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Helpers;
using System.Drawing.Imaging;

namespace RH.HeadShop.Helpers
{
    public class FaceRecognition
    {
        public RectangleF FaceRectRelative;
        public Vector2 LeftEyeCenter;
        public Vector2 RightEyeCenter;
        public Vector2 MouthCenter;

        private int angleCount = 0;

        public void Recognize(ref string path, bool needCrop)
        {
            FaceRectRelative = RectangleF.Empty;
            LeftEyeCenter = RightEyeCenter = MouthCenter = Vector2.Zero;

            var executablePath = Path.GetDirectoryName(Application.ExecutablePath);
            var faceFileName = Path.Combine(executablePath, "Haar cascades", "haarcascade_frontalface_default.xml");
            var eyeFileName = Path.Combine(executablePath, "Haar cascades", "haarcascade_eye.xml");
            var mouthFileName = Path.Combine(executablePath, "Haar cascades", "haarcascade_mcs_mouth.xml");

            var image = new Image<Bgr, byte>(path);
            var faceRectangle = Rectangle.Empty;

            var mouthRectangle = Rectangle.Empty;

            var gray = image.Convert<Gray, Byte>(); //Convert it to Grayscale

            //normalizes brightness and increases contrast of the image
            gray._EqualizeHist();

            using (var face = new HaarCascade(faceFileName))
            using (var eye = new HaarCascade(eyeFileName))
            using (var mouth = new HaarCascade(mouthFileName))
            {

                //Detect the faces  from the gray scale image and store the locations as rectangle
                //The first dimensional is the channel
                //The second dimension is the index of the rectangle in the specific channel
                var facesDetected = gray.DetectHaarCascade(face, 1.1, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.FIND_BIGGEST_OBJECT, new Size(20, 20));
                if (facesDetected.Length == 0 || facesDetected[0].Length == 0)
                    faceRectangle = new Rectangle(0, 0, image.Width, image.Height);
                else
                {
                    faceRectangle = facesDetected[0][0].rect;

                    if (needCrop)       // если это создание проекта - то нужно обрезать фотку и оставить только голову
                    {
                        var newHeight = (int)(faceRectangle.Height * 0.5);
                        var newWidth = newHeight + faceRectangle.Height >= image.Height ? (int)(faceRectangle.Width * 0.2) : (int)(faceRectangle.Width * 0.3);   // если по условию - значит лицо крупно, и по ширине незачем широко оставлять
                        var newImageRect = faceRectangle;
                        newImageRect.Inflate(newWidth, newHeight);
                        if (newImageRect.Width < image.Width || newImageRect.Height < image.Height)         // если действительно лицо маленькое - делаем обрезание
                        {
                            newImageRect.X = newImageRect.X < 0 ? 0 : newImageRect.X;
                            newImageRect.Y = newImageRect.Y < 0 ? 0 : newImageRect.Y;
                            if (newImageRect.Width + newImageRect.X > image.Width)
                            {
                                var delta = (int)Math.Ceiling(((newImageRect.Width + newImageRect.X) - image.Width) * 0.5f);
                                if (newImageRect.X - delta < 0)
                                    newImageRect.Width -= delta * 2;
                                else
                                {
                                    newImageRect.Width -= delta;
                                    newImageRect.X -= delta;
                                }
                            }
                            if (newImageRect.Height + newImageRect.Y > image.Height)
                            {
                                var delta = (int)Math.Ceiling(((newImageRect.Height + newImageRect.Y) - image.Height) * 0.5f);
                                if (newImageRect.Y - delta < 0)
                                    newImageRect.Height -= delta * 2;
                                else
                                {
                                    newImageRect.Height -= delta;
                                    newImageRect.Y -= delta;
                                }
                            }

                            using (var croppedImage = ImageEx.Crop(path, newImageRect))
                            {
                                path = UserConfig.AppDataDir;
                                FolderEx.CreateDirectory(path);
                                path = Path.Combine(path, "tempHaarImage.jpg");
                                croppedImage.Save(path, ImageFormat.Jpeg);
                            }
                            Recognize(ref path, false);
                            return;
                        }
                    }
                }

                #region Рот

                //Set the region of interest on the faces
                gray.ROI = faceRectangle;
                var mouthDetected = gray.DetectHaarCascade(mouth, 1.1, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_ROUGH_SEARCH, new Size(20, 20));
                gray.ROI = Rectangle.Empty;

                if (mouthDetected.Length > 0 && mouthDetected[0].Length > 0)
                {
                    var sortedMouths = mouthDetected[0].OrderByDescending(x => x.rect.Y).ToList();
                    var hasBetterMouth = false;
                    if (sortedMouths.Count > 1)     // обрабатываем случай, когда два рта расположены близко, но нижний - неправильный.
                    {
                        var mouthRect1 = sortedMouths[0].rect;
                        var mouthRect2 = sortedMouths[1].rect;

                        if (Math.Abs(mouthRect1.Y - mouthRect2.Y) < 20)
                        {
                            var rectS1 = mouthRect1.Width * mouthRect1.Height;
                            var rectS2 = mouthRect2.Width * mouthRect2.Height;

                            if (rectS2 > rectS1)
                            {
                                hasBetterMouth = true;
                                mouthRectangle = mouthRect2;
                                mouthRectangle.Offset(faceRectangle.X, faceRectangle.Y);

                                var heightCoef = mouthRectangle.Height > 60 ? 0.28f : 0.4f;
                                //   var heightCoef = 1;
                                MouthCenter = new Vector2(mouthRectangle.X + mouthRectangle.Width * 0.5f, mouthRectangle.Y + mouthRectangle.Height * heightCoef);
                            }
                        }
                    }

                    if (!hasBetterMouth)
                    {
                        mouthRectangle = sortedMouths[0].rect;
                        mouthRectangle.Offset(faceRectangle.X, faceRectangle.Y);

                        var heightCoef = mouthRectangle.Height > 60 ? 0.28f : 0.4f;     // более пиздец точное распознавание -_-
                        //var heightCoef = 1;
                        MouthCenter = new Vector2(mouthRectangle.X + mouthRectangle.Width * 0.5f, mouthRectangle.Y + mouthRectangle.Height * heightCoef);
                    }

                    image.Draw(mouthRectangle, new Bgr(Color.Green), 2);
                }

                if (MouthCenter == Vector2.Zero)        // если не определилось - втыкаем по дефолту
                    MouthCenter = new Vector2(faceRectangle.Width * 0.5f, faceRectangle.Height / 1.5f);

                #endregion

                #region Глазки

                //Set the region of interest on the faces
                gray.ROI = faceRectangle;
                var eyesDetected = gray.DetectHaarCascade(eye, 1.1, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                gray.ROI = Rectangle.Empty;

                if (eyesDetected.Length > 0 && eyesDetected[0].Length > 0)
                {
                    if (eyesDetected[0].Length == 1)        // определился один глаз
                    {
                        var eyeRect = eyesDetected[0][0].rect;
                        eyeRect.Offset(faceRectangle.X, faceRectangle.Y);
                        var center = new Vector2(eyeRect.X + eyeRect.Width * 0.5f, eyeRect.Y + eyeRect.Height * 0.5f);

                        if (center.X < MouthCenter.X)           // определяем глаз по положению рта
                        {
                            LeftEyeCenter = center;
                        }
                        else
                        {
                            RightEyeCenter = center;
                        }
                    }
                    else            // определилось несколько глаз. выбираем нужные. развлекаемся
                    {
                        var sortedEyes = eyesDetected[0].OrderBy(x => x.rect.X).ToList();
                        var j1 = 0;
                        for (var j = 0; j < sortedEyes.Count - 1; j++)
                        {
                            var rf = sortedEyes[j].rect;
                            rf.Offset(faceRectangle.X, faceRectangle.Y);
                            var center = new Vector2(rf.X + rf.Width * 0.5f, rf.Y + rf.Height * 0.5f);
                            if (Math.Abs(center.Y - MouthCenter.Y) > 20)
                            {
                                LeftEyeCenter = center;
                                j1 = j;
                                break;
                            }
                        }

                        for (var i = sortedEyes.Count - 1; i > j1; i--)
                        {
                            var rf = sortedEyes[i].rect;
                            rf.Offset(faceRectangle.X, faceRectangle.Y);
                            var center = new Vector2(rf.X + rf.Width * 0.5f, rf.Y + rf.Height * 0.5f);

                            if (Math.Abs(center.Y - LeftEyeCenter.Y) < 65 && Math.Abs(center.X - LeftEyeCenter.X) > 20) // абсолютно от балды числа .что бы уж сильно явные выпады убрать
                            {
                                RightEyeCenter = center;
                                break;
                            }
                        }
                    }
                }

                #region Глазки не определились. Через три пизды колено определяем

                if (LeftEyeCenter == Vector2.Zero)
                {
                    if (RightEyeCenter != Vector2.Zero) // определяем через правый глаз и рот
                    {
                        var delta = Math.Abs(RightEyeCenter.X - MouthCenter.X);
                        LeftEyeCenter = new Vector2(MouthCenter.X - delta, RightEyeCenter.Y);
                    }
                    else                // примерно определяем через прямоугольник лица
                        LeftEyeCenter = new Vector2(faceRectangle.X + faceRectangle.Width / 3.5f, faceRectangle.Y + faceRectangle.Height / 3f);
                }

                if (RightEyeCenter == Vector2.Zero)
                {
                    if (LeftEyeCenter != Vector2.Zero) // определяем через левый глаз и рот
                    {
                        var delta = MouthCenter.X - LeftEyeCenter.X;
                        RightEyeCenter = new Vector2(MouthCenter.X + delta, LeftEyeCenter.Y);
                    }
                    else                // примерно определяем через прямоугольник лица
                        RightEyeCenter = new Vector2(faceRectangle.X + faceRectangle.Width / 3.5f, faceRectangle.Y + faceRectangle.Height / 3f);
                }

                #endregion

                #region Поворот фотки по глазам!

                var v = new Vector2(LeftEyeCenter.X - RightEyeCenter.X, LeftEyeCenter.Y - RightEyeCenter.Y);
                v.Normalize();      // ПД !
                var xVector = new Vector2(1, 0);

                float xDiff = xVector.X - v.X;
                float yDiff = xVector.Y - v.Y;
                var angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

                if (Math.Abs(angle) > 1 && angleCount <= 5)                // поворачиваем наклоненные головы
                {
                    ++angleCount;

                    using (var ms = new MemoryStream(File.ReadAllBytes(path))) // Don't use using!!
                    {
                        var originalImg = (Bitmap)Bitmap.FromStream(ms);

                        path = UserConfig.AppDataDir;
                        FolderEx.CreateDirectory(path);
                        path = Path.Combine(path, "tempHaarImage.jpg");

                        using (var ii = ImageEx.RotateImage(new Bitmap(originalImg), (float)-angle))
                            ii.Save(path, ImageFormat.Jpeg);
                    }

                    Recognize(ref path, false);
                    return;
                }

                #endregion

                #endregion
            }

            #region Переводим в относительные координаты

            MouthCenter = new Vector2(MouthCenter.X / (image.Width * 1f), MouthCenter.Y / (image.Height * 1f));
            LeftEyeCenter = new Vector2(LeftEyeCenter.X / (image.Width * 1f), LeftEyeCenter.Y / (image.Height * 1f));
            RightEyeCenter = new Vector2(RightEyeCenter.X / (image.Width * 1f), RightEyeCenter.Y / (image.Height * 1f));

            var leftTop = new Vector2(LeftEyeCenter.X, Math.Max(LeftEyeCenter.Y, RightEyeCenter.Y));
            var rightBottom = new Vector2(RightEyeCenter.X, MouthCenter.Y);

            FaceRectRelative = new RectangleF(leftTop.X, leftTop.Y, rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y);

            #endregion
        }
    }

}
