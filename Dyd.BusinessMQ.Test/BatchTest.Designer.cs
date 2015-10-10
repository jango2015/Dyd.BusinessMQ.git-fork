namespace Dyd.BusinessMQ.Test
{
    partial class BatchTest
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tbcount = new System.Windows.Forms.TextBox();
            this.tbbingxing = new System.Windows.Forms.TextBox();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(132, 68);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "批量发送";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(383, 21);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "测试发送";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbcount
            // 
            this.tbcount.Location = new System.Drawing.Point(162, 21);
            this.tbcount.Name = "tbcount";
            this.tbcount.Size = new System.Drawing.Size(100, 21);
            this.tbcount.TabIndex = 2;
            this.tbcount.Text = "1000";
            // 
            // tbbingxing
            // 
            this.tbbingxing.Location = new System.Drawing.Point(24, 21);
            this.tbbingxing.Name = "tbbingxing";
            this.tbbingxing.Size = new System.Drawing.Size(100, 21);
            this.tbbingxing.TabIndex = 2;
            this.tbbingxing.Text = "10";
            // 
            // tbPath
            // 
            this.tbPath.Location = new System.Drawing.Point(277, 21);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(100, 21);
            this.tbPath.TabIndex = 3;
            this.tbPath.Text = "dyd.mytest2";
            // 
            // BatchTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 141);
            this.Controls.Add(this.tbPath);
            this.Controls.Add(this.tbbingxing);
            this.Controls.Add(this.tbcount);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "BatchTest";
            this.Text = "BatchTest";
            this.Load += new System.EventHandler(this.BatchTest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbcount;
        private System.Windows.Forms.TextBox tbbingxing;
        private System.Windows.Forms.TextBox tbPath;
    }
}