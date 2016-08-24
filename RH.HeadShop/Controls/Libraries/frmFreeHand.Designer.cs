namespace RH.HeadShop.Controls.Libraries
{
    partial class frmFreeHand
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
            this.trackRadius = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.rbHandBrush1 = new System.Windows.Forms.RadioButton();
            this.rbHandBrush2 = new System.Windows.Forms.RadioButton();
            this.rbHandBrush3 = new System.Windows.Forms.RadioButton();
            this.rbHandBrush4 = new System.Windows.Forms.RadioButton();
            this.cbMirror = new System.Windows.Forms.CheckBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // trackRadius
            // 
            this.trackRadius.AutoSize = false;
            this.trackRadius.Location = new System.Drawing.Point(2, 25);
            this.trackRadius.Maximum = 100;
            this.trackRadius.Minimum = 1;
            this.trackRadius.Name = "trackRadius";
            this.trackRadius.Size = new System.Drawing.Size(98, 30);
            this.trackRadius.TabIndex = 0;
            this.trackRadius.TickFrequency = 10;
            this.trackRadius.Value = 50;
            this.trackRadius.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackRadius_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Range";
            // 
            // rbHandBrush1
            // 
            this.rbHandBrush1.AutoSize = true;
            this.rbHandBrush1.Checked = true;
            this.rbHandBrush1.Location = new System.Drawing.Point(12, 63);
            this.rbHandBrush1.Name = "rbHandBrush1";
            this.rbHandBrush1.Size = new System.Drawing.Size(14, 13);
            this.rbHandBrush1.TabIndex = 2;
            this.rbHandBrush1.TabStop = true;
            this.rbHandBrush1.UseVisualStyleBackColor = true;
            this.rbHandBrush1.CheckedChanged += new System.EventHandler(this.handBrush_CheckedChanged);
            // 
            // rbHandBrush2
            // 
            this.rbHandBrush2.AutoSize = true;
            this.rbHandBrush2.Location = new System.Drawing.Point(12, 82);
            this.rbHandBrush2.Name = "rbHandBrush2";
            this.rbHandBrush2.Size = new System.Drawing.Size(14, 13);
            this.rbHandBrush2.TabIndex = 3;
            this.rbHandBrush2.UseVisualStyleBackColor = true;
            this.rbHandBrush2.CheckedChanged += new System.EventHandler(this.handBrush_CheckedChanged);
            // 
            // rbHandBrush3
            // 
            this.rbHandBrush3.AutoSize = true;
            this.rbHandBrush3.Location = new System.Drawing.Point(12, 103);
            this.rbHandBrush3.Name = "rbHandBrush3";
            this.rbHandBrush3.Size = new System.Drawing.Size(14, 13);
            this.rbHandBrush3.TabIndex = 4;
            this.rbHandBrush3.UseVisualStyleBackColor = true;
            this.rbHandBrush3.CheckedChanged += new System.EventHandler(this.handBrush_CheckedChanged);
            // 
            // rbHandBrush4
            // 
            this.rbHandBrush4.AutoSize = true;
            this.rbHandBrush4.Location = new System.Drawing.Point(12, 124);
            this.rbHandBrush4.Name = "rbHandBrush4";
            this.rbHandBrush4.Size = new System.Drawing.Size(14, 13);
            this.rbHandBrush4.TabIndex = 5;
            this.rbHandBrush4.UseVisualStyleBackColor = true;
            this.rbHandBrush4.CheckedChanged += new System.EventHandler(this.handBrush_CheckedChanged);
            // 
            // cbMirror
            // 
            this.cbMirror.AutoSize = true;
            this.cbMirror.Location = new System.Drawing.Point(16, 145);
            this.cbMirror.Name = "cbMirror";
            this.cbMirror.Size = new System.Drawing.Size(52, 17);
            this.cbMirror.TabIndex = 10;
            this.cbMirror.Text = "Mirror";
            this.cbMirror.UseVisualStyleBackColor = true;
            this.cbMirror.CheckedChanged += new System.EventHandler(this.cbMirror_CheckedChanged);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::RH.HeadShop.Properties.Resources.handBrush4;
            this.pictureBox4.Location = new System.Drawing.Point(45, 124);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(35, 15);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 9;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::RH.HeadShop.Properties.Resources.handBrush3;
            this.pictureBox3.Location = new System.Drawing.Point(45, 103);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(35, 15);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 8;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::RH.HeadShop.Properties.Resources.handBrush2;
            this.pictureBox2.Location = new System.Drawing.Point(45, 82);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(35, 15);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 7;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::RH.HeadShop.Properties.Resources.handBrush1;
            this.pictureBox1.Location = new System.Drawing.Point(45, 61);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(35, 15);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // frmFreeHand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(101, 164);
            this.Controls.Add(this.cbMirror);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.rbHandBrush4);
            this.Controls.Add(this.rbHandBrush3);
            this.Controls.Add(this.rbHandBrush2);
            this.Controls.Add(this.rbHandBrush1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackRadius);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmFreeHand";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Freehand";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFreeHand_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.trackRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbHandBrush1;
        private System.Windows.Forms.RadioButton rbHandBrush2;
        private System.Windows.Forms.RadioButton rbHandBrush3;
        private System.Windows.Forms.RadioButton rbHandBrush4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TrackBar trackRadius;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        public System.Windows.Forms.CheckBox cbMirror;
    }
}