using System;
using System.Drawing;
using RH.HeadShop.Helpers;

namespace RH.HeadShop.Controls
{
    public partial class ctrlNewPart : UserControlEx
    {
        public string Title
        {
            get { return textTitle.Text; }
        }

        public ctrlNewPart()
        {
            InitializeComponent();

            var ti = "Item";
            var i = 0;
            while (ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(ti))
            {
                ti = "Item_" + i;
                ++i;
            }
            textTitle.Text = ti;
        }

        private void textTitle_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textTitle.Text) || ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(textTitle.Text))
            {
                sbOk.Enabled = false;
                textTitle.BackColor = Color.MistyRose;
            }
            else
            {
                sbOk.Enabled = true;
                textTitle.BackColor = Color.White;
            }
        }
    }
}
