using System.Windows.Forms;

namespace RH.HeadShop.Controls.Progress
{
    partial class ProgressForm
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
            this.statusLabel =new Label();
            this.progressBar = new ProgressBar();
            this.substatusLabel = new Label();
            this.subprogressBar = new ProgressBar();
            this.remainedTimeLabel = new Label();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(14, 12);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(471, 15);
            this.statusLabel.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(14, 31);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(471, 18);
            this.progressBar.TabIndex = 1;
            // 
            // substatusLabel
            // 
            this.substatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.substatusLabel.Location = new System.Drawing.Point(14, 55);
            this.substatusLabel.Name = "substatusLabel";
            this.substatusLabel.Size = new System.Drawing.Size(471, 15);
            this.substatusLabel.TabIndex = 2;
            // 
            // subprogressBar
            // 
            this.subprogressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subprogressBar.Location = new System.Drawing.Point(14, 74);
            this.subprogressBar.Name = "subprogressBar";
            this.subprogressBar.Size = new System.Drawing.Size(471, 18);
            this.subprogressBar.TabIndex = 3;
            // 
            // remainedTimeLabel
            // 
            this.remainedTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remainedTimeLabel.Location = new System.Drawing.Point(14, 98);
            this.remainedTimeLabel.Name = "remainedTimeLabel";
            this.remainedTimeLabel.Size = new System.Drawing.Size(471, 15);
            this.remainedTimeLabel.TabIndex = 5;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 154);
            this.Controls.Add(this.remainedTimeLabel);
            this.Controls.Add(this.subprogressBar);
            this.Controls.Add(this.substatusLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Operation is performed";
            this.ResumeLayout(false);

        }

        #endregion

        private Label statusLabel;
        private ProgressBar progressBar;
        private Label substatusLabel;
        private ProgressBar subprogressBar;
        private Label remainedTimeLabel;
    }
}