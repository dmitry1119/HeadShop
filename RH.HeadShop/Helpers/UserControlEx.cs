using System.Windows.Forms;

namespace RH.HeadShop.Helpers
{
    /// <summary> Extended usercontrol. Contains methods for preventing flickering </summary>
    public class UserControlEx : UserControl
    {
        public UserControlEx()
        {
            DoubleBuffered = true;
            //  if (Environment.OSVersion.Version.Major < 6)
            {
                // Enable double duffering to stop flickering.
                SetStyle(ControlStyles.DoubleBuffer, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.SupportsTransparentBackColor, false);     // maybe false. 
                SetStyle(ControlStyles.Opaque, false);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                //  SetStyle(ControlStyles.ContainerControl, true);
            }
        }

        protected override CreateParams CreateParams            // prevent flickering
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        #region Update

        private byte updateDepth;
        public bool IsUpdating
        {
            get
            {
                return updateDepth > 0;
            }
        }
        public void BeginUpdate()
        {
            updateDepth++;
        }
        public void EndUpdate()
        {
            if (updateDepth > 0)
                updateDepth--;
        }

        #endregion
    }
}
