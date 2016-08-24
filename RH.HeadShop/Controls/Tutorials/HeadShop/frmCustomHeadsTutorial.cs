using System.Diagnostics;
using System.Windows.Forms;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;

namespace RH.HeadShop.Controls.Tutorials.HeadShop
{
    public partial class frmCustomHeadsTutorial : FormEx
    {
        public frmCustomHeadsTutorial()
        {
            InitializeComponent();
        }

        private void frmCustomHeadsTutorial_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://youtu.be/H9dqNF4HdMQ");
        }

        private void cbShow_CheckedChanged(object sender, System.EventArgs e)
        {
            UserConfig.ByName("Options")["Tutorials", "CustomHeads"] = cbShow.Checked ? "0" : "1";
        }
    }
}
