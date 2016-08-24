namespace RH.HeadShop.Controls.Panels
{
    partial class PanelHead
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
            this.btnMirror = new System.Windows.Forms.Button();
            this.btnAutodots = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDots = new System.Windows.Forms.Button();
            this.btnPolyLine = new System.Windows.Forms.Button();
            this.btnShapeTool = new System.Windows.Forms.Button();
            this.btnLasso = new System.Windows.Forms.Button();
            this.btnFlipLeft = new System.Windows.Forms.Button();
            this.btnFlipRight = new System.Windows.Forms.Button();
            this.btnNewPict = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMirror
            // 
            this.btnMirror.BackColor = System.Drawing.SystemColors.Control;
            this.btnMirror.Location = new System.Drawing.Point(18, 16);
            this.btnMirror.Name = "btnMirror";
            this.btnMirror.Size = new System.Drawing.Size(63, 23);
            this.btnMirror.TabIndex = 1;
            this.btnMirror.Tag = "2";
            this.btnMirror.Text = "Mirror";
            this.btnMirror.UseVisualStyleBackColor = false;
            this.btnMirror.Click += new System.EventHandler(this.btnMirror_Click);
            // 
            // btnAutodots
            // 
            this.btnAutodots.BackColor = System.Drawing.SystemColors.Control;
            this.btnAutodots.Location = new System.Drawing.Point(97, 16);
            this.btnAutodots.Name = "btnAutodots";
            this.btnAutodots.Size = new System.Drawing.Size(63, 23);
            this.btnAutodots.TabIndex = 2;
            this.btnAutodots.Tag = "2";
            this.btnAutodots.Text = "Autodots";
            this.btnAutodots.UseVisualStyleBackColor = false;
            this.btnAutodots.Click += new System.EventHandler(this.btnAutodots_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.BackColor = System.Drawing.SystemColors.Control;
            this.btnUndo.Location = new System.Drawing.Point(358, 16);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(63, 23);
            this.btnUndo.TabIndex = 5;
            this.btnUndo.Tag = "2";
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = false;
            this.btnUndo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnUndo_MouseDown);
            this.btnUndo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnUndo_MouseUp);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.Control;
            this.btnDelete.Location = new System.Drawing.Point(278, 16);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(63, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Tag = "2";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnDelete_MouseDown);
            this.btnDelete.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnDelete_MouseUp);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.Control;
            this.btnSave.Location = new System.Drawing.Point(198, 16);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(63, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Tag = "2";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseDown);
            this.btnSave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseUp);
            // 
            // btnDots
            // 
            this.btnDots.Enabled = false;
            this.btnDots.Image = global::RH.HeadShop.Properties.Resources.btnDotsNormal;
            this.btnDots.Location = new System.Drawing.Point(551, 9);
            this.btnDots.Name = "btnDots";
            this.btnDots.Size = new System.Drawing.Size(30, 30);
            this.btnDots.TabIndex = 6;
            this.btnDots.Tag = "2";
            this.btnDots.UseVisualStyleBackColor = true;
            this.btnDots.Click += new System.EventHandler(this.btnDots_Click);
            // 
            // btnPolyLine
            // 
            this.btnPolyLine.Enabled = false;
            this.btnPolyLine.Image = global::RH.HeadShop.Properties.Resources.btnPolyLineNormal;
            this.btnPolyLine.Location = new System.Drawing.Point(590, 9);
            this.btnPolyLine.Name = "btnPolyLine";
            this.btnPolyLine.Size = new System.Drawing.Size(30, 30);
            this.btnPolyLine.TabIndex = 7;
            this.btnPolyLine.Tag = "2";
            this.btnPolyLine.UseVisualStyleBackColor = true;
            this.btnPolyLine.Click += new System.EventHandler(this.btnPolyLine_Click);
            // 
            // btnShapeTool
            // 
            this.btnShapeTool.Enabled = false;
            this.btnShapeTool.Image = global::RH.HeadShop.Properties.Resources.btnHandNormal1;
            this.btnShapeTool.Location = new System.Drawing.Point(631, 9);
            this.btnShapeTool.Name = "btnShapeTool";
            this.btnShapeTool.Size = new System.Drawing.Size(30, 30);
            this.btnShapeTool.TabIndex = 8;
            this.btnShapeTool.Tag = "2";
            this.btnShapeTool.UseVisualStyleBackColor = true;
            this.btnShapeTool.Click += new System.EventHandler(this.btnShapeTool_Click);
            // 
            // btnLasso
            // 
            this.btnLasso.BackColor = System.Drawing.SystemColors.Control;
            this.btnLasso.Location = new System.Drawing.Point(456, 16);
            this.btnLasso.Name = "btnLasso";
            this.btnLasso.Size = new System.Drawing.Size(63, 23);
            this.btnLasso.TabIndex = 9;
            this.btnLasso.Tag = "2";
            this.btnLasso.Text = "Lasso";
            this.btnLasso.UseVisualStyleBackColor = false;
            this.btnLasso.Click += new System.EventHandler(this.btnLasso_Click);
            // 
            // btnFlipLeft
            // 
            this.btnFlipLeft.Image = global::RH.HeadShop.Properties.Resources.btnToRightNormal;
            this.btnFlipLeft.Location = new System.Drawing.Point(694, 9);
            this.btnFlipLeft.Name = "btnFlipLeft";
            this.btnFlipLeft.Size = new System.Drawing.Size(30, 30);
            this.btnFlipLeft.TabIndex = 10;
            this.btnFlipLeft.Tag = "2";
            this.btnFlipLeft.UseVisualStyleBackColor = true;
            this.btnFlipLeft.Click += new System.EventHandler(this.btnFlipLeft_Click);
            // 
            // btnFlipRight
            // 
            this.btnFlipRight.Image = global::RH.HeadShop.Properties.Resources.btnToLeftNormal;
            this.btnFlipRight.Location = new System.Drawing.Point(734, 9);
            this.btnFlipRight.Name = "btnFlipRight";
            this.btnFlipRight.Size = new System.Drawing.Size(30, 30);
            this.btnFlipRight.TabIndex = 11;
            this.btnFlipRight.Tag = "2";
            this.btnFlipRight.UseVisualStyleBackColor = true;
            this.btnFlipRight.Click += new System.EventHandler(this.btnFlipRight_Click);
            // 
            // btnNewPict
            // 
            this.btnNewPict.BackColor = System.Drawing.SystemColors.Control;
            this.btnNewPict.Location = new System.Drawing.Point(97, 16);
            this.btnNewPict.Name = "btnNewPict";
            this.btnNewPict.Size = new System.Drawing.Size(63, 23);
            this.btnNewPict.TabIndex = 12;
            this.btnNewPict.Tag = "2";
            this.btnNewPict.Text = "New Pict";
            this.btnNewPict.UseVisualStyleBackColor = false;
            this.btnNewPict.Visible = false;
            this.btnNewPict.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnNewPict_MouseDown);
            this.btnNewPict.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnNewPict_MouseUp);
            // 
            // PanelHead
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::RH.HeadShop.Properties.Resources.menuBackground;
            this.Controls.Add(this.btnNewPict);
            this.Controls.Add(this.btnFlipRight);
            this.Controls.Add(this.btnFlipLeft);
            this.Controls.Add(this.btnLasso);
            this.Controls.Add(this.btnShapeTool);
            this.Controls.Add(this.btnPolyLine);
            this.Controls.Add(this.btnDots);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAutodots);
            this.Controls.Add(this.btnMirror);
            this.Name = "PanelHead";
            this.Size = new System.Drawing.Size(994, 49);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMirror;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDots;
        private System.Windows.Forms.Button btnPolyLine;
        private System.Windows.Forms.Button btnShapeTool;
        private System.Windows.Forms.Button btnLasso;
        private System.Windows.Forms.Button btnFlipLeft;
        private System.Windows.Forms.Button btnFlipRight;
        private System.Windows.Forms.Button btnNewPict;
        public System.Windows.Forms.Button btnAutodots;
    }
}
