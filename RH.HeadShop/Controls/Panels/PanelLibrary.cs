using System;
using System.Drawing;
using System.Windows.Forms;
using RH.HeadShop.Helpers;

namespace RH.HeadShop.Controls.Panels
{
    public partial class PanelLibrary : UserControlEx
    {
        #region Var

        public EventHandler OnOpenLibrary;
        public EventHandler OnDelete;
        public EventHandler OnSave;
        public EventHandler OnExport;

        #endregion

        public PanelLibrary(bool needExport, bool needSaveDelete)
        {
            InitializeComponent();
            btnExport.Visible = needExport;
            btnSave.Visible = btnDelete.Visible = needSaveDelete;
        }

        #region Form's event

        private void btnSave_MouseDown(object sender, MouseEventArgs e)
        {
            btnSave.BackColor = SystemColors.ControlDarkDark;
            btnSave.ForeColor = Color.White;
        }
        private void btnSave_MouseUp(object sender, MouseEventArgs e)
        {
            btnSave.BackColor = SystemColors.Control;
            btnSave.ForeColor = Color.Black;

            if (OnSave != null)
                OnSave(this, EventArgs.Empty);
        }

        private void btnDelete_MouseDown(object sender, MouseEventArgs e)
        {
            btnDelete.BackColor = SystemColors.ControlDarkDark;
            btnDelete.ForeColor = Color.White;
        }
        private void btnDelete_MouseUp(object sender, MouseEventArgs e)
        {
            btnDelete.BackColor = SystemColors.Control;
            btnDelete.ForeColor = Color.Black;

            if (OnDelete != null)
                OnDelete(this, EventArgs.Empty);
        }

        private void btnExport_MouseDown(object sender, MouseEventArgs e)
        {
            btnExport.BackColor = SystemColors.ControlDarkDark;
            btnExport.ForeColor = Color.White;
        }
        private void btnExport_MouseUp(object sender, MouseEventArgs e)
        {
            btnExport.BackColor = SystemColors.Control;
            btnExport.ForeColor = Color.Black;

            if (OnExport != null)
                OnExport(this, EventArgs.Empty);
        }

        public void HideControl()
        {
            btnOpen.Tag = "2";
            btnOpen.BackColor = SystemColors.Control;
            btnOpen.ForeColor = Color.Black;
        }
        public void ShowControl()
        {
            btnOpen.Tag = "1";
            btnOpen.BackColor = SystemColors.ControlDarkDark;
            btnOpen.ForeColor = Color.White;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (btnOpen.Tag.ToString() == "2")
            {
                btnOpen.Tag = "1";
                btnOpen.BackColor = SystemColors.ControlDarkDark;
                btnOpen.ForeColor = Color.White;
                if (OnOpenLibrary != null)
                    OnOpenLibrary(this, EventArgs.Empty);
            }
            else
            {
                btnOpen.Tag = "2";
                btnOpen.BackColor = SystemColors.Control;
                btnOpen.ForeColor = Color.Black;
                if (OnOpenLibrary != null)
                    OnOpenLibrary(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
