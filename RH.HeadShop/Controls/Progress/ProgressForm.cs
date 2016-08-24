using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RH.HeadShop.Helpers;

namespace RH.HeadShop.Controls.Progress
{
    partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
            Visible = false;
        }

        public ProgressSettings ProgressSettings;

        #region ProgressProc

        private int percentDone
        {
            set
            {
                progressBar.Value = value;
            }
        }

        private string status
        {
            get
            {
                return statusLabel.Text;
            }
            set
            {
                statusLabel.Text = value;
            }
        }

        private int subPercentDone
        {
            set
            {
                subprogressBar.Value = value;
            }
        }

        private string subStatus
        {
            get
            {
                return substatusLabel.Text;
            }
            set
            {
                statusLabel.Text = value;
            }
        }

        private bool subprogressBar_Visible = true;
        private void UpdateFormStyle()
        {
            var delta = subprogressBar.Top - progressBar.Top;
            if (string.IsNullOrEmpty(subStatus) && subprogressBar_Visible)
            {
                subprogressBar_Visible = false;
                substatusLabel.Visible = false;
                subprogressBar.Visible = false;
                remainedTimeLabel.Top -= delta;
                Height -= delta;
            }
            if (!string.IsNullOrEmpty(subStatus) && !subprogressBar_Visible)
            {
                subprogressBar_Visible = true;
                substatusLabel.Visible = true;
                subprogressBar.Visible = true;
                remainedTimeLabel.Top += delta;
                Height += delta;
            }
        }

        private long startProgressDateTime;

        public void StartProgress()
        {
            var now = ProgressManager.Now;
            startProgressDateTime = now;
            status = "";
            percentDone = 0;
        }
        public void StopProgress()
        {
            ProgressSettings.PercentDone = 0;
            ProgressSettings.Status = "";
            ProgressSettings.SubPercentDone = 0;
            ProgressSettings.SubStatus = "";
            if (Visible)
                Visible = false;
        }
        public void WorkProgress()
        {
            DoProgress();
        }
        private TimeSpan previousTotalTime = TimeSpan.FromMilliseconds(0);
        private void DoProgress()
        {
            var now = ProgressManager.Now;

            if (status != ProgressSettings.Status)
            {
                status = ProgressSettings.Status;
            }

            var t = TimeSpan.FromMilliseconds(now - startProgressDateTime);
            if (t > TimeSpan.FromMilliseconds(1000))
            {
                if (!Visible)
                {
                    Show(ProgramCore.MainForm);
                    ProgressManager.ProgressHWnd = Handle;
                }
                if (Visible)
                {
                    try
                    {
                        status = ProgressSettings.Status;
                        percentDone = (int)ProgressSettings.PercentDone;
                        subPercentDone = (int)ProgressSettings.SubPercentDone;
                        subStatus = ProgressSettings.SubStatus;
                        if (ProgressSettings.PercentDone > 0)
                        {
                            var tt = new TimeSpan((long)(100 * t.Ticks / ProgressSettings.PercentDone)); //totalTime
                            if (previousTotalTime - TimeSpan.FromMilliseconds(5000) < tt && tt < previousTotalTime + TimeSpan.FromMilliseconds(5000))
                                tt = previousTotalTime;
                            previousTotalTime = tt;
                            var rt = tt - t; //remaingingTime
                            remainedTimeLabel.Text = @"Time elapsed: " +
                                                     (Math.Round(t.TotalDays) != 0 ? StringConverter.DaysToStr((int)Math.Round(t.TotalDays)) : "") +
                                                     (t.Hours != 0 ? StringConverter.HoursToStr(t.Hours) : "") +
                                                     (t.Minutes != 0 ? StringConverter.MinutesToStr(t.Minutes) : "") +
                                                     (t.Seconds != 0 || t.TotalSeconds < 1 ? t.Seconds.SecondsToStr() : "") +
                                                     ((rt <= TimeSpan.FromMilliseconds(0)) ? "" :
                                                         ".Remaining time: " +
                                                         (Math.Round(rt.TotalDays) != 0 ? StringConverter.DaysToStr((int)Math.Round(rt.TotalDays)) : "") +
                                                         (rt.Hours != 0 ? StringConverter.HoursToStr(rt.Hours) : "") +
                                                         (rt.Minutes != 0 ? StringConverter.MinutesToStr(rt.Minutes) : "") +
                                                         (rt.Seconds != 0 || rt.TotalSeconds < 1 ? rt.Seconds.SecondsToStr() : ""));
                        }
                        else
                            remainedTimeLabel.Text = "";
                    }
                    catch
                    {
                        remainedTimeLabel.Text = "";
                    }
                }

                if (Visible)
                    UpdateFormStyle();
            }
        }

        #endregion

    }
    class ProgressSettings
    {
        public string Status;
        public double PercentDone;
        public string SubStatus;
        public double SubPercentDone;

        public ProgressSettings()
        {
            Status = string.Empty;
            PercentDone = 0;
            SubStatus = string.Empty;
            SubPercentDone = 0;
        }
    }
    public static class ProgressManager
    {
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long value);
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long value);

        private static long freq;
        public static long Now;
        public static long GetNowTicks()
        {
            if (freq == 0)
                QueryPerformanceFrequency(out freq);
            long cntr;
            QueryPerformanceCounter(out cntr);
            return (long)(cntr * 1.0 / freq * 1000);
        }

        static ProgressSettings progressSettings;
        public static IntPtr ProgressHWnd = IntPtr.Zero;
        public static IntPtr ProgressCancelButtonHWnd = IntPtr.Zero;
        private static bool stackReleaseWaiting;
        private static ProgressForm frm;
        private static long lastUpdateTime;
        public static void ProgressProc(object sender, ProgressProcEventArgs e)
        {
            Now = GetNowTicks();
            if (Now - lastUpdateTime < 100)
                return;
            lastUpdateTime = Now;

            if (progressSettings == null)
            {
                progressSettings = new ProgressSettings();

                frm = new ProgressForm();
                var r = ProgramCore.MainForm.Bounds;
                frm.Location = new Point(r.Left + (r.Width - frm.Width) / 2, r.Top + (r.Height - frm.Height) / 2);
                frm.ProgressSettings = progressSettings;
            }
            progressSettings.Status = e.Status ?? progressSettings.Status;
            progressSettings.SubStatus = e.SubProgressStatus;
            progressSettings.PercentDone = progressSettings.PercentDone == -1 ? progressSettings.PercentDone : e.PercentDone;
            progressSettings.SubPercentDone = e.SubProgressPercentDone;

            if (!stackReleaseWaiting)
            {
                stackReleaseWaiting = true;
                frm.StartProgress();
                ProgramCore.AddCallStackReleasedProc(CallStackReleased);
            }
            else
            {
                frm.WorkProgress();
            }
        }

        private static void CallStackReleased(object sender, EventArgs e)
        {
            stackReleaseWaiting = false;
            frm.StopProgress();
        }
    }
}