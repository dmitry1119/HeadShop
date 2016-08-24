using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenTK;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;
using RH.ImageListView;

namespace RH.HeadShop.Controls.Libraries
{
    /// <summary> Accessory library form </summary>
    public partial class frmAccessories : FormEx
    {
        private Matrix4 tempTransform;          // matrix transformation, require for changing angle and size of selected accessory

        /// <summary> Constructor </summary>
        public frmAccessories()
        {
            InitializeComponent();
            InitializeListView();       // Initialize accessory list

            Sizeble = false;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.OnSelectedMeshChanged += pickingController_OnSelectedMeshChanged;
        }

        void pickingController_OnSelectedMeshChanged()
        {
            BeginUpdate();
            try
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count != 1 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].meshType != MeshType.Accessory)
                {
                    ctrlAngle.Angle = 0;
                    teAngle.Text = "0";
                    trackBarSize.Value = 1;

                    imageListView.ClearSelection();
                }
                else
                {
                    trackBarSize.Value = (int)(ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].MeshSize * 10);

                    tempTransform = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform;

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
                var path = Path.Combine(UserConfig.DocumentsDir, "Accessory");
                var di = new DirectoryInfo(path);
                if (!di.Exists)
                    return;

                foreach (var p in di.GetFiles("*.jpg"))
                    imageListView.Items.Add(p.FullName);
            }
            finally
            {
                imageListView.ResumeLayout();
            }
        }
        private void InitTempTransform()
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count != 1)
                return;

            tempTransform = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform;           // for accessories always only ONE item can be selected
        }

        #endregion

        #region Form's event

        private void frmAccessories_Activated(object sender, EventArgs e)
        {
            ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(2);      // disable animations
        }
        private void frmAccessories_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count != 1 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].meshType != MeshType.Accessory)
                return;

            var size = trackBarSize.Value / 10f;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].MeshSize = size;

            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform = tempTransform;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 0] -= ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.X;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 1] -= ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.Y;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 2] -= ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.Z;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform *= Matrix4.CreateScale(size / meshScale);
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 0] += ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.X;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 1] += ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.Y;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Transform[3, 2] += ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position.Z;

            ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].IsChanged = true;
        }

        private float meshScale;
        private void trackBarSize_MouseDown(object sender, MouseEventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count != 1 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].meshType != MeshType.Accessory)
                return;

            InitTempTransform();
            meshScale = ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].MeshSize;
        }

        private void ctrlAngle_MouseDown(object sender, MouseEventArgs e)
        {
            InitTempTransform();
        }
        private void ctrlAngle_OnAngleChanged()
        {
            if (IsUpdating)
                return;

            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count != 1 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].meshType != MeshType.Accessory)
                return;

            BeginUpdate();
            try
            {
                teAngle.Text = ctrlAngle.Angle.ToString(CultureInfo.InvariantCulture);
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].MeshAngle = ctrlAngle.Angle;
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Rotate(ctrlAngle.Angle, tempTransform, false, ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position, ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].Position);
            }
            finally
            {
                EndUpdate();
            }
        }
        private void teAngle_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating)
                return;

            var value = StringConverter.ToInt(teAngle.Text, 0);
            ctrlAngle.Angle = value;
        }
        private void teAngle_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var textEdit = (TextBox)sender;
            var regex = new Regex("^(0?[0-9]?[0-9]|[1-2][0-9][0-9]|3[0-5][0-9]|360)$");
            errorProvider1.SetError(textEdit, !regex.IsMatch(textEdit.Text) ? "Please enter only number between (0; 360)" : "");
        }

        private void btnAddNewMaterial_Click(object sender, EventArgs e)
        {
            string accessoryPath;
            string sampleImagePath;
            using (var ofd = new OpenFileDialogEx("Select new accessory..", "OBJ files|*.obj"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                accessoryPath = ofd.FileName;
            }
            using (var ofd = new OpenFileDialogEx("Select accessory image..", "Image files|*.jpg"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                sampleImagePath = ofd.FileName;
            }

            var directoryPath = Path.Combine(UserConfig.DocumentsDir, "Accessory");
            var oldFileName = Path.GetFileNameWithoutExtension(accessoryPath);
            var newFileName = oldFileName;
            var filePath = Path.Combine(directoryPath, newFileName + ".obj");
            var index = 0;
            while (File.Exists(filePath))
            {
                newFileName = oldFileName + string.Format("_{0}", index);
                filePath = Path.Combine(directoryPath, newFileName + ".obj");
                ++index;
            }

            File.Copy(accessoryPath, filePath, false);

            var mtl = oldFileName + ".mtl";
            var newMtlName = newFileName + ".mtl";
            ObjLoader.CopyMtl(mtl, newMtlName, Path.GetDirectoryName(accessoryPath), "", directoryPath);

            if (mtl != newMtlName)      // situation, when copy attribute and can change mtl filename. so, we need to rename link to this mtl in main obj file
            {
                string lines;
                using (var sd = new StreamReader(filePath, Encoding.Default))
                {
                    lines = sd.ReadToEnd();
                    lines = lines.Replace(mtl, newMtlName);
                }
                using (var sw = new StreamWriter(filePath, false, Encoding.Default))
                    sw.Write(lines);
            }

            var samplePath = Path.Combine(directoryPath, newFileName + ".jpg");
            File.Copy(sampleImagePath, samplePath, false);
            InitializeListView();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (var sel in imageListView.SelectedItems)
            {
                var path = Path.Combine(sel.FilePath, sel.FileName);
                var mtlPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".mtl");
                var objPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".obj");

                var fi = new FileInfo(path);
                if (fi.Exists)
                {
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }

                var mtlFi = new FileInfo(mtlPath);
                if (mtlFi.Exists)
                {
                    mtlFi.Attributes = FileAttributes.Normal;
                    mtlFi.Delete();
                }

                var objFi = new FileInfo(objPath);
                if (objFi.Exists)
                {
                    objFi.Attributes = FileAttributes.Normal;
                    objFi.Delete();
                }
            }
            InitializeListView();
        }

        #endregion

    }
}

