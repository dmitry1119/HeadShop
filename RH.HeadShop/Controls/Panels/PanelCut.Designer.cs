namespace RH.HeadShop.Controls.Panels
{
    partial class PanelCut
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
            this.btnCut = new System.Windows.Forms.Button();
            this.btnMirror = new System.Windows.Forms.Button();
            this.btnDuplicate = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLine = new System.Windows.Forms.Button();
            this.btnPolyLine = new System.Windows.Forms.Button();
            this.btnArc = new System.Windows.Forms.Button();
            this.btnLasso = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCut
            // 
            this.btnCut.BackColor = System.Drawing.SystemColors.Control;
            this.btnCut.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCut.Location = new System.Drawing.Point(13, 17);
            this.btnCut.Name = "btnCut";
            this.btnCut.Size = new System.Drawing.Size(63, 23);
            this.btnCut.TabIndex = 0;
            this.btnCut.Tag = "2";
            this.btnCut.Text = "Cut";
            this.btnCut.UseVisualStyleBackColor = false;
            this.btnCut.Click += new System.EventHandler(this.btnCut_Click);
            // 
            // btnMirror
            // 
            this.btnMirror.BackColor = System.Drawing.SystemColors.Control;
            this.btnMirror.Location = new System.Drawing.Point(91, 17);
            this.btnMirror.Name = "btnMirror";
            this.btnMirror.Size = new System.Drawing.Size(63, 23);
            this.btnMirror.TabIndex = 1;
            this.btnMirror.Tag = "2";
            this.btnMirror.Text = "Mirror";
            this.btnMirror.UseVisualStyleBackColor = false;
            this.btnMirror.Click += new System.EventHandler(this.btnMirror_Click);
            // 
            // btnDuplicate
            // 
            this.btnDuplicate.BackColor = System.Drawing.SystemColors.Control;
            this.btnDuplicate.Location = new System.Drawing.Point(170, 17);
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(63, 23);
            this.btnDuplicate.TabIndex = 2;
            this.btnDuplicate.Tag = "2";
            this.btnDuplicate.Text = "Duplicate";
            this.btnDuplicate.UseVisualStyleBackColor = false;
            this.btnDuplicate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnDuplicate_MouseDown);
            this.btnDuplicate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnDuplicate_MouseUp);
            // 
            // btnUndo
            // 
            this.btnUndo.BackColor = System.Drawing.SystemColors.Control;
            this.btnUndo.Location = new System.Drawing.Point(431, 17);
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
            this.btnDelete.Location = new System.Drawing.Point(351, 17);
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
            this.btnSave.Location = new System.Drawing.Point(271, 17);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(63, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Tag = "2";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseDown);
            this.btnSave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnSave_MouseUp);
            // 
            // btnLine
            // 
            this.btnLine.Image = global::RH.HeadShop.Properties.Resources.btnLinePressed;
            this.btnLine.Location = new System.Drawing.Point(624, 10);
            this.btnLine.Name = "btnLine";
            this.btnLine.Size = new System.Drawing.Size(30, 30);
            this.btnLine.TabIndex = 6;
            this.btnLine.Tag = "1";
            this.btnLine.UseVisualStyleBackColor = true;
            this.btnLine.Click += new System.EventHandler(this.btnLine_Click);
            // 
            // btnPolyLine
            // 
            this.btnPolyLine.Image = global::RH.HeadShop.Properties.Resources.btnPolyLineNormal;
            this.btnPolyLine.Location = new System.Drawing.Point(663, 10);
            this.btnPolyLine.Name = "btnPolyLine";
            this.btnPolyLine.Size = new System.Drawing.Size(30, 30);
            this.btnPolyLine.TabIndex = 7;
            this.btnPolyLine.Tag = "2";
            this.btnPolyLine.UseVisualStyleBackColor = true;
            this.btnPolyLine.Click += new System.EventHandler(this.btnPolyLine_Click);
            // 
            // btnArc
            // 
            this.btnArc.Image = global::RH.HeadShop.Properties.Resources.btnArcNormal;
            this.btnArc.Location = new System.Drawing.Point(704, 10);
            this.btnArc.Name = "btnArc";
            this.btnArc.Size = new System.Drawing.Size(30, 30);
            this.btnArc.TabIndex = 8;
            this.btnArc.Tag = "2";
            this.btnArc.UseVisualStyleBackColor = true;
            this.btnArc.Click += new System.EventHandler(this.btnArc_Click);
            // 
            // btnLasso
            // 
            this.btnLasso.BackColor = System.Drawing.SystemColors.Control;
            this.btnLasso.Location = new System.Drawing.Point(529, 17);
            this.btnLasso.Name = "btnLasso";
            this.btnLasso.Size = new System.Drawing.Size(63, 23);
            this.btnLasso.TabIndex = 9;
            this.btnLasso.Tag = "2";
            this.btnLasso.Text = "Lasso";
            this.btnLasso.UseVisualStyleBackColor = false;
            this.btnLasso.Click += new System.EventHandler(this.btnLasso_Click);
            // 
            // PanelCut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::RH.HeadShop.Properties.Resources.menuBackground;
            this.Controls.Add(this.btnLasso);
            this.Controls.Add(this.btnArc);
            this.Controls.Add(this.btnPolyLine);
            this.Controls.Add(this.btnLine);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnDuplicate);
            this.Controls.Add(this.btnMirror);
            this.Controls.Add(this.btnCut);
            this.Name = "PanelCut";
            this.Size = new System.Drawing.Size(743, 49);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCut;
        private System.Windows.Forms.Button btnMirror;
        private System.Windows.Forms.Button btnDuplicate;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLine;
        private System.Windows.Forms.Button btnPolyLine;
        private System.Windows.Forms.Button btnArc;
        private System.Windows.Forms.Button btnLasso;
    }
}
