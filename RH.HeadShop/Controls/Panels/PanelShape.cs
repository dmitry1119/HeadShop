using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RH.HeadShop.Helpers;
using RH.HeadShop.Render;
using RH.HeadShop.Render.Controllers;
using RH.HeadShop.Render.Meshes;

namespace RH.HeadShop.Controls.Panels
{
    public partial class PanelShape : UserControlEx
    {
        #region Var

        public EventHandler OnSave;
        public EventHandler OnUndo;
        private int ShapeType()
        {
            if (btnShape.Tag.ToString() == "1")
                return 0;
            if (btnStretch.Tag.ToString() == "1")
                return 1;
            if (btnPleat.Tag.ToString() == "1")
                return 2;
            return -1;
        }

        #endregion

        public PanelShape()
        {
            InitializeComponent();
        }

        public void ResetModeTools()
        {
            if (btnMirror.Tag.ToString() == "1")
                btnMirror_Click(this, EventArgs.Empty);

            if (btnShape.Tag.ToString() == "1")
                btnShape_Click(this, EventArgs.Empty);
            if (btnStretch.Tag.ToString() == "1")
                btnStretch_Click(this, EventArgs.Empty);
            if (btnPleat.Tag.ToString() == "1")
                btnPleat_Click(this, EventArgs.Empty);
        }

        #region Form's event

        internal void btnMirror_Click(object sender, EventArgs e)
        {
            if (btnMirror.Tag.ToString() == "2")
            {
                btnMirror.Tag = "1";

                btnMirror.BackColor = SystemColors.ControlDarkDark;
                btnMirror.ForeColor = Color.White;

                ProgramCore.MainForm.ctrlRenderControl.ToolShapeMirrored = true;
            }
            else
            {
                btnMirror.Tag = "2";

                btnMirror.BackColor = SystemColors.Control;
                btnMirror.ForeColor = Color.Black;

                ProgramCore.MainForm.ctrlRenderControl.ToolShapeMirrored = false;
            }
        }

        internal void btnShape_Click(object sender, EventArgs e)
        {
            if (btnShape.Tag.ToString() == "2")
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.All(x => x.meshType != MeshType.Hair))
                {
                    MessageBox.Show("Select object!!!", "Notification", MessageBoxButtons.OK);
                    return;
                }

                ProgramCore.MainForm.ResetModeTools();

                btnShape.Tag = "1";
                btnShape.BackColor = SystemColors.ControlDarkDark;
                btnShape.ForeColor = Color.White;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HairShapeSetRect;

