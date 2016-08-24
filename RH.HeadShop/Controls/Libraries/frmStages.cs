using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Meshes;
using RH.ImageListView;

namespace RH.HeadShop.Controls.Libraries
{
    public partial class frmStages : FormEx
    {
        private string currentPose = string.Empty;

        public frmStages()
        {
            InitializeComponent();
            InitializeListView();

            Sizeble = false;
        }

        #region Supported void's

        public void InitializeListView()
        {
            #region Backgrounds

            imageListBackgrounds.AllowDuplicateFileNames = true;
            imageListBackgrounds.SetRenderer(new ImageListViewRenderers.DefaultRenderer());

            imageListBackgrounds.Columns.Add(ColumnType.Name);
            imageListBackgrounds.Columns.Add(ColumnType.FileSize);
            imageListBackgrounds.ThumbnailSize = new Size(96, 96);

            imageListBackgrounds.Items.Clear();
            imageListBackgrounds.SuspendLayout();
            try
            {
                var path = Path.Combine(Application.StartupPath, "Stages", "Backgrounds");
                var di = new DirectoryInfo(path);
                if (!di.Exists)
                    return;

                foreach (var p in di.GetFiles("*.jpg"))
                    imageListBackgrounds.Items.Add(p.FullName);
            }
            finally
            {
                imageListBackgrounds.ResumeLayout();
            }

            #endregion

            #region Poses

            currentPose = string.Empty;

            imageListPoses.AllowDuplicateFileNames = true;
            imageListPoses.SetRenderer(new ImageListViewRenderers.DefaultRenderer());

            imageListPoses.Columns.Add(ColumnType.Name);
            imageListPoses.Columns.Add(ColumnType.FileSize);
            imageListPoses.ThumbnailSize = new Size(96, 96);

            imageListPoses.Items.Clear();
            imageListPoses.SuspendLayout();
            try
            {
                var exts = new List<string> { ".jpg", ".png" };
                var path = Path.Combine(Application.StartupPath, "Stages", "Poses", ProgramCore.Project.ManType.GetCaption());
                var di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    foreach (var p in GetFilesByExtensions(di, exts))
                    {
                        var item = new ImageListViewItem(p.FullName)
                        {
                            Tag = PoseType.Pose
                        };
                        imageListPoses.Items.Add(item);
                    }
                }
            }
            finally
            {
                imageListPoses.ResumeLayout();
            }

            #endregion
        }
        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, List<string> extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private void SetTexture(string filePath)
        {
            var textureId = ProgramCore.MainForm.ctrlRenderControl.GetTexture(filePath);
            ProgramCore.MainForm.ctrlRenderControl.BackgroundTexture = textureId;
        }
        private void SetPose(ImageListViewItem sel)
        {
            var animFileName = Path.GetFileNameWithoutExtension(sel.Text) + ".obj";
            var animPath = Path.Combine(Application.StartupPath, "Stages", "Poses", ManType.Child.GetCaption(), animFileName);

            if (currentPose == animPath)
                return;

            currentPose = animPath;

            ProgramCore.MainForm.ctrlRenderControl.PoseMorphing = ProgramCore.MainForm.ctrlRenderControl.pickingController.LoadPartsMorphInfo(currentPose, ProgramCore.MainForm.ctrlRenderControl.headMeshesController.RenderMesh);

            trackBarPose.Enabled = true;
            trackBarPose.Value = 100;
            SetPosePosition();
        }
        private void SetPosePosition()
        {
            var delta = 1.0f - trackBarPose.Value / 100f;

            foreach (var m in ProgramCore.MainForm.ctrlRenderControl.PoseMorphing)
                m.Value.Delta = delta;

            ProgramCore.MainForm.ctrlRenderControl.DoMorth();
        }

        public void DoPhoto()
        {
            using (var sfd = new SaveFileDialogEx("Save screenshot", "Image file(*.jpg)|*.jpg"))
            {
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
                ProgramCore.MainForm.ctrlRenderControl.GrabScreenshot(sfd.FileName, ProgramCore.MainForm.ctrlRenderControl.ClientSize.Width, ProgramCore.MainForm.ctrlRenderControl.ClientSize.Height);
            }
        }

        #endregion

        #region Form's event

        private void trackBarPose_Scroll(object sender, EventArgs e)
        {
            SetPosePosition();
        }

        private void frmStages_Activated(object sender, EventArgs e)
        {
            ProgramCore.MainForm.ctrlRenderControl.StagesActivate(false);
        }
        private void frmStages_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void btnPhoto_Click(object sender, EventArgs e)
        {
            DoPhoto();
        }

