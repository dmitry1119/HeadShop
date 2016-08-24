using System;
using System.Collections.Generic;
using OpenTK;

namespace RH.HeadShop.Render.Helpers
{
    public class BezierCurve
    {
        public class BeizerCurveSegment
        {
            public Vector2 StartPoint
            {
                get;
                set;
            }
            public Vector2 EndPoint
            {
                get;
                set;
            }
            public Vector2 FirstControlPoint
            {
                get;
                set;
            }
            public Vector2 SecondControlPoint
            {
                get;
                set;
            }
        }

        public List<Vector2> Bezier2D(List<Vector2> points, int cpts)
        {
            var result = new List<Vector2>();
            var toRet = new List<BeizerCurveSegment>();
            for (var i = 0; i < points.Count - 1; i++)   //iterate for points but the last one
            {
                // Assume we need to calculate the control
                // points between (x1,y1) and (x2,y2).
                // Then x0,y0 - the previous vertex,
                //      x3,y3 - the next one.
                var x1 = points[i].X;
                var y1 = points[i].Y;

                var x2 = points[i + 1].X;
                var y2 = points[i + 1].Y;

                float x0;
                float y0;

                if (i == 0) //if is first point
                {
                    var previousPoint = points[i];  //if is the first point the previous one will be it self
                    x0 = previousPoint.X;
                    y0 = previousPoint.Y;
                }
                else
                {
                    x0 = points[i - 1].X;   //Previous Point
                    y0 = points[i - 1].Y;
                }

                float x3, y3;

                if (i == points.Count - 2)    //if is the last point possible (last but one)
                {
                    var nextPoint = points[i + 1];  //if is the last point the next point will be the last one
                    x3 = nextPoint.X;
                    y3 = nextPoint.Y;
                }
                else
                {
                    x3 = points[i + 2].X;   //Next Point
                    y3 = points[i + 2].Y;
                }

                var xc1 = (x0 + x1) / 2.0f;
                var yc1 = (y0 + y1) / 2.0f;
                var xc2 = (x1 + x2) / 2.0f;
                var yc2 = (y1 + y2) / 2.0f;
                var xc3 = (x2 + x3) / 2.0f;
                var yc3 = (y2 + y3) / 2.0f;

                var len1 = (float)Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
                var len2 = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                var len3 = (float)Math.Sqrt((x3 - x2) * (x3 - x2) + (y3 - y2) * (y3 - y2));

                var k1 = len1 / (len1 + len2);
                var k2 = len2 / (len2 + len3);

                var xm1 = xc1 + (xc2 - xc1) * k1;
                var ym1 = yc1 + (yc2 - yc1) * k1;

                var xm2 = xc2 + (xc3 - xc2) * k2;
                var ym2 = yc2 + (yc3 - yc2) * k2;

                const float smoothValue = 0.8f;
                // Resulting control points. Here smooth_value is mentioned
                // above coefficient K whose value should be in range [0...1].
                var ctrl1_x = xm1 + (xc2 - xm1) * smoothValue + x1 - xm1;
                var ctrl1_y = ym1 + (yc2 - ym1) * smoothValue + y1 - ym1;

                var ctrl2_x = xm2 + (xc2 - xm2) * smoothValue + x2 - xm2;
                var ctrl2_y = ym2 + (yc2 - ym2) * smoothValue + y2 - ym2;

                toRet.Add(new BeizerCurveSegment
                {
                    StartPoint = new Vector2(x1, y1),
                    EndPoint = new Vector2(x2, y2),
                    FirstControlPoint = i == 0 ? new Vector2(x1, y1) : new Vector2(ctrl1_x, ctrl1_y),
                    SecondControlPoint = i == points.Count - 2 ? new Vector2(x2, y2) : new Vector2(ctrl2_x, ctrl2_y)
                });
            }

            foreach (var segment in toRet)
            {
                var dt = 1.0f / cpts;
                var t = 0.0f;
                for (var i = 0; i <= cpts; i++)
                {                    
                    var cube = t * t * t;
                    var square = t * t;
                    var ax = 3 * (segment.FirstControlPoint.X - segment.StartPoint.X);
                    var ay = 3 * (segment.FirstControlPoint.Y - segment.StartPoint.Y);
                    var bx = 3 * (segment.SecondControlPoint.X - segment.FirstControlPoint.X) - ax;
                    var by = 3 * (segment.SecondControlPoint.Y - segment.FirstControlPoint.Y) - ay;
                    var cx = segment.EndPoint.X - segment.StartPoint.X - ax - bx;
                    var cy = segment.EndPoint.Y - segment.StartPoint.Y - ay - by;
                    var x = (cx * cube) + (bx * square) + (ax * t) + segment.StartPoint.X;
                    var y = (cy * cube) + (by * square) + (ay * t) + segment.StartPoint.Y;
                    result.Add(new Vector2(x, y));
                    t += dt;
                }
            }
            return result;           
        }
    }
}
