using RH.HeadShop.Helpers;

namespace RH.HeadShop.Controls.Libraries
{
    partial class frmParts
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node0");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node1");
            this.labelEmpty = new System.Windows.Forms.Label();
            this.tlParts = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLoadLibrary = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBarSize = new System.Windows.Forms.TrackBar();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).BeginInit();
            this.SuspendLayout();
            // 
            // labelEmpty
            // 
            this.labelEmpty.AutoSize = true;
            this.labelEmpty.BackColor = System.Drawing.Color.White;
            this.labelEmpty.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelEmpty.Location = new System.Drawing.Point(12, 9);
            this.labelEmpty.Name = "labelEmpty";
            this.labelEmpty.Size = new System.Drawing.Size(217, 15);
            this.labelEmpty.TabIndex = 1;
            this.labelEmpty.Text = "There are no items to show in this view.";
            // 
            // tlParts
            // 
            this.tlParts.CheckBoxes = true;
            this.tlParts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlParts.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tlParts.Location = new System.Drawing.Point(0, 0);
            this.tlParts.Name = "tlParts";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Node0";
            treeNode2.Name = "Node1";
            treeNode2.Text = "Node1";
            this.tlParts.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.tlParts.ShowLines = false;
            this.tlParts.ShowRootLines = false;
            this.tlParts.Size = new System.Drawing.Size(291, 259);
            this.tlParts.TabIndex = 2;
            this.tlParts.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tlParts_AfterCheck);
            this.tlParts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tlParts_AfterSelect);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnLoadLibrary);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.trackBarSize);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(291, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(92, 259);
            this.panel1.TabIndex = 3;
            // 
            // btnLoadLibrary
            // 
            this.btnLoadLibrary.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnLoadLibrary.Location = new System.Drawing.Point(2, 224);
            this.btnLoadLibrary.Name = "btnLoadLibrary";
            this.btnLoadLibrary.Size = new System.Drawing.Size(87, 30);
            this.btnLoadLibrary.TabIndex = 13;
            this.btnLoadLibrary.Text = "Load library";
            this.btnLoadLibrary.UseVisualStyleBackColor = true;
            this.btnLoadLibrary.Click += new System.EventHandler(this.btnLoadLibrary_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(15, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Size";
            // 
            // trackBarSize
            // 
            this.trackBarSize.Location = new System.Drawing.Point(23, 33);
            this.trackBarSize.Maximum = 20;
            this.trackBarSize.Minimum = 1;
            this.trackBarSize.Name = "trackBarSize";
            this.trackBarSize.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarSize.Size = new System.Drawing.Size(40, 163);
            this.trackBarSize.TabIndex = 0;
            this.trackBarSize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarSize.Value = 10;
            this.trackBarSize.Scroll += new System.EventHandler(this.trackBarSize_Scroll);
            this.trackBarSize.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBarSize_MouseDown);
            // 
            // frmParts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 259);
            this.Controls.Add(this.tlParts);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelEmpty);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmParts";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Parts library";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.frmParts_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmParts_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelEmpty;
        private System.Windows.Forms.TreeView tlParts;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.Button btnLoadLibrary;
    }
}