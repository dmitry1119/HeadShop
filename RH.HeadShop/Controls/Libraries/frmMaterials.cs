using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenTK;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Meshes;
using RH.ImageListView;

namespace RH.HeadShop.Controls.Libraries
{
    public partial class frmMaterials : FormEx
    {
        public frmMaterials()
        {
            InitializeComponent();
            InitializeListView();

            Sizeble = false;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.OnSelectedMeshChanged += pickingController_OnSelectedMeshChanged;
        }

        void pickingController_OnSelectedMeshChanged()
        {
            BeginUpdate();
            try
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0)
                {
                    panelColor.BackColor = Color.Empty;
                    teAlpha.Text = "255";

                    ctrlAngle.Angle = 0;
                    teAngle.Text = "0";
                    trackBarSize.Value = 1;

                    imageListView.ClearSelection();

                    trackBarSize.Enabled = ctrlAngle.Enabled = teAngle.Enabled = teAlpha.Enabled = btnPickColor.Enabled = false;
                }
                else
                {
                    var firstColorForPanel = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor.Values.ElementAt(0);
                    var newColor = Color.FromArgb((int)(firstColorForPanel[3] * 255), (int)(firstColorForPanel[0] * 255),
                                                  (int)(firstColorForPanel[1] * 255), (int)(firstColorForPanel[2] * 255));
                    panelColor.BackColor = newColor;
                    teAlpha.Text = newColor.A.ToString(CultureInfo.InvariantCulture);

                    teAngle.Text = ((int)ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].TextureAngle).ToString(CultureInfo.InvariantCulture);
                    ctrlAngle.Angle = (int)ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].TextureAngle;
                    trackBarSize.Value = (int)(ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].TextureSize * 10);

                    if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Material.Texture > 0)
                    {
                        var texturePath = ProgramCore.MainForm.ctrlRenderControl.textures.FirstOrDefault(x => x.Value == ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Material.Texture).Key;
                        foreach (var item in imageListView.Items)
                            if (item.FileName == texturePath)
                            {
                                item.Selected = true;
                                break;
                            }
                    }

                    trackBarSize.Enabled = ctrlAngle.Enabled = teAngle.Enabled = teAlpha.Enabled = btnPickColor.Enabled = true;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        #region Supported void's

        private void InitializeListView()
        {
            imageListView.AllowDuplicateFileNames = true;
            imageListView.SetRenderer(new ImageListViewRenderers.DefaultRenderer());

            imageListView.Columns.Add(ColumnType.Name);
            imageListView.Columns.Add(ColumnType.FileSize);
            imageListView.ThumbnailSize = new Size(64, 64);

            imageListView.Items.Clear();
            imageListView.SuspendLayout();
            try
            {
                var path = Path.Combine(UserConfig.DocumentsDir, "Materials");
                var di = new DirectoryInfo(path);
                if (!di.Exists)
                    return;

                foreach (var p in di.GetFiles("*.jpg"))
                {
                    if (p.FullName.Contains("_alpha."))
                        continue;
                    imageListView.Items.Add(p.FullName);
                }
            }
            finally
            {
                imageListView.ResumeLayout();
            }
        }

        private void UpdateTexture()
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes == null)
                return;

            var radAngle = (float)(Math.PI * ctrlAngle.Angle / 180f);
            var size = trackBarSize.Value / 10f;

            foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes)
            {
                mesh.TextureSize = size;
                mesh.TextureAngle = ctrlAngle.Angle;
                mesh.UpdateTextureCoordinates(radAngle, size);
            }
        }

        #endregion

        #region Form's event

        private void frmMaterials_Activated(object sender, EventArgs e)
        {
            ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(1);
        }
        private void frmMaterials_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void btnPickColor_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor.Keys.Count; i++)
            {
                var key = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor.Keys.ElementAt(i);
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor[key] = new Vector4(panelColor.BackColor.R / 255f, panelColor.BackColor.G / 255f, panelColor.BackColor.B / 255f, panelColor.BackColor.A / 255f);
            }
        }
        private void panelColor_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.FullOpen = true;
                if (cd.ShowDialog() != DialogResult.OK)
                    return;
                panelColor.BackColor = cd.Color;
            }

        }
        private void imageListView_DoubleClick(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes == null || imageListView.SelectedItems.Count == 0)
                return;

            var sel = imageListView.SelectedItems[0];
            var transparentPath = Path.Combine(Path.GetDirectoryName(sel.FileName), Path.GetFileNameWithoutExtension(sel.FileName) + "_alpha" + Path.GetExtension(sel.FileName));
            transparentPath = File.Exists(transparentPath) ? transparentPath : string.Empty;

            foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes)
            {
                mesh.Material.DiffuseTextureMap = sel.FileName;
                mesh.Material.TransparentTextureMap = transparentPath;
            }
        }

        private void teAlpha_Validating(object sender, CancelEventArgs e)
        {
            var textEdit = (TextBox)sender;
            var regex = new Regex("^([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])$");
            errorProvider1.SetError(textEdit, !regex.IsMatch(textEdit.Text) ? "Please enter only number between (0; 255)" : "");
        }
        private void teAlpha_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating)
                return;

            for (var i = 0; i < ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor.Keys.Count;i++)
            {
                var key = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor.Keys.ElementAt(i);
                var color = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor[key];
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedColor[key] = new Vector4(color[0], color[1], color[2], Helpers.StringConverter.ToFloat(teAlpha.Text, 255) / 255f);
            }
        }

        private void ctrlAngle_AngleChanged()
        {
            if (IsUpdating)
                return;

            BeginUpdate();

            UpdateTexture();
            teAngle.Text = ctrlAngle.Angle.ToString(CultureInfo.InvariantCulture);

            EndUpdate();
        }
        private void teAngle_Validating(object sender, CancelEventArgs e)
        {
            var textEdit = (TextBox)sender;
            var regex = new Regex("^(0?[0-9]?[0-9]|[1-2][0-9][0-9]|3[0-5][0-9]|360)$");
            errorProvider1.SetError(textEdit, !regex.IsMatch(textEdit.Text) ? "Please enter only number between (0; 360)" : "");
        }
        private void teAngle_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating)
                return;

            var value = Helpers.StringConverter.ToInt(teAngle.Text, 0);
            ctrlAngle.Angle = value;
        }

        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            UpdateTexture();
        }

        private void btnAddNewMaterial_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialogEx("Select new material..", "Image files|*.jpg"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var directoryPath = Path.Combine(UserConfig.DocumentsDir, "Materials");
                var oldFileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                var newFileName = oldFileName;
                var filePath = Path.Combine(directoryPath, newFileName + ".jpg");
                var index = 0;
                while (File.Exists(filePath))
                {
                    newFileName = oldFileName + string.Format("_{0}", index);
                    filePath = Path.Combine(directoryPath, newFileName + ".jpg");
                    ++index;
                }

                File.Copy(ofd.FileName, filePath, false);
                InitializeListView();
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (var sel in imageListView.SelectedItems)
            {
                var path = Path.Combine(sel.FilePath, sel.FileName);
                var fi = new FileInfo(path);
                if (fi.Exists)
                {
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }
            }
            InitializeListView();
        }

        #endregion

    }
}
