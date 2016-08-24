using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;
using RH.ImageListView;

namespace RH.HeadShop.Controls.Libraries
{
    /// <summary> Style library form </summary>
    public partial class frmStyles : FormEx
    {
        /// <summary> Constructor </summary>
        public frmStyles()
        {
            InitializeComponent();
            InitializeListView();       // Initialize style list

            Sizeble = false;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.OnSelectedMeshChanged += pickingController_OnSelectedMeshChanged;
        }

        void pickingController_OnSelectedMeshChanged()
        {
            BeginUpdate();
            try
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.All(x => x.meshType != MeshType.Hair))
                {
                    imageListView.ClearSelection();
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
                var path = Path.Combine(UserConfig.DocumentsDir, "Style");
                var di = new DirectoryInfo(path);
                if (!di.Exists)
                    return;

                foreach (var p in di.GetFiles("*.jpg"))
                {
                    if (UserConfig.ByName("Options")["Styles", p.FullName, "1"] == "1")
                        imageListView.Items.Add(p.FullName);
                }
            }
            finally
            {
                imageListView.ResumeLayout();
            }
        }

        #endregion

        #region Form's event

        private void frmStyles_Activated(object sender, EventArgs e)
        {
            if (Visible)
                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(3);      // disable animations
        }
        private void frmStyles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void btnAddNewMaterial_Click(object sender, EventArgs e)
        {
            string hairPath;
            string sampleImagePath;
            using (var ofd = new OpenFileDialogEx("Select new style..", "OBJ files|*.obj"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                hairPath = ofd.FileName;
            }
            using (var ofd = new OpenFileDialogEx("Select style image..", "Image files|*.jpg"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                sampleImagePath = ofd.FileName;
            }

            var directoryPath = Path.Combine(UserConfig.DocumentsDir, "Style");
            var oldFileName = Path.GetFileNameWithoutExtension(hairPath);
            var newFileName = oldFileName;
            var filePath = Path.Combine(directoryPath, newFileName + ".obj");

            if (hairPath != filePath)
                File.Copy(hairPath, filePath, true);
           
            var mtl = oldFileName + ".mtl";
            var newMtlName = newFileName + ".mtl";
            if (mtl != newMtlName)
                ObjLoader.CopyMtl(mtl, newMtlName, Path.GetDirectoryName(hairPath), "", directoryPath);

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
            if (sampleImagePath != samplePath)
                File.Copy(sampleImagePath, samplePath, true);
            UserConfig.ByName("Options")["Styles", samplePath] = "1";

            InitializeListView();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (var sel in imageListView.SelectedItems)
            {
                var path = Path.Combine(sel.FilePath, sel.FileName);
                UserConfig.ByName("Options")["Styles", path] = "0";     // not delete, just hide it.

                /*     var mtlPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".mtl");
                     var objPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".obj");
                     var objNullPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_null.obj");

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

                     var objNullFi = new FileInfo(objNullPath);
                     if (objNullFi.Exists)
                     {
                         objNullFi.Attributes = FileAttributes.Normal;
                         objNullFi.Delete();
                     }*/
            }
            InitializeListView();
        }

        #endregion

        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            var k = (trackBarSize.Value - trackBarSize.Minimum) * 1f / (trackBarSize.Maximum - trackBarSize.Minimum);
            foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes)
                mesh.InterpolateMesh((trackBarSize.Value - trackBarSize.Minimum) * 1f / (trackBarSize.Maximum - trackBarSize.Minimum));
        }

    }
}

