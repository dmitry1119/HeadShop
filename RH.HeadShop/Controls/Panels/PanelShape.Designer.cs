namespace RH.HeadShop.Controls.Panels
{
    partial class PanelShape
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnPleat = new System.Windows.Forms.Button();
            this.btnStretch = new System.Windows.Forms.Button();
            this.btnShape = new System.Windows.Forms.Button();
            this.btnMirror = new System.Windows.Forms.Button();
            this.trackBarShape = new System.Windows.Forms.TrackBar();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarShape)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPleat
            // 
            this.btnPleat.BackColor = System.Drawing.SystemColors.Control;
            this.btnPleat.Location = new System.Drawing.Point(141, 17);
            this.btnPleat.Name = "btnPleat";
            this.btnPleat.Size = new System.Drawing.Size(63, 23);
            this.btnPleat.TabIndex = 11;
            this.btnPleat.Tag = "2";
            this.btnPleat.Text = "Pleat";
            this.btnPleat.UseVisualStyleBackColor = false;
            this.btnPleat.Click += new System.EventHandler(this.btnPleat_Click);
            // 
            // btnStretch
            // 
            this.btnStretch.BackColor = System.Drawing.SystemColors.Control;
            this.btnStretch.Location = new System.Drawing.Point(72, 17);
            this.btnStretch.Name = "btnStretch";
            this.btnStretch.Size = new System.Drawing.Size(63, 23);
            this.btnStretch.TabIndex = 10;
            this.btnStretch.Tag = "2";
            this.btnStretch.Text = "Stretch";
            this.btnStretch.UseVisualStyleBackColor = false;
            this.btnStretch.Click += new System.EventHandler(this.btnStretch_Click);
            // 
            // btnShape
            // 
            this.btnShape.BackColor = System.Drawing.SystemColors.Control;
            this.btnShape.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnShape.Location = new System.Drawing.Point(3, 17);
            this.btnShape.Name = "btnShape";
            this.btnShape.Size = new System.Drawing.Size(63, 23);
            this.btnShape.TabIndex = 9;
            this.btnShape.Tag = "2";
            this.btnShape.Text = "Shape";
            this.btnShape.UseVisualStyleBackColor = false;
            this.btnShape.Click += new System.EventHandler(this.btnShape_Click);
            // 
            // btnMirror
            // 
            this.btnMirror.BackColor = System.Drawing.SystemColors.Control;
            this.btnMirror.Location = new System.Drawing.Point(210, 17);
            this.btnMirror.Name = "btnMirror";
            this.btnMirror.Size = new System.Drawing.Size(63, 23);
            this.btnMirror.TabIndex = 15;
            this.btnMirror.Tag = "2";
            this.btnMirror.Text = "Mirror";
            this.btnMirror.UseVisualStyleBackColor = false;
            this.btnMirror.Click += new System.EventHandler(this.btnMirror_Click);
            // 
            // trackBarShape
            // 
            this.trackBarShape.AutoSize = false;
            this.trackBarShape.Location = new System.Drawing.Point(475, 18);
            this.trackBarShape.Maximum = 50;
            this.trackBarShape.Minimum = -50;
            this.trackBarShape.Name = "trackBarShape";
            this.trackBarShape.Size = new System.Drawing.Size(167, 21);
            this.trackBarShape.TabIndex = 16;
            this.trackBarShape.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarShape.Scroll += new System.EventHandler(this.trackBarShape_Scroll);
            this.trackBarShape.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBarShape_MouseDown);
            // 
            // btnUndo
            // 
            this.btnUndo.BackColor = System.Drawing.SystemColors.Control;
            this.btnUndo.Location = new System.Drawing.Point(399, 17);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(63, 23);
            this.btnUndo.TabIndex = 18;
            this.btnUndo.Tag = "2";
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = false;
            this.btnUndo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnUndo_MouseDown);
            this.btnUndo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnUndo_MouseUp);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.Control;
            this.btnSave.Location = new System.Drawing.Point(324, 17);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(63, 23);
            this.btnSave.TabIndex = 17;
            this.btnSave.Tag = "2";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseDown);
            this.btnSave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseUp);
            // 
            // PanelShape
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::RH.HeadShop.Properties.Resources.menuBackground;
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.trackBarShape);
            this.Controls.Add(this.btnMirror);
            this.Controls.Add(this.btnPleat);
            this.Controls.Add(this.btnStretch);
            this.Controls.Add(this.btnShape);
            this.Name = "PanelShape";
            this.Size = new System.Drawing.Size(654, 49);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarShape)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPleat;
        private System.Windows.Forms.Button btnStretch;
        private System.Windows.Forms.Button btnShape;
        private System.Windows.Forms.Button btnMirror;
        private System.Windows.Forms.TrackBar trackBarShape;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnSave;
    }
}