        private void imageListBackgrounds_DoubleClick(object sender, EventArgs e)
        {
            if (imageListBackgrounds.SelectedItems.Count == 0)
                return;

            var sel = imageListBackgrounds.SelectedItems[0];
            SetTexture(sel.FileName);
        }
        private void imageListPoses_DoubleClick(object sender, EventArgs e)
        {
            if (imageListPoses.SelectedItems.Count == 0)
                return;

            var sel = imageListPoses.SelectedItems[0];
            SetPose(sel);
        }
        private void imageListPoses_SelectionChanged(object sender, EventArgs e)
        {
            if (IsUpdating)
                return;

            BeginUpdate();
            if (imageListPoses.SelectedItems.Count > 0)
                imageListBackgrounds.ClearSelection();
            EndUpdate();
        }
        private void imageListBackgrounds_SelectionChanged(object sender, EventArgs e)
        {
            if (IsUpdating)
                return;

            BeginUpdate();
            if (imageListBackgrounds.SelectedItems.Count > 0)
                imageListPoses.ClearSelection();
            EndUpdate();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            string elementPath;
            var sampleImagePath = string.Empty;
            var filterIndex = -1;
            using (var ofd = new OpenFileDialogEx("Select new item..", "Background|*.jpg|Pose|*.obj"))
            {
                ofd.Multiselect = false;
                if (ofd.ShowDialog(false) != DialogResult.OK)
                    return;
                elementPath = ofd.FileName;
                filterIndex = ofd.FilterIndex;

                if (ofd.FilterIndex != 1)           // if pose - require load screenshot to it
                {
                    using (var imOfd = new OpenFileDialogEx("Select item image..", "Image files|*.jpg"))
                    {
                        imOfd.Multiselect = false;
                        if (imOfd.ShowDialog() != DialogResult.OK)
                            return;
                        sampleImagePath = imOfd.FileName;
                    }
                }
            }

            var directoryPath = string.Empty;
            switch (filterIndex)
            {
                case 1:
                    directoryPath = Path.Combine(Application.StartupPath, "Stages", "Backgrounds");
                    break;
                case 2:
                    directoryPath = Path.Combine(Application.StartupPath, "Stages", "Poses", ProgramCore.Project.ManType.GetCaption());
                    break;
            }

            if (string.IsNullOrEmpty(directoryPath))
                return;

            var fileExtension = Path.GetExtension(elementPath);
            var oldFileName = Path.GetFileNameWithoutExtension(elementPath);
            var newFileName = oldFileName;
            var filePath = Path.Combine(directoryPath, newFileName + fileExtension);
            var index = 0;
            while (File.Exists(filePath))
            {
                newFileName = oldFileName + string.Format("_{0}", index);
                filePath = Path.Combine(directoryPath, newFileName + fileExtension);
                ++index;
            }

            File.Copy(elementPath, filePath, false);

            if (!string.IsNullOrEmpty(sampleImagePath))
            {
                var samplePath = Path.Combine(directoryPath, newFileName + ".jpg");
                File.Copy(sampleImagePath, samplePath, false);
            }

            switch (filterIndex)
            {
                case 1:
                    break;
                case 2:
                    ProgramCore.MainForm.ctrlRenderControl.animationController.AddPoses(filePath);
                    break;
            }

            InitializeListView();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (imageListBackgrounds.SelectedItems.Count > 0)              // remove selected background
            {
                foreach (var sel in imageListBackgrounds.SelectedItems)
                {
                    var path = Path.Combine(sel.FilePath, sel.FileName);
                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        fi.Attributes = FileAttributes.Normal;
                        fi.Delete();
                    }
                }
            }
            else
            {
                foreach (var sel in imageListPoses.SelectedItems)          // remove selected pose
                {
                    var path = Path.Combine(sel.FilePath, sel.FileName);
                    var daePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".obj");

                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        fi.Attributes = FileAttributes.Normal;
                        fi.Delete();
                    }

                    var daeFi = new FileInfo(daePath);
                    if (daeFi.Exists)
                    {
                        daeFi.Attributes = FileAttributes.Normal;
                        daeFi.Delete();
                    }
                }
            }

            InitializeListView();
        }

        #endregion

        #region For menu

        public void SetStage1()
        {
            if (imageListBackgrounds.Items.Count == 0)
                return;

            SetTexture(imageListBackgrounds.Items[0].FileName);
        }
        public void SetStage2()
        {
            if (imageListBackgrounds.Items.Count <= 1)
                return;

            SetTexture(imageListBackgrounds.Items[1].FileName);

        }
        public void SetStage3()
        {
            if (imageListBackgrounds.Items.Count <= 2)
                return;

            SetTexture(imageListBackgrounds.Items[2].FileName);
        }

        public void SetPose1()
        {
            if (imageListPoses.Items.Count == 0)
                return;
            SetPose(imageListPoses.Items[0]);
        }
        public void SetPose2()
        {
            if (imageListPoses.Items.Count <= 1)
                return;
            SetPose(imageListPoses.Items[1]);
        }
        public void SetPose3()
        {
            if (imageListPoses.Items.Count <= 2)
                return;
            SetPose(imageListPoses.Items[2]);
        }

        #endregion

    }
}
