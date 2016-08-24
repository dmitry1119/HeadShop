using System.Linq;
using System.Windows.Forms;

namespace RH.HeadShop.Controls
{
    partial class frmControlBox : Form
    {
        public frmControlBox()
        {
            InitializeComponent();
        }

        private void frmControlBox_Shown(object sender, System.EventArgs e)
        {
            var control = Controls.Cast<Control>().FirstOrDefault(c => !(c is Button));
            if (control != null)
                control.Focus();
        }
    }
}