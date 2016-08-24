namespace RH.HeadShop.Controls.Progress
{
    public class ProgressProcEventArgs
    {
        public string Status { get; private set; }
        public double PercentDone { get; set; }
        public string SubProgressStatus { get; private set; }

        private readonly double subProgressPercentDone;
        public double SubProgressPercentDone
        {
            get
            {
                return string.IsNullOrEmpty(SubProgressStatus) ? PercentDone : subProgressPercentDone;
            }
        }
        public ProgressProcEventArgs(string status, double percentDone)
        {
            Status = status;
            PercentDone = percentDone;
            SubProgressStatus = null;
            subProgressPercentDone = 0;
        }
        public ProgressProcEventArgs(string status, double percentDone, string subProgressStatus, double subProgressPercentDone)
        {
            Status = status;
            PercentDone = percentDone;
            SubProgressStatus = subProgressStatus;
            this.subProgressPercentDone = subProgressPercentDone;
        }
    }
    public delegate void ProgressProcEventHandler(object sender, ProgressProcEventArgs e);
    public delegate void SimpleProgressProcHandler(string status, double percent);
}
