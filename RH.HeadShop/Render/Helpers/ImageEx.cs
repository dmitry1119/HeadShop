using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RH.HeadShop.Render.Helpers
{
    public static class ImageEx
    {
        /// <summary> Обрезать изображение </summary>
        public static Bitmap Crop(Bitmap img, Rectangle cropArea)
        {
            return img.Clone(cropArea, img.PixelFormat);
        }
        public static Bitmap Crop(Image img, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(img);
            return Crop(bmpImage, cropArea);
        }
        public static Bitmap Crop(string imgPath, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(imgPath);
            return Crop(bmpImage, cropArea);
        }

        /// <summary> Вставить изображение в определенную область </summary>
        public static void CopyRegionIntoImage(Bitmap srcBitmap, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (var grD = Graphics.FromImage(destBitmap))
            {
                var srcRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
                grD.DrawImage(srcBitmap, destRegion, srcRect, GraphicsUnit.Pixel);
            }
        }

        /// <summary> Поворот изображения на произвольный угол </summary>
        public static Bitmap RotateImage(Bitmap img, float angle)
        {
            var rotatedImage = new Bitmap(img.Width, img.Height, img.PixelFormat);
            using (var g = Graphics.FromImage(rotatedImage))
            {
                g.Clear(Color.White);
                g.TranslateTransform(img.Width * 0.5f, img.Height * 0.5f); //set the rotation point as the center into the matrix
                g.RotateTransform(angle); //rotate
                g.TranslateTransform(-img.Width * 0.5f, -img.Height * 0.5f); //restore rotation point into the matrix
                g.DrawImage(img, new Point(0, 0)); //draw the image on the new bitmap
            }

            return rotatedImage;
        }

        public static Image ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = (size.Width / (float)sourceWidth);
            nPercentH = (size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }
    }
}
