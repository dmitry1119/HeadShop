namespace RH.HeadShop.Controls.Libraries
{
    partial class frmStages
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBarPose = new System.Windows.Forms.TrackBar();
            this.btnPhoto = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.imageListBackgrounds = new RH.ImageListView.ImageListViewEx();
            this.panelPoses = new System.Windows.Forms.GroupBox();
            this.imageListPoses = new RH.ImageListView.ImageListViewEx();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPose)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panelPoses.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnAddNew);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.trackBarPose);
            this.panel1.Controls.Add(this.btnPhoto);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(279, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(117, 364);
            this.panel1.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDelete.Location = new System.Drawing.Point(4, 282);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 29);
            this.btnDelete.TabIndex = 11;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAddNew
            // 
            this.btnAddNew.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAddNew.Location = new System.Drawing.Point(4, 250);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(110, 29);
            this.btnAddNew.TabIndex = 10;
            this.btnAddNew.Text = "Add new";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btnAddNew_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(19, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Posing";
            // 
            // trackBarPose
            // 
            this.trackBarPose.Location = new System.Drawing.Point(34, 34);
            this.trackBarPose.Maximum = 100;
            this.trackBarPose.Name = "trackBarPose";
            this.trackBarPose.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarPose.Size = new System.Drawing.Size(40, 178);
            this.trackBarPose.TabIndex = 2;
            this.trackBarPose.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarPose.Value = 90;
            this.trackBarPose.Scroll += new System.EventHandler(this.trackBarPose_Scroll);
            // 
            // btnPhoto
            // 
            this.btnPhoto.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPhoto.Location = new System.Drawing.Point(4, 218);
            this.btnPhoto.Name = "btnPhoto";
            this.btnPhoto.Size = new System.Drawing.Size(110, 26);
            this.btnPhoto.TabIndex = 1;
            this.btnPhoto.Text = "Photo";
            this.btnPhoto.UseVisualStyleBackColor = true;
            this.btnPhoto.Click += new System.EventHandler(this.btnPhoto_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.imageListBackgrounds);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(140, 364);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backgrounds";
            // 
            // imageListBackgrounds
            // 
            this.imageListBackgrounds.AllowMultyuse = false;
            this.imageListBackgrounds.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.imageListBackgrounds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageListBackgrounds.GroupHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.imageListBackgrounds.Location = new System.Drawing.Point(3, 16);
            this.imageListBackgrounds.Name = "imageListBackgrounds";
            this.imageListBackgrounds.PersistentCacheFile = "";
            this.imageListBackgrounds.PersistentCacheSize = ((long)(100));
            this.imageListBackgrounds.Size = new System.Drawing.Size(134, 345);
            this.imageListBackgrounds.TabIndex = 4;
            this.imageListBackgrounds.SelectionChanged += new System.EventHandler(this.imageListBackgrounds_SelectionChanged);
            this.imageListBackgrounds.DoubleClick += new System.EventHandler(this.imageListBackgrounds_DoubleClick);
            // 
            // panelPoses
            // 
            this.panelPoses.Controls.Add(this.imageListPoses);
            this.panelPoses.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelPoses.Location = new System.Drawing.Point(140, 0);
            this.panelPoses.Name = "panelPoses";
            this.panelPoses.Size = new System.Drawing.Size(140, 364);
            this.panelPoses.TabIndex = 6;
            this.panelPoses.TabStop = false;
            this.panelPoses.Text = "Poses";
            // 
            // imageListPoses
            // 
            this.imageListPoses.AllowMultyuse = false;
            this.imageListPoses.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.imageListPoses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageListPoses.GroupHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.imageListPoses.Location = new System.Drawing.Point(3, 16);
            this.imageListPoses.Name = "imageListPoses";
            this.imageListPoses.PersistentCacheFile = "";
            this.imageListPoses.PersistentCacheSize = ((long)(100));
            this.imageListPoses.Size = new System.Drawing.Size(134, 345);
            this.imageListPoses.TabIndex = 4;
            this.imageListPoses.SelectionChanged += new System.EventHandler(this.imageListPoses_SelectionChanged);
            this.imageListPoses.DoubleClick += new System.EventHandler(this.imageListPoses_DoubleClick);
            // 
            // frmStages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 364);
            this.Controls.Add(this.panelPoses);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmStages";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Stages";
            this.Activated += new System.EventHandler(this.frmStages_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmStages_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPose)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panelPoses.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnPhoto;
        private System.Windows.Forms.Label label1;
        private ImageListView.ImageListViewEx imageListBackgrounds;
        public System.Windows.Forms.TrackBar trackBarPose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox panelPoses;
        private ImageListView.ImageListViewEx imageListPoses;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAddNew;
    }
}