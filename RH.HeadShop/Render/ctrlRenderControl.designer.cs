namespace RH.HeadShop.Render
{
    partial class ctrlRenderControl
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
            this.components = new System.ComponentModel.Container();
            this.RenderTimer = new System.Windows.Forms.Timer(this.components);
            this.workTimer = new System.Windows.Forms.Timer(this.components);
            this.keyTimer = new System.Windows.Forms.Timer(this.components);
            this.panelOrtoBottom = new System.Windows.Forms.Panel();
            this.panelOrtoRight = new System.Windows.Forms.Panel();
            this.panelOrtoLeft = new System.Windows.Forms.Panel();
            this.panelStop = new System.Windows.Forms.Panel();
            this.panelOrtoTop = new System.Windows.Forms.Panel();
            this.glControl = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // RenderTimer
            // 
            this.RenderTimer.Interval = 40;
            this.RenderTimer.Tick += new System.EventHandler(this.RenderTimer_Tick);
            // 
            // workTimer
            // 
            this.workTimer.Interval = 50;
            this.workTimer.Tick += new System.EventHandler(this.workTimer_Tick);
            // 
            // keyTimer
            // 
            this.keyTimer.Interval = 50;
            this.keyTimer.Tick += new System.EventHandler(this.keyTimer_Tick);
            // 
            // panelOrtoBottom
            // 
            this.panelOrtoBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOrtoBottom.BackgroundImage = global::RH.HeadShop.Properties.Resources.panelBottom;
            this.panelOrtoBottom.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelOrtoBottom.Location = new System.Drawing.Point(627, 99);
            this.panelOrtoBottom.Name = "panelOrtoBottom";
            this.panelOrtoBottom.Size = new System.Drawing.Size(45, 45);
            this.panelOrtoBottom.TabIndex = 5;
            this.panelOrtoBottom.Click += new System.EventHandler(this.panelOrtoBottom_Click);
            // 
            // panelOrtoRight
            // 
            this.panelOrtoRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOrtoRight.BackgroundImage = global::RH.HeadShop.Properties.Resources.panelRight;
            this.panelOrtoRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelOrtoRight.Location = new System.Drawing.Point(671, 54);
            this.panelOrtoRight.Name = "panelOrtoRight";
            this.panelOrtoRight.Size = new System.Drawing.Size(45, 45);
            this.panelOrtoRight.TabIndex = 5;
            this.panelOrtoRight.Click += new System.EventHandler(this.panelOrtoRight_Click);
            // 
            // panelOrtoLeft
            // 
            this.panelOrtoLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOrtoLeft.BackgroundImage = global::RH.HeadShop.Properties.Resources.panelLeft;
            this.panelOrtoLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelOrtoLeft.Location = new System.Drawing.Point(581, 55);
            this.panelOrtoLeft.Name = "panelOrtoLeft";
            this.panelOrtoLeft.Size = new System.Drawing.Size(45, 45);
            this.panelOrtoLeft.TabIndex = 5;
            this.panelOrtoLeft.Click += new System.EventHandler(this.panelOrtoLeft_Click);
            // 
            // panelStop
            // 
            this.panelStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStop.BackgroundImage = global::RH.HeadShop.Properties.Resources.panelStop;
            this.panelStop.Location = new System.Drawing.Point(626, 54);
            this.panelStop.Name = "panelStop";
            this.panelStop.Size = new System.Drawing.Size(45, 45);
            this.panelStop.TabIndex = 5;
            this.panelStop.Click += new System.EventHandler(this.panelStop_Click);
            // 
            // panelOrtoTop
            // 
            this.panelOrtoTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOrtoTop.BackgroundImage = global::RH.HeadShop.Properties.Resources.panelTop;
            this.panelOrtoTop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelOrtoTop.Location = new System.Drawing.Point(626, 9);
            this.panelOrtoTop.Name = "panelOrtoTop";
            this.panelOrtoTop.Size = new System.Drawing.Size(45, 45);
            this.panelOrtoTop.TabIndex = 4;
            this.panelOrtoTop.Click += new System.EventHandler(this.panelOrtoTop_Click);
            // 
            // glControl
            // 
            this.glControl.AllowDrop = true;
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(0, 0);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(728, 493);
            this.glControl.TabIndex = 3;
            this.glControl.VSync = false;
            this.glControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.glControl_DragDrop);
            this.glControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.glControl_DragEnter);
            this.glControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyDown);
            this.glControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyUp);
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDown);
            this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseMove);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseUp);
            this.glControl.Resize += new System.EventHandler(this.glControl_Resize);
            // 
            // ctrlRenderControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelOrtoBottom);
            this.Controls.Add(this.panelOrtoRight);
            this.Controls.Add(this.panelOrtoLeft);
            this.Controls.Add(this.panelStop);
            this.Controls.Add(this.panelOrtoTop);
            this.Controls.Add(this.glControl);
            this.Name = "ctrlRenderControl";
            this.Size = new System.Drawing.Size(728, 493);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControl;
        public System.Windows.Forms.Timer workTimer;
        private System.Windows.Forms.Panel panelOrtoTop;
        private System.Windows.Forms.Panel panelStop;
        private System.Windows.Forms.Panel panelOrtoBottom;
        public System.Windows.Forms.Timer keyTimer;
        public System.Windows.Forms.Timer RenderTimer;
        public System.Windows.Forms.Panel panelOrtoLeft;
        public System.Windows.Forms.Panel panelOrtoRight;
    }
}
