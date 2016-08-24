using System;

namespace RH.HeadShop.Controls
{
    partial class frmNewProject1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNewProject1));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textProjectName = new System.Windows.Forms.TextBox();
            this.textProjectFolder = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnOpenFolderDlg = new System.Windows.Forms.Button();
            this.btnOpenFileDlg = new System.Windows.Forms.Button();
            this.textTemplateImage = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureTemplate = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupLoadProject = new System.Windows.Forms.GroupBox();
            this.btnLoadProject = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textLoadProject = new System.Windows.Forms.TextBox();
            this.rbNew = new System.Windows.Forms.RadioButton();
            this.rbSaved = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTemplate)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupLoadProject.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(6, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(424, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select template jpg image(image you want to use as a template).";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(6, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(440, 54);
            this.label2.TabIndex = 3;
            this.label2.Text = "You are about to start a new project. Please enter a new project name and select " +
                "a location for the project folder.\r\nAll project items will be saved in this new " +
                "folder.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(6, 176);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Project name";
            // 
            // textProjectName
            // 
            this.textProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textProjectName.Location = new System.Drawing.Point(106, 172);
            this.textProjectName.Name = "textProjectName";
            this.textProjectName.Size = new System.Drawing.Size(297, 24);
            this.textProjectName.TabIndex = 5;
            // 
            // textProjectFolder
            // 
            this.textProjectFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textProjectFolder.Location = new System.Drawing.Point(106, 202);
            this.textProjectFolder.Name = "textProjectFolder";
            this.textProjectFolder.ReadOnly = true;
            this.textProjectFolder.Size = new System.Drawing.Size(297, 24);
            this.textProjectFolder.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(6, 206);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Project folder";
            // 
            // btnApply
            // 
            this.btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnApply.Location = new System.Drawing.Point(477, 345);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(104, 35);
            this.btnApply.TabIndex = 8;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnOpenFolderDlg
            // 
            this.btnOpenFolderDlg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOpenFolderDlg.Location = new System.Drawing.Point(409, 202);
            this.btnOpenFolderDlg.Name = "btnOpenFolderDlg";
            this.btnOpenFolderDlg.Size = new System.Drawing.Size(33, 24);
            this.btnOpenFolderDlg.TabIndex = 9;
            this.btnOpenFolderDlg.Text = "...";
            this.btnOpenFolderDlg.UseVisualStyleBackColor = true;
            this.btnOpenFolderDlg.Click += new System.EventHandler(this.btnOpenFolderDlg_Click);
            // 
            // btnOpenFileDlg
            // 
            this.btnOpenFileDlg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOpenFileDlg.Location = new System.Drawing.Point(409, 64);
            this.btnOpenFileDlg.Name = "btnOpenFileDlg";
            this.btnOpenFileDlg.Size = new System.Drawing.Size(33, 24);
            this.btnOpenFileDlg.TabIndex = 12;
            this.btnOpenFileDlg.Text = "...";
            this.btnOpenFileDlg.UseVisualStyleBackColor = true;
            this.btnOpenFileDlg.Click += new System.EventHandler(this.btnOpenFileDlg_Click);
            // 
            // textTemplateImage
            // 
            this.textTemplateImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textTemplateImage.Location = new System.Drawing.Point(106, 64);
            this.textTemplateImage.Name = "textTemplateImage";
            this.textTemplateImage.ReadOnly = true;
            this.textTemplateImage.Size = new System.Drawing.Size(297, 24);
            this.textTemplateImage.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(6, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "Template";
            // 
            // pictureTemplate
            // 
            this.pictureTemplate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureTemplate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureTemplate.Location = new System.Drawing.Point(12, 12);
            this.pictureTemplate.Name = "pictureTemplate";
            this.pictureTemplate.Size = new System.Drawing.Size(281, 375);
            this.pictureTemplate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureTemplate.TabIndex = 0;
            this.pictureTemplate.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnOpenFileDlg);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textTemplateImage);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textProjectName);
            this.groupBox1.Controls.Add(this.btnOpenFolderDlg);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textProjectFolder);
            this.groupBox1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(312, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 236);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create new project";
            // 
            // groupLoadProject
            // 
            this.groupLoadProject.Controls.Add(this.btnLoadProject);
            this.groupLoadProject.Controls.Add(this.label6);
            this.groupLoadProject.Controls.Add(this.textLoadProject);
            this.groupLoadProject.Enabled = false;
            this.groupLoadProject.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupLoadProject.Location = new System.Drawing.Point(312, 282);
            this.groupLoadProject.Name = "groupLoadProject";
            this.groupLoadProject.Size = new System.Drawing.Size(449, 57);
            this.groupLoadProject.TabIndex = 14;
            this.groupLoadProject.TabStop = false;
            this.groupLoadProject.Text = "Load project";
            // 
            // btnLoadProject
            // 
            this.btnLoadProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnLoadProject.Location = new System.Drawing.Point(409, 23);
            this.btnLoadProject.Name = "btnLoadProject";
            this.btnLoadProject.Size = new System.Drawing.Size(33, 24);
            this.btnLoadProject.TabIndex = 12;
            this.btnLoadProject.Text = "...";
            this.btnLoadProject.UseVisualStyleBackColor = true;
            this.btnLoadProject.Click += new System.EventHandler(this.btnLoadProject_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(6, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 17);
            this.label6.TabIndex = 10;
            this.label6.Text = "Project file";
            // 
            // textLoadProject
            // 
            this.textLoadProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.textLoadProject.Location = new System.Drawing.Point(106, 23);
            this.textLoadProject.Name = "textLoadProject";
            this.textLoadProject.ReadOnly = true;
            this.textLoadProject.Size = new System.Drawing.Size(297, 24);
            this.textLoadProject.TabIndex = 11;
            // 
            // rbNew
            // 
            this.rbNew.AutoSize = true;
            this.rbNew.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbNew.Checked = true;
            this.rbNew.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold);
            this.rbNew.Location = new System.Drawing.Point(321, 254);
            this.rbNew.Name = "rbNew";
            this.rbNew.Size = new System.Drawing.Size(106, 21);
            this.rbNew.TabIndex = 15;
            this.rbNew.TabStop = true;
            this.rbNew.Text = "New Project";
            this.rbNew.UseVisualStyleBackColor = true;
            this.rbNew.CheckedChanged += new System.EventHandler(this.rbNew_CheckedChanged);
            // 
            // rbSaved
            // 
            this.rbSaved.AutoSize = true;
            this.rbSaved.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbSaved.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold);
            this.rbSaved.Location = new System.Drawing.Point(481, 254);
            this.rbSaved.Name = "rbSaved";
            this.rbSaved.Size = new System.Drawing.Size(156, 21);
            this.rbSaved.TabIndex = 16;
            this.rbSaved.Text = "Open Saved Project";
            this.rbSaved.UseVisualStyleBackColor = true;
            // 
            // frmNewProject1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 395);
            this.Controls.Add(this.rbSaved);
            this.Controls.Add(this.rbNew);
            this.Controls.Add(this.groupLoadProject);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.pictureTemplate);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(772, 424);
            this.Name = "frmNewProject1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new or load project";
            ((System.ComponentModel.ISupportInitialize)(this.pictureTemplate)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupLoadProject.ResumeLayout(false);
            this.groupLoadProject.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureTemplate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textProjectName;
        private System.Windows.Forms.TextBox textProjectFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnOpenFolderDlg;
        private System.Windows.Forms.Button btnOpenFileDlg;
        private System.Windows.Forms.TextBox textTemplateImage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupLoadProject;
        private System.Windows.Forms.Button btnLoadProject;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textLoadProject;
        private System.Windows.Forms.RadioButton rbNew;
        private System.Windows.Forms.RadioButton rbSaved;
    }
}