                btnStretch.Tag = btnPleat.Tag = "2";
                btnStretch.BackColor = btnPleat.BackColor = SystemColors.Control;
                btnStretch.ForeColor = btnPleat.ForeColor = Color.Black;
            }
            else
            {
                btnShape.Tag = "2";
                btnShape.BackColor = SystemColors.Control;
                btnShape.ForeColor = Color.Black;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.None;
            }
        }
        internal void btnStretch_Click(object sender, EventArgs e)
        {
            if (btnStretch.Tag.ToString() == "2")
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.All(x => x.meshType != MeshType.Hair))
                {
                    MessageBox.Show("Select object!!!", "Notification", MessageBoxButtons.OK);
                    return;
                }

                ProgramCore.MainForm.ResetModeTools();

                btnStretch.Tag = "1";
                btnStretch.BackColor = SystemColors.ControlDarkDark;
                btnStretch.ForeColor = Color.White;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HairStretchSetRect;

                btnShape.Tag = btnPleat.Tag = "2";
                btnShape.BackColor = btnPleat.BackColor = SystemColors.Control;
                btnShape.ForeColor = btnPleat.ForeColor = Color.Black;
            }
            else
            {
                btnStretch.Tag = "2";
                btnStretch.BackColor = SystemColors.Control;
                btnStretch.ForeColor = Color.Black;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.None;
            }
        }
        internal void btnPleat_Click(object sender, EventArgs e)
        {
            if (btnPleat.Tag.ToString() == "2")
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Count == 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.All(x => x.meshType != MeshType.Hair))
                {
                    MessageBox.Show("Select object!!!", "Notification", MessageBoxButtons.OK);
                    return;
                }

                ProgramCore.MainForm.ResetModeTools();

                btnPleat.Tag = "1";
                btnPleat.BackColor = SystemColors.ControlDarkDark;
                btnPleat.ForeColor = Color.White;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.HairPleatSetRect;

                btnStretch.Tag = btnShape.Tag = "2";
                btnStretch.BackColor = btnShape.BackColor = SystemColors.Control;
                btnStretch.ForeColor = btnShape.ForeColor = Color.Black;
            }
            else
            {
                btnPleat.Tag = "2";
                btnPleat.BackColor = SystemColors.Control;
                btnPleat.ForeColor = Color.Black;
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.None;
            }
        }

        private void trackBarShape_MouseDown(object sender, MouseEventArgs e)
        {
            var type = ShapeType();
            if (type < 0)
                return;
            var controller = ProgramCore.MainForm.ctrlRenderControl;
            if (controller.HairRect.Count > 0 &&
                controller.IsShapeChanged)
            {
                controller.IsShapeChanged = false;
                var p0 = controller.HairRect[0];
                var p1 = controller.HairRect[1];
                var p2 = controller.HairRect[2];
                var p3 = controller.HairRect[3];

                controller.shapeController.Initialize(p0, p1, p2, p3, controller.pickingController.SelectedMeshes, controller.camera, type);

                if (controller.ToolShapeMirrored)
                {
                    var halfWidth = controller.Width * 0.5f;
                    p0.X = halfWidth - (p0.X - halfWidth);
                    p1.X = halfWidth - (p1.X - halfWidth);
                    p2.X = halfWidth - (p2.X - halfWidth);
                    p3.X = halfWidth - (p3.X - halfWidth);
                    controller.shapeControllerMirror.Initialize(p0, p1, p2, p3, controller.pickingController.SelectedMeshes, controller.camera, type);
                }

                var history = new HistoryShape(controller.shapeController.MeshesInfo);
                controller.historyController.Add(history);
            }
        }
        private void trackBarShape_Scroll(object sender, EventArgs e)
        {
            var controller = ProgramCore.MainForm.ctrlRenderControl;
            if (controller.Mode == Mode.HairStretch || controller.Mode == Mode.HairShape || controller.Mode == Mode.HairPleat)
            {
                controller.shapeController.Transform(((trackBarShape.Value - trackBarShape.Minimum) * 1.0f) / (trackBarShape.Maximum - trackBarShape.Minimum) * 2.0f - 1.0f);
                controller.shapeController.FillDataForMesh();
                if (ProgramCore.MainForm.ctrlRenderControl.ToolShapeMirrored)
                {
                    controller.shapeControllerMirror.Transform(((trackBarShape.Value - trackBarShape.Minimum) * 1.0f) / (trackBarShape.Maximum - trackBarShape.Minimum) * 2.0f - 1.0f);
                    controller.shapeControllerMirror.FillDataForMesh();
                    controller.shapeController.Synchronize(controller.shapeControllerMirror);
                }
                foreach (var mesh in controller.pickingController.SelectedMeshes)
                {
                    var meshInfo = controller.shapeController.MeshesInfo[mesh.Id];
                    mesh.SetVertices(meshInfo.Vertices, meshInfo.Indices);
                }
            }
        }

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

        private void btnUndo_MouseDown(object sender, MouseEventArgs e)
        {
            btnUndo.BackColor = SystemColors.ControlDarkDark;
            btnUndo.ForeColor = Color.White;
        }
        private void btnUndo_MouseUp(object sender, MouseEventArgs e)
        {
            btnUndo.BackColor = SystemColors.Control;
            btnUndo.ForeColor = Color.Black;

            if (OnUndo != null)
                OnUndo(this, EventArgs.Empty);
        }

        #endregion
    }
}
