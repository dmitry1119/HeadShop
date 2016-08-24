

namespace RH.HeadShop.Controls.Libraries
{
    partial class frmMaterials
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddNewMaterial = new System.Windows.Forms.Button();
            this.teAngle = new System.Windows.Forms.TextBox();
            this.teAlpha = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ctrlAngle = new RH.HeadShop.Controls.ctrlAngleSelector();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBarSize = new System.Windows.Forms.TrackBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelColor = new System.Windows.Forms.Panel();
            this.btnPickColor = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.imageListView = new RH.ImageListView.ImageListViewEx();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnAddNewMaterial);
            this.panel1.Controls.Add(this.teAngle);
            this.panel1.Controls.Add(this.teAlpha);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.ctrlAngle);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.trackBarSize);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(288, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(82, 399);
            this.panel1.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDelete.Location = new System.Drawing.Point(2, 367);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 29);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAddNewMaterial
            // 
            this.btnAddNewMaterial.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAddNewMaterial.Location = new System.Drawing.Point(2, 338);
            this.btnAddNewMaterial.Name = "btnAddNewMaterial";
            this.btnAddNewMaterial.Size = new System.Drawing.Size(75, 29);
            this.btnAddNewMaterial.TabIndex = 7;
            this.btnAddNewMaterial.Text = "Add new";
            this.btnAddNewMaterial.UseVisualStyleBackColor = true;
            this.btnAddNewMaterial.Click += new System.EventHandler(this.btnAddNewMaterial_Click);
            // 
            // teAngle
            // 
            this.teAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.teAngle.Location = new System.Drawing.Point(10, 244);
            this.teAngle.Name = "teAngle";
            this.teAngle.Size = new System.Drawing.Size(58, 24);
            this.teAngle.TabIndex = 6;
            this.teAngle.Text = "0";
            this.teAngle.TextChanged += new System.EventHandler(this.teAngle_TextChanged);
            this.teAngle.Validating += new System.ComponentModel.CancelEventHandler(this.teAngle_Validating);
            // 
            // teAlpha
            // 
            this.teAlpha.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.teAlpha.Location = new System.Drawing.Point(10, 303);
            this.teAlpha.Name = "teAlpha";
            this.teAlpha.Size = new System.Drawing.Size(58, 24);
            this.teAlpha.TabIndex = 5;
            this.teAlpha.Text = "255";
            this.teAlpha.TextChanged += new System.EventHandler(this.teAlpha_TextChanged);
            this.teAlpha.Validating += new System.ComponentModel.CancelEventHandler(this.teAlpha_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(7, 274);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Opacity";
            // 
            // ctrlAngle
            // 
            this.ctrlAngle.Angle = 0;
            this.ctrlAngle.Location = new System.Drawing.Point(13, 191);
            this.ctrlAngle.Name = "ctrlAngle";
            this.ctrlAngle.Size = new System.Drawing.Size(47, 47);
            this.ctrlAngle.TabIndex = 3;
            this.ctrlAngle.OnAngleChanged += new RH.HeadShop.Controls.ctrlAngleSelector.AngleChangedDelegate(this.ctrlAngle_AngleChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(11, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Angle";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(11, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Size";
            // 
            // trackBarSize
            // 
            this.trackBarSize.Location = new System.Drawing.Point(23, 29);
            this.trackBarSize.Maximum = 20;
            this.trackBarSize.Minimum = 1;
            this.trackBarSize.Name = "trackBarSize";
            this.trackBarSize.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarSize.Size = new System.Drawing.Size(40, 133);
            this.trackBarSize.TabIndex = 0;
            this.trackBarSize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarSize.Value = 10;
            this.trackBarSize.Scroll += new System.EventHandler(this.trackBarSize_Scroll);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panelColor);
            this.panel2.Controls.Add(this.btnPickColor);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(288, 76);
            this.panel2.TabIndex = 1;
            // 
            // panelColor
            // 
            this.panelColor.BackColor = System.Drawing.Color.Silver;
            this.panelColor.Location = new System.Drawing.Point(12, 12);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(110, 53);
            this.panelColor.TabIndex = 1;
            this.panelColor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseDoubleClick);
            // 
            // btnPickColor
            // 
            this.btnPickColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPickColor.Location = new System.Drawing.Point(131, 19);
            this.btnPickColor.Name = "btnPickColor";
            this.btnPickColor.Size = new System.Drawing.Size(151, 41);
            this.btnPickColor.TabIndex = 0;
            this.btnPickColor.Text = "Pick Solid Color";
            this.btnPickColor.UseVisualStyleBackColor = true;
            this.btnPickColor.Click += new System.EventHandler(this.btnPickColor_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // imageListView
            // 
            this.imageListView.AllowMultyuse = false;
            this.imageListView.ColumnHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.imageListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageListView.GroupHeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.imageListView.Location = new System.Drawing.Point(0, 76);
            this.imageListView.Name = "imageListView";
            this.imageListView.PersistentCacheFile = "";
            this.imageListView.PersistentCacheSize = ((long)(100));
            this.imageListView.Size = new System.Drawing.Size(288, 323);
            this.imageListView.TabIndex = 2;
            this.imageListView.DoubleClick += new System.EventHandler(this.imageListView_DoubleClick);
            // 
            // frmMaterials
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 399);
            this.Controls.Add(this.imageListView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMaterials";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Materials";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.frmMaterials_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMaterials_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button btnPickColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBarSize;
        private ctrlAngleSelector ctrlAngle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox teAlpha;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private RH.ImageListView.ImageListViewEx imageListView;
        private System.Windows.Forms.TextBox teAngle;
        private System.Windows.Forms.Button btnAddNewMaterial;
        private System.Windows.Forms.Button btnDelete;
    }
}