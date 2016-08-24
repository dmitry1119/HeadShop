using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Meshes;

namespace RH.HeadShop.Controls.Libraries
{
    public partial class frmParts : FormEx
    {
        private Dictionary<DynamicRenderMesh, Tuple<Matrix4, float>> transforms = new Dictionary<DynamicRenderMesh, Tuple<Matrix4, float>>();   // matrix transformation, and meshSize require for changing angle and size of selected accessory

        public frmParts()
        {
            InitializeComponent();

            Sizeble = false;
            ProgramCore.MainForm.ctrlRenderControl.pickingController.OnSelectedMeshChanged += pickingController_OnSelectedMeshChanged;
        }

        void pickingController_OnSelectedMeshChanged()
        {
            BeginUpdate();
            try
            {
                transforms.Clear();
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0)
                    trackBarSize.Value = 1;
                else
                {
                    trackBarSize.Value = (int)(ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes[0].MeshSize * 10);

                    foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes)
                        transforms.Add(mesh, new Tuple<Matrix4, float>(mesh.Transform, mesh.MeshSize));
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public void UpdateList()
        {
            BeginUpdate();
            try
            {
                tlParts.Nodes.Clear();
                foreach (var element in ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes)
                {
                    var node = new TreeNode(element.Key)
                    {
                        Checked = element.Value[0].IsVisible,
                        Tag = element.Value
                    };
                    tlParts.Nodes.Add(node);
                }
                labelEmpty.Visible = tlParts.Nodes.Count == 0;
            }
            finally
            {
                EndUpdate();
            }
        }

        private void frmParts_Activated(object sender, EventArgs e)
        {
            ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(0);      // disable animation
        }
        private void frmParts_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;            // this cancels the close event.
        }

        private void tlParts_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (IsUpdating)
                return;

            var meshes = e.Node.Tag as DynamicRenderMeshes;
            if (meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    mesh.IsVisible = e.Node.Checked;

                    if (!mesh.IsVisible && ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Contains(mesh))
                        ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Remove(mesh);
                }
            }
        }
        private void tlParts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (IsUpdating)
                return;

            var meshes = e.Node.Tag as DynamicRenderMeshes;
            if (meshes != null && meshes.Count > 0 && meshes[0].IsVisible)
            {
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Clear();
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.AddRange(meshes);
            }
        }

        private void InitTempTransform()
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0)
                return;

            transforms.Clear();
            foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes)
                transforms.Add(mesh, new Tuple<Matrix4, float>(mesh.Transform, mesh.MeshSize));
        }
        private void trackBarSize_MouseDown(object sender, MouseEventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes == null)
                return;

            InitTempTransform();
        }
        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes == null)
                return;

            var size = trackBarSize.Value / 10f;
            foreach (var mesh in ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes)
            {
                mesh.MeshSize = size;

                mesh.Transform = transforms[mesh].Item1;
                mesh.Transform[3, 0] -= mesh.Position.X;
                mesh.Transform[3, 1] -= mesh.Position.Y;
                mesh.Transform[3, 2] -= mesh.Position.Z;
                mesh.Transform *= Matrix4.CreateScale(size / transforms[mesh].Item2);
                mesh.Transform[3, 0] += mesh.Position.X;
                mesh.Transform[3, 1] += mesh.Position.Y;
                mesh.Transform[3, 2] += mesh.Position.Z;

                mesh.IsChanged = true;
            }
        }

        private void btnLoadLibrary_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderDialogEx())
            {
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var dir = new DirectoryInfo(ofd.SelectedFolder[0]);
                foreach (var file in dir.GetFiles("*.obj", SearchOption.AllDirectories))
                {
                    var meshType = MeshType.Hair;
                    using (var sr = new StreamReader(file.FullName, Encoding.Default))
                    {
                        while (!sr.EndOfStream)
                        {
                            var currentLine = sr.ReadLine();
                            if (String.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
                            {
                                if (currentLine == "#Accessories")
                                {
                                    meshType = MeshType.Accessory;
                                    break;
                                }
                            }
                        }
                    }

                    var title = Path.GetFileNameWithoutExtension(file.Name);
                    var meshes = ProgramCore.MainForm.ctrlRenderControl.pickingController.AddMehes(file.FullName, meshType, false, ProgramCore.Project.ManType, false);
                    for (var i = 0; i < meshes.Count; i++)
                    {
                        var mesh = meshes[i];
                        if (mesh.vertexArray.Length == 0)
                            continue;
                        mesh.Title = title + "_" + i;
                        mesh.IsChanged = true;

                        if (!ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.ContainsKey(title))
                            ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.Add(title, new DynamicRenderMeshes());

                        ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes[title].Add(mesh);
                    }
                }

                UpdateList();
            }
        }
    }
}
