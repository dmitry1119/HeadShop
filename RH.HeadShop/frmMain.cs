using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RH.HeadShop.Controls;
using RH.HeadShop.Controls.Libraries;
using RH.HeadShop.Controls.Panels;
using RH.HeadShop.Controls.Progress;
using RH.HeadShop.Controls.Tutorials;
using RH.HeadShop.Controls.Tutorials.HairShop;
using RH.HeadShop.Controls.Tutorials.HeadShop;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Meshes;
using RH.HeadShop.Render.Obj;

namespace RH.HeadShop
{
    public partial class frmMain : Form
    {
        #region Var

        private readonly MRUManager mruManager = new MRUManager();

        public frmMaterials frmMaterial;
        public frmAccessories frmAccessories;
        public frmStages frmStages;
        public frmParts frmParts;
        public frmStyles frmStyles;
        public frmFreeHand frmFreeHand;

        private PanelCut panelCut;
        private PanelShape panelShape;
        private PanelLibrary panelAccessories;
        private PanelLibrary panelMaterials;
        private PanelLibrary panelStages;
        private PanelLibrary panelStyles;
        public PanelHead panelFront
        {
            get;
            private set;
        }
        public PanelHead panelProfile
        {
            get;
            private set;
        }
        private PanelFeatures panelFeatures;

        private readonly frmStartTutorial frmTutStart = new frmStartTutorial();
        public readonly frmRecognizeTutorial frmTutRecognize = new frmRecognizeTutorial();

        private readonly frmAccessoryTutorial frmTutAccessory = new frmAccessoryTutorial();
        private readonly frmCutTutorial frmTutCut = new frmCutTutorial();
        private readonly frmMaterialTutorial frmTutMaterial = new frmMaterialTutorial();
        private readonly frmShapeTutorial frmTutShape = new frmShapeTutorial();
        private readonly frmStageTutorial frmTutStage = new frmStageTutorial();
        private readonly frmStyleTutorial frmTutStyle = new frmStyleTutorial();

        public readonly frmCustomHeadsTutorial frmTutCustomHeads = new frmCustomHeadsTutorial();
        public readonly frmAutodotsTutorial frmTutAutodots = new frmAutodotsTutorial();
        public readonly frmShapedotsTutorial frmTutShapedots = new frmShapedotsTutorial();
        public readonly frmMirrorTutorial frmTutMirror = new frmMirrorTutorial();
        public readonly frmFreehandTutorial frmTutFreehand = new frmFreehandTutorial();
        private readonly frmProfileTutorial frmTutProfile = new frmProfileTutorial();

        /// <summary> Флаг, означающий что мы находимся во вкладкам изменения ибала. </summary>
        public bool HeadMode
        {
            get;
            private set;
        }

        public bool HeadFront
        {
            get;
            private set;
        }
        public bool HeadProfile
        {
            get;
            private set;
        }
        public bool HeadFeature
        {
            get;
            private set;
        }

        public List<string> PluginUvGroups = new List<string>();

        #endregion

