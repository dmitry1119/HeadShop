namespace RH.HeadShop.Controls
{
    partial class frmNewProject2
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
            this.textName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureTemplate = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbImportObj = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChild = new System.Windows.Forms.PictureBox();
            this.btnFemale = new System.Windows.Forms.PictureBox();
            this.btnMale = new System.Windows.Forms.PictureBox();
            this.btnInfo = new System.Windows.Forms.PictureBox();
            this.btnPlay = new System.Windows.Forms.PictureBox();
            this.btnQuestion = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.RenderTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureTemplate)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnChild)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnFemale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnMale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnQuestion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textName
            // 
            this.textName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textName.Location = new System.Drawing.Point(91, 12);
            this.textName.Name = "textName";
            this.textName.ReadOnly = true;
            this.textName.Size = new System.Drawing.Size(716, 24);
            this.textName.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(13, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Name";
            // 
            // pictureTemplate
            // 
            this.pictureTemplate.Location = new System.Drawing.Point(12, 42);
            this.pictureTemplate.Name = "pictureTemplate";
            this.pictureTemplate.Size = new System.Drawing.Size(795, 459);
            this.pictureTemplate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureTemplate.TabIndex = 15;
            this.pictureTemplate.TabStop = false;
            this.pictureTemplate.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureTemplate_Paint);
            this.pictureTemplate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureTemplate_MouseDown);
            this.pictureTemplate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureTemplate_MouseMove);
            this.pictureTemplate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureTemplate_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::RH.HeadShop.Properties.Resources.bgWizard2;
            this.panel1.Controls.Add(this.rbImportObj);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnChild);
            this.panel1.Controls.Add(this.btnFemale);
            this.panel1.Controls.Add(this.btnMale);
            this.panel1.Controls.Add(this.btnInfo);
            this.panel1.Controls.Add(this.btnPlay);
            this.panel1.Controls.Add(this.btnQuestion);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnNext);
            this.panel1.Location = new System.Drawing.Point(12, 511);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(795, 100);
            this.panel1.TabIndex = 14;
            // 
            // rbImportObj
            // 
            this.rbImportObj.AutoSize = true;
            this.rbImportObj.BackColor = System.Drawing.Color.Transparent;
            this.rbImportObj.Location = new System.Drawing.Point(528, 56);
            this.rbImportObj.Name = "rbImportObj";
            this.rbImportObj.Size = new System.Drawing.Size(14, 13);
            this.rbImportObj.TabIndex = 19;
            this.rbImportObj.TabStop = true;
            this.rbImportObj.UseVisualStyleBackColor = false;
            this.rbImportObj.CheckedChanged += new System.EventHandler(this.rbImportObj_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(489, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 18);
            this.label1.TabIndex = 18;
            this.label1.Text = "Import OBJ";
            // 
            // btnChild
            // 
            this.btnChild.Image = global::RH.HeadShop.Properties.Resources.btnChildGray;
            this.btnChild.Location = new System.Drawing.Point(390, 34);
            this.btnChild.Name = "btnChild";
            this.btnChild.Size = new System.Drawing.Size(59, 59);
            this.btnChild.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnChild.TabIndex = 17;
            this.btnChild.TabStop = false;
            this.btnChild.Tag = "2";
            this.btnChild.Click += new System.EventHandler(this.btnChild_Click);
            // 
            // btnFemale
            // 
            this.btnFemale.Image = global::RH.HeadShop.Properties.Resources.btnFemaleGray;
            this.btnFemale.Location = new System.Drawing.Point(285, 34);
            this.btnFemale.Name = "btnFemale";
            this.btnFemale.Size = new System.Drawing.Size(59, 59);
            this.btnFemale.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnFemale.TabIndex = 16;
            this.btnFemale.TabStop = false;
            this.btnFemale.Tag = "2";
            this.btnFemale.Click += new System.EventHandler(this.btnFemale_Click);
            // 
            // btnMale
            // 
            this.btnMale.Image = global::RH.HeadShop.Properties.Resources.btnMaleNormal;
            this.btnMale.Location = new System.Drawing.Point(184, 34);
            this.btnMale.Name = "btnMale";
            this.btnMale.Size = new System.Drawing.Size(59, 59);
            this.btnMale.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnMale.TabIndex = 15;
            this.btnMale.TabStop = false;
            this.btnMale.Tag = "1";
            this.btnMale.Click += new System.EventHandler(this.btnMale_Click);
            // 
            // btnInfo
            // 
            this.btnInfo.Image = global::RH.HeadShop.Properties.Resources.btnInfoNormal;
            this.btnInfo.Location = new System.Drawing.Point(731, 11);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(34, 34);
            this.btnInfo.TabIndex = 14;
            this.btnInfo.TabStop = false;
            this.btnInfo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnInfo_MouseDown);
            this.btnInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnInfo_MouseUp);
            // 
            // btnPlay
            // 
            this.btnPlay.Image = global::RH.HeadShop.Properties.Resources.btnPlayNormal;
            this.btnPlay.Location = new System.Drawing.Point(682, 11);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(34, 34);
            this.btnPlay.TabIndex = 13;
            this.btnPlay.TabStop = false;
            this.btnPlay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnPlay_MouseDown);
            this.btnPlay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnPlay_MouseUp);
            // 
            // btnQuestion
            // 
            this.btnQuestion.Image = global::RH.HeadShop.Properties.Resources.btnQuestionNormal;
            this.btnQuestion.Location = new System.Drawing.Point(633, 11);
            this.btnQuestion.Name = "btnQuestion";
            this.btnQuestion.Size = new System.Drawing.Size(34, 34);
            this.btnQuestion.TabIndex = 12;
            this.btnQuestion.TabStop = false;
            this.btnQuestion.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnQuestion_MouseDown);
            this.btnQuestion.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnQuestion_MouseUp);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image = global::RH.HeadShop.Properties.Resources.splitter;
            this.pictureBox2.Location = new System.Drawing.Point(607, -4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(10, 97);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::RH.HeadShop.Properties.Resources.splitter;
            this.pictureBox1.Location = new System.Drawing.Point(165, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(10, 97);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(396, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 18);
            this.label6.TabIndex = 7;
            this.label6.Text = "Child";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(281, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Female";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(191, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "Male";
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.SystemColors.Control;
            this.btnNext.Location = new System.Drawing.Point(694, 56);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(95, 37);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // RenderTimer
            // 
            this.RenderTimer.Interval = 40;
            this.RenderTimer.Tick += new System.EventHandler(this.RenderTimer_Tick);
            // 
            // frmNewProject2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 623);
            this.Controls.Add(this.pictureTemplate);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textName);
            this.Controls.Add(this.label5);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNewProject2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Properties";
            this.Load += new System.EventHandler(this.frmNewProject2_Load);
            this.Resize += new System.EventHandler(this.frmNewProject2_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureTemplate)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnChild)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnFemale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnMale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnQuestion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureTemplate;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox btnQuestion;
        private System.Windows.Forms.PictureBox btnInfo;
        private System.Windows.Forms.PictureBox btnPlay;
        private System.Windows.Forms.PictureBox btnMale;
        private System.Windows.Forms.PictureBox btnChild;
        private System.Windows.Forms.PictureBox btnFemale;
        public System.Windows.Forms.Timer RenderTimer;
        private System.Windows.Forms.RadioButton rbImportObj;
        private System.Windows.Forms.Label label1;
    }
}