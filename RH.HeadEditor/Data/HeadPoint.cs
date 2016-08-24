using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;

namespace RH.HeadEditor.Data
{
    /// <summary> Точка, используемая для трансформации головы </summary>
    public class HeadPoint
    {
        public Vector2 Value;
        public bool Visible = true;
        public bool Selected;

        /// <summary> Связанные точки, которые должны двигаться автоматически, вместе с этой </summary>
        public List<int> LinkedPoints = new List<int>();

        public HeadPoint(Vector2 value)
        {
            Value = value;
        }
        public HeadPoint(float x, float y)
        {
            Value = new Vector2(x, y);
        }

        /// <summary> Проверить входит ли точка в лассо выделение </summary>
        /// <param name="lassoPoints"></param>
        public virtual void CheckLassoSelection(List<Vector2> lassoPoints)
        {
            if (!Visible)
                return;

            Selected = false;
            var count = 0;
            for (var i = 0; i < lassoPoints.Count; i++)
            {
                var j = (i + 1) % lassoPoints.Count;
                var p0 = lassoPoints[i];
                var p1 = lassoPoints[j];

                if (p0.Y == lassoPoints[j].Y)
                    continue;
                if (p0.Y > Value.Y && p1.Y > Value.Y)
                    continue;
                if (p0.Y < Value.Y && p1.Y < Value.Y)
                    continue;
                if (Math.Max(p0.Y, p1.Y) == Value.Y)
                    count++;
                else
                {
                    if (Math.Min(p0.Y, p1.Y) == Value.Y)
                        continue;

                    var t = (Value.Y - p0.Y) / (p1.Y - p0.Y);
                    if (p0.X + t * (p1.X - p0.X) >= Value.X)
                        count++;
                }
            }
            if (count % 2 == 1)
                Selected = true;
        }

        public virtual HeadPoint Clone()
        {
            var newPoint = new HeadPoint(Value)
                               {
                                   Visible = Visible,
                                   Selected = Selected
                               };
            newPoint.LinkedPoints.AddRange(LinkedPoints);
            return newPoint;
        }

        public void ToStream(BinaryWriter bw)
        {
            bw.Write(Value.X);
            bw.Write(Value.Y);

            bw.Write(Visible);

            bw.Write(LinkedPoints.Count);
            foreach (var lp in LinkedPoints)
                bw.Write(lp);
        }
        public static HeadPoint FromStream(BinaryReader br)
        {
            var result = new HeadPoint(br.ReadSingle(), br.ReadSingle());
            result.Visible = br.ReadBoolean();

            var cnt = br.ReadInt32();
            for (var i = 0; i < cnt; i++)
                result.LinkedPoints.Add(br.ReadInt32());

            return result;
        }
    }

    public class HeadPoints<T> : List<T>
        where T : HeadPoint
    {
        public List<T> SelectedPoints
        {
            get
            {
                return this.Where(point => point.Selected).ToList();
            }
        }

        public void SelectPoints(List<int> indexes)
        {
            foreach (var index in indexes)
                this[index].Selected = true;
        }
        public void SelectAll()
        {
            foreach (var point in this)
                point.Selected = true;
        }
        /// <summary> Снять выделение</summary>
        public void ClearSelection()
        {
            foreach (var point in this)
                point.Selected = false;
        }

        public void UpdatePointSelection(Vector2 mousePoint, float radius = 0.1f)
        {
            foreach (var point in this)
            {
                if (mousePoint.X >= point.Value.X - radius && mousePoint.X <= point.Value.X + radius && mousePoint.Y >= point.Value.Y - radius && mousePoint.Y <= point.Value.Y + radius)
                {
                    point.Selected = true;
                    break;
                }
            }
        }

    }
}
