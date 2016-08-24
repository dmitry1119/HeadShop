using System;
using System.Windows.Forms;

namespace RH.HeadShop.Helpers
{
    /// <summary> Extended form contains anti-flickering and update void's </summary>
    public class FormEx : Form
    {
        public bool Sizeble = true;

        public FormEx()
        {
            ResizeBegin += FormEx_ResizeBegin;
            ResizeEnd += FormEx_ResizeEnd;
        }

        #region Anti-flickering

        void FormEx_ResizeBegin(object sender, EventArgs e)
        {
            ToggleAntiFlicker(true);
        }
        void FormEx_ResizeEnd(object sender, EventArgs e)
        {
            ToggleAntiFlicker(false);
        }

        int originalExStyle = -1;
        bool bEnableAntiFlicker = true;
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                //   if (Environment.OSVersion.Version.Major < 6)
                {
                    if (originalExStyle == -1)
                        originalExStyle = base.CreateParams.ExStyle;
                    if (bEnableAntiFlicker)
                    {
                        cp.ExStyle |= 0x02000000; //WS_EX_COMPOSITED
                    }
                    else
                    {
                        cp.ExStyle = originalExStyle;
                    }
                }
                //  else
                //     cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void ToggleAntiFlicker(bool Enable)
        {
            bEnableAntiFlicker = Enable;
            //hacky, but works
            if (Sizeble)
                MaximizeBox = true;
        }

        #endregion

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