        public frmMain(string fn)
        {
            InitializeComponent();

            // Enable double duffering to stop flickering.
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            KeyPreview = true;
            ProgramCore.ProgressProc += ProgressProc;

            PluginUvGroups.AddRange(new[] { "1_lip", "1_skinface", "lips", "face" });
            if (!string.IsNullOrEmpty(fn))
            {
                if (fn.StartsWith("fs"))
                {
                    ProgramCore.PluginMode = true;
                    var strs = fn.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower().Trim());
                    foreach (var str in strs)
                    {
                        if (str.StartsWith("uvgroups"))
                        {
                            PluginUvGroups.AddRange(str.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower().Trim()).ToList());
                            break;
                        }
                    }
                }
                else
                    if ((fn.Contains(".hds") || fn.Contains(".hrs")) && File.Exists(fn)) // open associated files
                        OpenProject(fn);
            }
        }
        private void exitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ProgramCore.Splash.ShowDialog();

            if (UserConfig.ByName("Options")["Tutorials", "Start", "1"] == "1")
                frmTutStart.ShowDialog(this);

            ctrlRenderControl.Initialize();

            var newProjectDlg = new frmNewProject1(true);
            newProjectDlg.ShowDialog(this);

            if (newProjectDlg.dialogResult != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            if (newProjectDlg.LoadProject && !string.IsNullOrEmpty(newProjectDlg.LoadingProject))
                OpenProject(newProjectDlg.LoadingProject);
            else
                CreateNewProject(newProjectDlg.ProjectFolder, newProjectDlg.ProjectName, newProjectDlg.TemplateImage, true);

            if (ProgramCore.PluginMode)     // хотелка, что бы прога всегда была выше Daz'a
                TopMost = true;

            frmMaterial = new frmMaterials();
            frmMaterial.VisibleChanged += frmMaterial_VisibleChanged;

            frmAccessories = new frmAccessories();
            frmAccessories.VisibleChanged += frmAccessories_VisibleChanged;

            frmStyles = new frmStyles();
            frmStyles.VisibleChanged += frmStyles_VisibleChanged;

            frmStages = new frmStages();
            frmStages.VisibleChanged += frmStages_VisibleChanged;

            frmParts = new frmParts();
            frmParts.UpdateList();

            frmFreeHand = new frmFreeHand();

            panelCut = new PanelCut
            {
                Dock = DockStyle.Fill
            };
            panelCut.OnDelete += hairLibraryOnDelete_Click;
            panelCut.OnSave += OnSavePart_Click;
            panelCut.OnUndo += OnUndo_Click;

            panelShape = new PanelShape
            {
                Dock = DockStyle.Fill
            };
            panelShape.OnSave += OnSavePart_Click;
            panelShape.OnUndo += OnUndo_Click;

            panelAccessories = new PanelLibrary(true, true)
            {
                Dock = DockStyle.Fill
            };
            panelAccessories.OnOpenLibrary += accessoryLibraryOnOpen_Click;
            panelAccessories.OnDelete += accessoryLibraryOnDelete_Click;
            panelAccessories.OnExport += OnExport_Click;
            panelAccessories.OnSave += OnSavePart_Click;

            panelStyles = new PanelLibrary(true, true)
            {
                Dock = DockStyle.Fill
            };
            panelStyles.OnOpenLibrary += styleLibraryOnOpen_Click;
            panelStyles.OnDelete += styleLibraryOnDelete_Click;
            panelStyles.OnExport += OnExport_Click;
            panelStyles.OnSave += OnSavePart_Click;

            panelShape.OnSave += OnSavePart_Click;

            panelMaterials = new PanelLibrary(true, true)
            {
                Dock = DockStyle.Fill
            };
            panelMaterials.OnOpenLibrary += materialLibraryOnOpen_Click;
            panelMaterials.OnDelete += materialLibraryOnDelete_Click;
            panelMaterials.OnExport += OnExport_Click;
            panelMaterials.OnSave += OnSavePart_Click;
            panelShape.OnSave += OnSavePart_Click;

            panelStages = new PanelLibrary(true, false)
            {
                Dock = DockStyle.Fill
            };
            panelStages.OnOpenLibrary += stagesLibraryOnOpen_Click;
            panelStages.OnExport += OnExport_Click;
            panelStages.OnDelete += stagesLibraryOnDelete_Click;

            panelFront = new PanelHead(true)
            {
                Dock = DockStyle.Fill
            };
            panelFront.OnDelete += OnDeleteHeadSelectedPoints_Click;
            panelFront.OnSave += OnSaveHead_Click;
            panelFront.OnUndo += OnUndo_Click;
            panelFront.OnShapeTool += OnShapeTool_Click;

            panelProfile = new PanelHead(false)
            {
                Dock = DockStyle.Fill
            };
            panelProfile.OnDelete += OnDeleteHeadSelectedPoints_Click;
            panelProfile.OnSave += OnSaveHead_Click;
            panelProfile.OnUndo += OnUndo_Click;
            panelProfile.OnShapeTool += OnShapeTool_Click;

            panelFeatures = new PanelFeatures
            {
                Dock = DockStyle.Fill
            };
            panelFeatures.OnDelete += OnDeleteHeadSelectedPoints_Click;
            panelFeatures.OnSave += OnSaveHead_Click;
            panelFeatures.OnUndo += OnUndo_Click;

            if (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Count == 0)
                panelMenuFront_Click(null, EventArgs.Empty); // set opened by default
            else
                panelMenuCut_Click(null, EventArgs.Empty);      // иначе это наш проект волосач и по дефолту мы работаем с волосами, а не с формой лица.

            InitRecentItems();

            if (ProgramCore.Project.ManType == ManType.Custom && UserConfig.ByName("Options")["Tutorials", "CustomHeads", "1"] == "1")
                frmTutCustomHeads.ShowDialog(this);
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmMaterial = null;
            frmAccessories = null;
            frmStyles = null;
            frmStages = null;
            e.Cancel = false;
        }

        private void InitRecentItems()
        {
            mruManager.Initialize(this, ProgramCore.RegistryPath);

            if (mruManager.mruList.Count == 0)
            {
                var recentFile = new ToolStripMenuItem
                {
                    Text = "Recent File",
                    Enabled = false
                };
                fileToolStripMenuItem.DropDownItems.Add(recentFile);
            }
            else
            {
                foreach (string item in mruManager.mruList)
                {
                    var recentFile = new ToolStripMenuItem
                    {
                        Text = item
                    };
                    recentFile.Click += recentFile_Click;
                    fileToolStripMenuItem.DropDownItems.Add(recentFile);
                }
            }

            var separator = new ToolStripSeparator();
            fileToolStripMenuItem.DropDownItems.Add(separator);

            var exitBtn = new ToolStripMenuItem
            {
                Text = "Exit"
            };
            exitBtn.Click += exitBtn_Click;
            fileToolStripMenuItem.DropDownItems.Add(exitBtn);
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProgramCore.MainForm.ctrlRenderControl.historyController.Undo();
        }

        private void OnSavePart_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveSelectedHairToPartsLibrary();
        }
        private void OnSaveHead_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveHeadToFile();
        }

        private void OnDeleteHeadSelectedPoints_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.headController.RemoveSelectedPoints();
        }

        private void OnUndo_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.historyController.Undo();
        }

        private bool beginExport;
        private void OnExport_Click(object sender, EventArgs e)
        {
            if (beginExport)
                return;

            beginExport = true;
            ctrlRenderControl.Export();
            beginExport = false;
        }

        #region Information

        public void ShowTutorial()
        {
            Process.Start("http://lib.store.yahoo.net/lib/yhst-48396527764316/HeadShop1.0.pdf");
        }
        public void ShowSiteInfo()
        {
            Process.Start("http://www.abalonellc.com/");
        }

        private void showManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTutorial();
        }
        private void aboutHeadShopProToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProgramCore.Splash.ShowDialog(this);
        }

        #endregion

        #region Libraries and panels

        private void frmAccessories_VisibleChanged(object sender, EventArgs e)
        {
            if (frmAccessories.Visible)
                panelAccessories.ShowControl();
            else
                panelAccessories.HideControl();
        }
        private void frmMaterial_VisibleChanged(object sender, EventArgs e)
        {
            if (frmMaterial.Visible)
                panelMaterials.ShowControl();
            else
                panelMaterials.HideControl();
        }
        private void frmStages_VisibleChanged(object sender, EventArgs e)
        {
            if (frmStages.Visible)
                panelStages.ShowControl();
            else
                panelStages.HideControl();
        }
        private void frmStyles_VisibleChanged(object sender, EventArgs e)
        {
            if (frmStyles.Visible)
                panelStyles.ShowControl();
            else
                panelStyles.HideControl();
        }

        private void materialLibraryOnOpen_Click(object sender, EventArgs e)
        {
            if (frmMaterial.Visible)
                frmMaterial.Hide();
            else
                frmMaterial.Show(this);
        }
        private void accessoryLibraryOnOpen_Click(object sender, EventArgs e)
        {
            if (frmAccessories.Visible)
                frmAccessories.Hide();
            else
                frmAccessories.Show(this);
        }
        private void styleLibraryOnOpen_Click(object sender, EventArgs e)
        {
            if (frmStyles.Visible)
                frmStyles.Hide();
            else
                frmStyles.Show(this);
        }
        private void OnShapeTool_Click(object sender, EventArgs e)
        {
            if (frmFreeHand.Visible)
                frmFreeHand.Hide();
            else
                frmFreeHand.Show(this);
        }
        private void stagesLibraryOnOpen_Click(object sender, EventArgs e)
        {
            if (frmStages.Visible)
                frmStages.Hide();
            else
            {
                panelMenuStage_Click(this, EventArgs.Empty);
                ProgramCore.MainForm.ctrlRenderControl.pickingController.SelectedMeshes.Clear();
                frmStages.Show(this);
            }
        }
        private void partsLibraryOnOpen_Click(object sender, EventArgs e)
        {
            if (frmParts.Visible)
                frmParts.Hide();
            else
                frmParts.Show(this);
        }
        private void styleLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmStyles.Visible)
                frmStyles.Hide();
            else
            {
                if (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Count > 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes.Count > 0)         // mean that it's user action
                {
                    if (MessageBox.Show("This action will remove all changes. Are you sure?", "Attention", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        return;

                    ctrlRenderControl.CleanProjectMeshes();                     // clear all changes and reset position.
                    ctrlRenderControl.OrtoTop();
                }

                frmStyles.Show(this);
            }
        }

        private void stagesLibraryOnDelete_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SetDefaultBackground();
        }
        private void materialLibraryOnDelete_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.DeleteSelectedTexture();
        }
        private void accessoryLibraryOnDelete_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.DeleteSelectedAccessory();
        }
        private void styleLibraryOnDelete_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.DeleteSelectedAccessory();
        }
        private void hairLibraryOnDelete_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.DeleteSelectedHair();
        }

        public void panelMenuCut_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuCut.Tag.ToString() == "2")
            {
                panelMenuCut.Tag = "1";
                panelMenuCut.Image = Properties.Resources.btnMenuCutPressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelCut);

                panelMenuStyle.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = panelMenuStage.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;
                ResetModeTools();

                if (UserConfig.ByName("Options")["Tutorials", "Cut", "1"] == "1")
                    frmTutCut.ShowDialog(this);
            }
        }
        public void panelMenuShape_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuShape.Tag.ToString() == "2")
            {
                panelMenuShape.Tag = "1";
                panelMenuShape.Image = Properties.Resources.btnMenuShapePressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelShape);

                panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = panelMenuStage.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;
                ResetModeTools();

                if (UserConfig.ByName("Options")["Tutorials", "Shape", "1"] == "1")
                    frmTutShape.ShowDialog(this);
            }
        }
        public void panelMenuAccessories_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuAccessories.Tag.ToString() == "2")
            {
                panelMenuAccessories.Tag = "1";
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesPressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelAccessories);

                panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuMaterials.Tag = panelMenuStage.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;

                if (UserConfig.ByName("Options")["Tutorials", "Accessory", "1"] == "1")
                    frmTutAccessory.ShowDialog(this);
            }
        }
        public void panelMenuMaterials_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuMaterials.Tag.ToString() == "2")
            {
                panelMenuMaterials.Tag = "1";
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorPressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelMaterials);

                panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuStage.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;

                if (UserConfig.ByName("Options")["Tutorials", "Material", "1"] == "1")
                    frmTutMaterial.ShowDialog(this);
            }
        }
        public void panelMenuStage_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuStage.Tag.ToString() == "2")
            {
                panelMenuStage.Tag = "1";
                panelMenuStage.Image = Properties.Resources.btnMenuStagePressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelStages);

                panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesActivate(false);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;

                if (UserConfig.ByName("Options")["Tutorials", "Stage", "1"] == "1")
                    frmTutStage.ShowDialog(this);
            }
        }
        public void panelMenuStyle_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuStyle.Tag.ToString() == "2")
            {
                if (sender != null && (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Count > 0 || ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes.Count > 0))         // mean that it's user action
                {
                    if (MessageBox.Show("This action will remove all changes. Are you sure?", "Attention", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        return;

                    ctrlRenderControl.CleanProjectMeshes();                     // clear all changes and reset position.
                    ctrlRenderControl.OrtoTop();
                }

                panelMenuStyle.Tag = "1";
                panelMenuStyle.Image = Properties.Resources.btnMenuStylePressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelStyles);

                panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = panelMenuStage.Tag = "2";
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица
                HeadMode = HeadFront = HeadProfile = HeadFeature = false;
                ResetModeTools();

                if (frmStyles != null && !frmStyles.Visible)
                    styleLibraryOnOpen_Click(null, EventArgs.Empty);

                if (UserConfig.ByName("Options")["Tutorials", "Style", "1"] == "1")
                    frmTutStyle.ShowDialog(this);
            }
        }

        public void panelMenuFront_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuFront.Tag.ToString() == "2")
            {
                panelMenuFront.Tag = "1";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontPressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelFront);

                panelMenuStage.Tag = panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuProfile.Tag = panelMenuFeatures.Tag = "2";
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                HeadMode = true;
                HeadFront = true;
                HeadProfile = HeadFeature = false;

                if (sender != null)         // иначе это во время загрузки программы. и не надо менять мод!
                    ctrlRenderControl.Mode = Mode.None;

                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();            // поворачиваем морду как надо
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица

                if (UserConfig.ByName("Options")["Tutorials", "Autodots", "1"] == "1")
                    frmTutAutodots.ShowDialog(this);
            }
        }
        private void panelMenuProfile_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (panelMenuProfile.Tag.ToString() == "2")
            {
                panelMenuProfile.Tag = "1";
                panelMenuProfile.Image = Properties.Resources.btnMenuProfilePressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelProfile);

                panelMenuStage.Tag = panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuFeatures.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                HeadMode = true;
                HeadProfile = true;
                HeadFront = HeadFeature = false;
                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = true;
                ctrlRenderControl.OrtoRight();            // поворачиваем морду как надо
                EnableRotating();

                ProgramCore.MainForm.ctrlRenderControl.autodotsShapeHelper.UpdateProfileLines();
                ProgramCore.MainForm.ctrlTemplateImage.InitializeProfileControlPoints();

                if (ProgramCore.Project.ProfileImage == null) // если нет профиля - просто копируем ебало справа
                {
                    ctrlRenderControl.Render();             // специально два раза, чтобы переинициализировать буфер. хз с чем это связано.
                    ctrlRenderControl.Render();
                    ctrlTemplateImage.btnCopyProfileImg_MouseUp(null, null);
                }
                else
                    ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.ProfileImage);

                if (ProgramCore.Project.CustomHeadNeedProfileSetup)     // произвольная модель. при первом включении нужно произвести первоначальную настройку.
                    ProgramCore.MainForm.ctrlRenderControl.SetCustomProfileSetup();

                if (UserConfig.ByName("Options")["Tutorials", "Profile", "1"] == "1")
                    frmTutProfile.ShowDialog(this);
            }
        }
        private void panelMenuFeatures_Click(object sender, EventArgs e)
        {
            if (ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomControlPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomPoints || ProgramCore.MainForm.ctrlRenderControl.Mode == Mode.SetCustomProfilePoints)
                return;

            if (ProgramCore.Project.ManType == ManType.Custom)
            {
                MessageBox.Show("Features not available for custom head!", "HeadShop", MessageBoxButtons.OK);
                return;
            }

            if (panelMenuFeatures.Tag.ToString() == "2")
            {
                panelFeatures.SetAge(ProgramCore.Project.AgeCoefficient);
                panelFeatures.Setfat(ProgramCore.Project.FatCoefficient);

                panelMenuFeatures.Tag = "1";
                panelMenuFeatures.Image = Properties.Resources.btnMenuFeaturesPressed;
                panelMenuControl.Controls.Clear();
                panelMenuControl.Controls.Add(panelFeatures);

                panelMenuStage.Tag = panelMenuStyle.Tag = panelMenuCut.Tag = panelMenuShape.Tag = panelMenuAccessories.Tag = panelMenuMaterials.Tag = "2";
                panelMenuStyle.Image = Properties.Resources.btnMenuStyleNormal;
                panelMenuCut.Image = Properties.Resources.btnMenuCutNormal;
                panelMenuShape.Image = Properties.Resources.btnMenuShapeNormal;
                panelMenuAccessories.Image = Properties.Resources.btnMenuAccessoriesNormal;
                panelMenuMaterials.Image = Properties.Resources.btnMenuColorNormal;
                panelMenuStage.Image = Properties.Resources.btnMenuStageNormal;

                panelMenuFront.Tag = panelMenuProfile.Tag = "2";
                panelMenuFront.Image = Properties.Resources.btnMenuFrontNormal;
                panelMenuProfile.Image = Properties.Resources.btnMenuProfileNormal;

                ProgramCore.MainForm.ctrlRenderControl.StagesDeactivate(-1);

                HeadMode = HeadFront = HeadProfile = HeadFeature = false;
                HeadFeature = true;
                HeadFront = HeadProfile = false;
                ctrlRenderControl.Mode = Mode.None;
                ctrlTemplateImage.btnCopyProfileImg.Visible = false;
                ctrlRenderControl.OrtoTop();
                EnableRotating();
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(ProgramCore.Project.FrontImage);       // возвращаем как было, после изменения профиля лица


                /*       if (UserConfig.ByName("Options")["Tutorials", "Features", "1"] == "1")
                           frmTutStage.ShowDialog(this);*/
            }
        }

        #endregion

        #region Navigaion

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.ZoomIn();
        }
        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.ZoomOut();
        }
        private void fitWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.camera.ResetCamera(true);
        }

        private void ortoBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.OrtoBack();
        }
        private void ortoTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.OrtoTop();
        }
        private void ortoLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.OrtoLeft();
        }
        private void ortoRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.OrtoRight();
        }

        private void turnRightoneStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TurnRight();
        }
        private void turnLeftoneStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TurnLeft();
        }

        private void stepToponeStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.StepTop();
        }
        private void stepBottomoneStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.StepBottom();
        }

        private void panTopcontinuousPanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TimerMode = Mode.TimerStepTop;
            ctrlRenderControl.workTimer.Start();
        }
        private void panBottomcontinuousPanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TimerMode = Mode.TimerStepBottom;
            ctrlRenderControl.workTimer.Start();
        }

        private void rotateLeftcontinuousRotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TimerMode = Mode.TimerTurnLeft;
            ctrlRenderControl.workTimer.Start();
        }
        private void rotateRightcontinuousRotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TimerMode = Mode.TimerTurnRight;
            ctrlRenderControl.workTimer.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.TimerMode = Mode.None;
            ctrlRenderControl.workTimer.Stop();
        }

        #endregion

        #region ProgressProc

        private bool isProgress;
        private DateTime lastUpdateDateTime = DateTime.MinValue;
        private void ProgressProc(object sender, ProgressProcEventArgs e)
        {
            var now = DateTime.Now;
            if (!isProgress)
            {
                StartProgress();
            }
            if (now - lastUpdateDateTime > TimeSpan.FromMilliseconds(40))
            {
                Application.DoEvents();
                lastUpdateDateTime = now;
            }
            ProgressManager.ProgressProc(sender, e);
        }
        private readonly ProgressMessageFilter progressMessageFilter = new ProgressMessageFilter();
        private void StartProgress()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.AddMessageFilter(progressMessageFilter);
                ProgramCore.AddCallStackReleasedProc(_stopProgress);
                isProgress = true;
            }
            catch (Exception)
            {
            }
        }
        private void StopProgress()
        {
            try
            {
                Application.RemoveMessageFilter(progressMessageFilter);
                Cursor = Cursors.Default;
                isProgress = false;
            }
            catch (Exception)
            {
            }
        }
        private void _stopProgress(object sender, EventArgs e)
        {
            StopProgress();
        }
        class ProgressMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.HWnd == ProgressManager.ProgressHWnd || m.HWnd == ProgressManager.ProgressCancelButtonHWnd)
                    return false;
                return (m.Msg != WMConsts.WM_TIMER && (m.Msg >= WMConsts.WM_KEYFIRST && m.Msg <= WMConsts.WM_KEYLAST || m.Msg >= WMConsts.WM_MOUSEFIRST && m.Msg <= WMConsts.WM_MOUSELAST));
            }
        }

        #endregion

        #region Instruments

        private void checkHand_Click(object sender, EventArgs e)
        {
            if (checkHand.Tag.ToString() == "2")
            {
                ResetScaleModeTools();
                ctrlRenderControl.ScaleMode = ScaleMode.Move;

                checkHand.Tag = "1";
                checkArrow.Tag = checkZoom.Tag = "2";

                checkHand.Image = Properties.Resources.btnHandPressed;
                checkArrow.Image = Properties.Resources.btnArrowNormal;
                checkZoom.Image = Properties.Resources.btnZoomNormal;
            }
            else
            {
                checkHand.Tag = "2";
                checkHand.Image = Properties.Resources.btnHandNormal;

                ctrlRenderControl.ScaleMode = ScaleMode.None;
            }
        }
        private void checkArrow_Click(object sender, EventArgs e)
        {
            if (checkArrow.Tag.ToString() == "2")
            {
                ResetScaleModeTools();
                ctrlRenderControl.ScaleMode = ScaleMode.Rotate;

                checkArrow.Tag = "1";
                checkHand.Tag = checkZoom.Tag = "2";

                checkArrow.Image = Properties.Resources.btnArrowPressed;
                checkHand.Image = Properties.Resources.btnHandNormal;
                checkZoom.Image = Properties.Resources.btnZoomNormal;
            }
            else
            {
                checkArrow.Tag = "2";
                checkArrow.Image = Properties.Resources.btnArrowNormal;

                ctrlRenderControl.ScaleMode = ScaleMode.None;
            }
        }
        private void checkZoom_Click(object sender, EventArgs e)
        {
            if (checkZoom.Tag.ToString() == "2")
            {
                ResetScaleModeTools();
                ctrlRenderControl.ScaleMode = ScaleMode.Zoom;

                checkZoom.Tag = "1";
                checkHand.Tag = checkArrow.Tag = "2";

                checkZoom.Image = Properties.Resources.btnZoomPressed;
                checkHand.Image = Properties.Resources.btnHandNormal;
                checkArrow.Image = Properties.Resources.btnArrowNormal;
            }
            else
            {
                checkZoom.Tag = "2";
                checkZoom.Image = Properties.Resources.btnZoomNormal;

                ctrlRenderControl.ScaleMode = ScaleMode.None;
            }
        }

        private void btnUnscale_MouseDown(object sender, MouseEventArgs e)
        {
            btnUnscale.Image = Properties.Resources.btnUnscalePressed;
        }
        private void btnUnscale_MouseUp(object sender, MouseEventArgs e)
        {
            btnUnscale.Image = Properties.Resources.btnUnscaleNormal;

            ctrlRenderControl.camera.ResetCamera(true);
            if (ProgramCore.MainForm.HeadProfile)
                ctrlRenderControl.OrtoRight();

            ctrlTemplateImage.RecalcRealTemplateImagePosition();

            checkHand.Tag = checkArrow.Tag = checkZoom.Tag = "2";
            checkHand.Image = Properties.Resources.btnHandNormal;
            checkArrow.Image = Properties.Resources.btnArrowNormal;
            checkZoom.Image = Properties.Resources.btnZoomNormal;

            //    ctrlRenderControl.Mode = Mode.None;
        }

        private void photoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmStages.DoPhoto();
        }

        private void stage1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetStage1();
        }
        private void stage2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetStage2();
        }
        private void stage3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetStage3();
        }

        private void pose1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetPose1();
        }
        private void pose2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetPose2();
        }
        private void pose3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelMenuStage_Click(this, EventArgs.Empty);
            frmStages.SetPose3();
        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelCut.btnLine_Click(this, EventArgs.Empty);
        }
        private void polyLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelCut.btnPolyLine_Click(this, EventArgs.Empty);
        }
        private void arcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelCut.btnArc_Click(this, EventArgs.Empty);
        }

        private void mirrorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            panelShape.btnMirror_Click(this, EventArgs.Empty);
        }
        private void stretchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelShape.btnStretch_Click(this, EventArgs.Empty);
        }
        private void shapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelShape.btnShape_Click(this, EventArgs.Empty);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelCut.btnCut_Click(this, EventArgs.Empty);
        }
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelCut.btnMirror_Click(this, EventArgs.Empty);
        }

        #endregion

        #region Project

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmNewProject1(false);
            frm.ShowDialog();

            if (frm.dialogResult != DialogResult.OK)
                return;

            CreateNewProject(frm.ProjectFolder, frm.ProjectName, frm.TemplateImage, false);
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialogEx("Open HeadShop/HairShop project", "HeadShop projects|*.hds|HairShop projects|*.hs"))
            {
                if (ofd.ShowDialog(false) != DialogResult.OK)
                    return;

                OpenProject(ofd.FileName);
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProgramCore.Project != null)
            {
                ctrlRenderControl.pickingController.SelectedMeshes.Clear();
                ProgramCore.Project.ToStream();
                MessageBox.Show("Project successfully saved!", "Done", MessageBoxButtons.OK);
            }
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProgramCore.Project == null)
                return;

            using (var sfd = new SaveFileDialogEx("Save as HeadShop project", "HeadShop projects|*.hs|OBJ Hair|*.obj|DAE model|*.dae"))
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    switch (sfd.FilterIndex)
                    {
                        case 1:
                            #region All project

                            var fullPath = sfd.FileName;
                            var projectName = Path.GetFileNameWithoutExtension(fullPath);
                            var projectPath = Path.GetDirectoryName(fullPath);

                            var parts = new List<Tuple<string, bool, MeshType, List<Guid>>>(); // save to part's library
                            foreach (var part in ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes)
                                parts.Add(new Tuple<string, bool, MeshType, List<Guid>>(part.Key, part.Value[0].IsVisible, part.Value[0].meshType, new List<Guid>(part.Value.Select(x => x.Id))));

                            ctrlRenderControl.pickingController.SelectedMeshes.Clear();
                            var img = Path.Combine(ProgramCore.Project.ProjectPath, ProgramCore.Project.FrontImagePath);

                            ProgramCore.Project = new Project(projectName, projectPath, img, ProgramCore.Project.ManType, ProgramCore.Project.HeadModelPath, true);

                            foreach (var part in parts)                     // restore part's library in new project
                            {
                                ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes.Add(part.Item1, new DynamicRenderMeshes());

                                foreach (var meshId in part.Item4)
                                {
                                    var mesh = part.Item3 == MeshType.Accessory ?
                                                                             ProgramCore.MainForm.ctrlRenderControl.pickingController.AccesoryMeshes[meshId] :
                                                                             ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes[meshId];
                                    if (mesh == null)
                                        continue;
                                    mesh.IsVisible = part.Item2;
                                    ProgramCore.MainForm.ctrlRenderControl.PartsLibraryMeshes[part.Item1].Add(mesh);
                                }
                            }

                            ProgramCore.Project.ToStream();
                            MessageBox.Show("Project successfully saved!", "Done", MessageBoxButtons.OK);

                            #endregion
                            break;
                        case 2:
                            ctrlRenderControl.Export();
                            break;
                        case 3:

                            #region Копируем модель

                            File.Copy(ProgramCore.Project.HeadModelPath, sfd.FileName, true);           // сама модель

                            #region Обрабатываем mtl файл и папку с текстурами

                            var oldFileName = Path.GetFileNameWithoutExtension(ProgramCore.Project.HeadModelPath);
                            var mtl = oldFileName + ".mtl";
                            using (var ms = new StreamReader(ProgramCore.Project.HeadModelPath))
                            {
                                for (var i = 0; i < 10; i++)
                                {
                                    if (ms.EndOfStream)
                                        break;
                                    var line = ms.ReadLine();
                                    if (line.ToLower().Contains("mtllib"))          // ищем ссылку в obj файле на mtl файл (у них могут быть разные названия, но всегда в одной папке
                                    {
                                        var lines = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                        if (lines.Length > 1)
                                        {
                                            mtl = lines[1];
                                            break;
                                        }
                                    }
                                }
                            }

                            ObjLoader.CopyMtl(mtl, mtl, Path.GetDirectoryName(ProgramCore.Project.HeadModelPath), "", Path.GetFileNameWithoutExtension(sfd.FileName));

                            #endregion

                            MessageBox.Show("Model successfully exported!", "Done", MessageBoxButtons.OK);

                            #endregion



                            break;
                    }
                }
            }
        }
        private void recentFile_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            OpenProject(item.Text);
        }

        public void UpdateProjectControls(bool newProject)
        {
            if (ProgramCore.Project == null)
            {
                ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(null);
            }
            else
            {
                ctrlRenderControl.LoadProject(newProject);
                ctrlRenderControl.camera.UpdateDy();

                if (panelCut != null && panelStyles != null)
                {
                    if (ProgramCore.MainForm.ctrlRenderControl.pickingController.HairMeshes.Count == 0)
                        panelMenuStyle_Click(null, EventArgs.Empty);
                    else
                        panelMenuCut_Click(null, EventArgs.Empty);
                }

                if (frmStages != null)
                    frmStages.InitializeListView();

                if (ProgramCore.Project.FrontImage == null)
                    ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage(null);
                else
                {
                    using (var bmp = new Bitmap(ProgramCore.Project.FrontImage))
                        ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage((Bitmap)bmp.Clone());
                }
            }

            if (frmStages != null)
            {
                ctrlRenderControl.StagesActivate(false);     // for recalc
                ctrlRenderControl.StagesDeactivate(0);
            }

            if (frmParts != null)
                frmParts.UpdateList();
        }
        private void OpenProject(string fileName)
        {
            ProgramCore.Project = Project.FromStream(fileName);
            UpdateProjectControls(false);

            ProgramCore.MainForm.ctrlRenderControl.InitializeShapedotsHelper();         // инициализация точек головы.

            if (ProgramCore.Project.AgeCoefficient != 0 || ProgramCore.Project.FatCoefficient != 0)  // восстанавливаем морфинги
            {
                foreach (var m in ProgramCore.MainForm.ctrlRenderControl.OldMorphing)
                    m.Value.Delta = ProgramCore.Project.AgeCoefficient;
                foreach (var m in ProgramCore.MainForm.ctrlRenderControl.FatMorphing)
                    m.Value.Delta = ProgramCore.Project.FatCoefficient;
                ProgramCore.MainForm.ctrlRenderControl.DoMorth();
            }

            MessageBox.Show("Project successfully loaded!", "Done");
            mruManager.Add(fileName);
        }

        private void CreateNewProject(string projectFolder, string projectName, string templateImage, bool needClose)
        {
            var projectPath = Path.Combine(projectFolder, string.Format("{0}.hds", projectName));

            #region Корректируем размер фотки

            using (var ms = new MemoryStream(File.ReadAllBytes(templateImage))) // Don't use using!!
            {
                var img = (Bitmap)Bitmap.FromStream(ms);
                var max = Math.Max(img.Width, img.Height);
                if (max > 1024)
                {
                    var k = 1024.0f / max;
                    var newImg = ImageEx.ResizeImage(img, new Size((int)(img.Width * k), (int)(img.Height * k)));

                    templateImage = UserConfig.AppDataDir;
                    FolderEx.CreateDirectory(templateImage);
                    templateImage = Path.Combine(templateImage, "tempProjectImage.jpg");

                    newImg.Save(templateImage, ImageFormat.Jpeg);
                }
            }

            #endregion

            var faceRecognition = new FaceRecognition();
            faceRecognition.Recognize(ref templateImage, true);     // это ОЧЕНЬ! важно. потому что мы во время распознавания можем создать обрезанную фотку и использовать ее как основную в проекте.

            var frm1 = new frmNewProject2(projectPath, templateImage, faceRecognition);
            frm1.ShowDialog(this);

            if (frm1.dialogResult != DialogResult.OK)
            {
                if (needClose)
                    Application.Exit();
                return;
            }

            ProgramCore.Project = new Project(projectName, projectFolder, templateImage, frm1.ManType, frm1.CustomModelPath, true);
            ProgramCore.Project.FaceRectRelative = faceRecognition.FaceRectRelative;
            ProgramCore.Project.MouthCenter = faceRecognition.MouthCenter;
            ProgramCore.Project.LeftEyeCenter = faceRecognition.LeftEyeCenter;
            ProgramCore.Project.RightEyeCenter = faceRecognition.RightEyeCenter;
            UpdateProjectControls(true);

            ProgramCore.MainForm.ctrlRenderControl.InitializeShapedotsHelper();         // инициализация точек головы. эта инфа тоже сохранится в проект

            ProgramCore.Project.ToStream();
            // ProgramCore.MainForm.ctrlRenderControl.UpdateMeshProportions();

            if (ProgramCore.Project.ManType == ManType.Custom)
            {
                ProgramCore.MainForm.ctrlRenderControl.Mode = Mode.SetCustomControlPoints;
                ProgramCore.MainForm.ctrlRenderControl.InitializeCustomControlSpritesPosition();

                var exampleImgPath = Path.Combine(Application.StartupPath, "Plugin", "ControlBaseDotsExample.jpg");
                using (var ms = new MemoryStream(File.ReadAllBytes(exampleImgPath))) // Don't use using!!
                    ProgramCore.MainForm.ctrlTemplateImage.SetTemplateImage((Bitmap)Bitmap.FromStream(ms), false);          // устанавливаем картинку помощь для юзера
            }

            mruManager.Add(projectPath);
        }

        internal void ResetModeTools()
        {
            if (panelShape != null)
                panelShape.ResetModeTools();
            if (panelCut != null)
                panelCut.ResetModeTools();
            if (panelFront != null)
                panelFront.ResetModeTools();
            if (panelProfile != null)
                panelProfile.ResetModeTools();

            ctrlRenderControl.ResetModeTools();
        }
        private void ResetScaleModeTools()
        {
            if (checkHand.Tag.ToString() == "1")
                checkHand_Click(this, EventArgs.Empty);
            if (checkZoom.Tag.ToString() == "1")
                checkZoom_Click(this, EventArgs.Empty);
            if (checkArrow.Tag.ToString() == "1")
                checkArrow_Click(this, EventArgs.Empty);
        }

        #endregion

        #region Helping

        public void ShowVideo()
        {
            Process.Start("https://www.youtube.com/watch?v=rkELnRgRHP4");
        }
        private void videoTutorialPart1CutAndShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowVideo();
        }
        private void videoTutorialPart2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=AjG09RGgHvw&feature=youtu.be");
        }

        private void startHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutStart.ShowDialog(this);
        }

        private void cutHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutCut.ShowDialog(this);
        }
        private void shapeHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutShape.ShowDialog(this);
        }
        private void accessoriesHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutAccessory.ShowDialog(this);
        }
        private void materialHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutMaterial.ShowDialog(this);
        }
        private void stageHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutStage.ShowDialog(this);
        }
        private void styleHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutStyle.ShowDialog(this);
        }

        private void autodotsHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutAutodots.ShowDialog(this);
        }
        private void shapedotsHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutShapedots.ShowDialog(this);
        }
        private void mirrorHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutMirror.ShowDialog(this);
        }
        private void freehandHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutFreehand.ShowDialog(this);
        }
        private void profileHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTutProfile.ShowDialog(this);
        }

        public void EnableRotating()
        {
            checkArrow.Enabled = true;
            rotateLeftcontinuousRotateToolStripMenuItem.Enabled = true;
            rotateRightcontinuousRotateToolStripMenuItem.Enabled = true;
            turnLeftoneStepToolStripMenuItem.Enabled = true;
            turnRightoneStepToolStripMenuItem.Enabled = true;

            ctrlRenderControl.panelOrtoLeft.Enabled = ctrlRenderControl.panelOrtoRight.Enabled = true;
        }
        public void DisableRotating()
        {
            if (checkArrow.Tag.ToString() == "1")
                checkArrow_Click(this, EventArgs.Empty);
            if (ctrlRenderControl.TimerMode == Mode.TimerTurnLeft || ctrlRenderControl.TimerMode == Mode.TimerTurnRight)
            {
                ctrlRenderControl.workTimer.Stop();
                ctrlRenderControl.TimerMode = Mode.None;
            }

            checkArrow.Enabled = false;
            rotateLeftcontinuousRotateToolStripMenuItem.Enabled = false;
            rotateRightcontinuousRotateToolStripMenuItem.Enabled = false;
            turnLeftoneStepToolStripMenuItem.Enabled = false;
            turnRightoneStepToolStripMenuItem.Enabled = false;

            ctrlRenderControl.panelOrtoLeft.Enabled = ctrlRenderControl.panelOrtoRight.Enabled = false;
        }

        #endregion

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            ctrlTemplateImage.KeyDown(e);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnExport_Click(this, EventArgs.Empty);
        }
        private void exportToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OnExport_Click(null, EventArgs.Empty);
        }
        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OnExport_Click(null, EventArgs.Empty);
        }

        private void pleatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelShape.btnPleat_Click(this, EventArgs.Empty);
        }
        private void saveToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveSelectedHairToPartsLibrary();
        }
        private void deleteToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.DeleteSelectedHair();
        }
        private void exportToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            OnExport_Click(null, EventArgs.Empty);
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (frmStyles.Visible)
                frmStyles.Hide();
            else
                frmStyles.Show(this);
        }

        private void saveToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveHeadToFile();
        }

        private void deleteToolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void undoToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.historyController.Undo();
        }

        private void saveToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveHeadToFile();
        }

        private void deleteToolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }

        private void linesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            panelProfile.btnPolyLine_Click(null, EventArgs.Empty);
        }

        private void handToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            panelProfile.btnShapeTool_Click(null, EventArgs.Empty);
        }

        private void saveToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ctrlRenderControl.SaveHeadToFile();
        }

        private void deleteToolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void autodotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelFront.btnAutodots_Click(null, EventArgs.Empty);
        }

        private void ponitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelFront.btnLasso_Click(null, EventArgs.Empty);
        }

        private void flipToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelFront.btnFlipLeft_Click(null, EventArgs.Empty);
        }

        private void flipToRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelFront.btnFlipRight_Click(null, EventArgs.Empty);
        }
    }
}
