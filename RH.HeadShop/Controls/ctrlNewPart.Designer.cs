namespace RH.HeadShop.Controls
{
    partial class ctrlNewPart
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
            this.label1 = new System.Windows.Forms.Label();
            this.textTitle = new System.Windows.Forms.TextBox();
            this.sbOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter part title:";
            // 
            // textTitle
            // 
            this.textTitle.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textTitle.Location = new System.Drawing.Point(104, 7);
            this.textTitle.Name = "textTitle";
            this.textTitle.Size = new System.Drawing.Size(196, 22);
            this.textTitle.TabIndex = 1;
            this.textTitle.Text = "Item1";
            this.textTitle.TextChanged += new System.EventHandler(this.textTitle_TextChanged);
            // 
            // sbOk
            // 
            this.sbOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.sbOk.Location = new System.Drawing.Point(231, 10);
            this.sbOk.Name = "sbOk";
            this.sbOk.Size = new System.Drawing.Size(75, 23);
            this.sbOk.TabIndex = 2;
            this.sbOk.Text = "OK";
            this.sbOk.UseVisualStyleBackColor = true;
            // 
            // ctrlNewPart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sbOk);
            this.Controls.Add(this.textTitle);
            this.Controls.Add(this.label1);
            this.Name = "ctrlNewPart";
            this.Size = new System.Drawing.Size(309, 36);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textTitle;
        public System.Windows.Forms.Button sbOk;
    }
}
