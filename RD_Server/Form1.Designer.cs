namespace RD_Server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            tbIP = new TextBox();
            tbPassword = new TextBox();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(51, 35);
            label1.Name = "label1";
            label1.Size = new Size(21, 19);
            label1.TabIndex = 0;
            label1.Text = "IP";
            // 
            // tbIP
            // 
            tbIP.Location = new Point(51, 57);
            tbIP.Name = "tbIP";
            tbIP.ReadOnly = true;
            tbIP.Size = new Size(230, 25);
            tbIP.TabIndex = 1;
            tbIP.TabStop = false;
            // 
            // tbPassword
            // 
            tbPassword.Location = new Point(51, 136);
            tbPassword.Name = "tbPassword";
            tbPassword.ReadOnly = true;
            tbPassword.Size = new Size(230, 25);
            tbPassword.TabIndex = 3;
            tbPassword.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(51, 114);
            label2.Name = "label2";
            label2.Size = new Size(67, 19);
            label2.TabIndex = 2;
            label2.Text = "Password";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(334, 211);
            Controls.Add(tbPassword);
            Controls.Add(label2);
            Controls.Add(tbIP);
            Controls.Add(label1);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RD_Server";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbIP;
        private TextBox tbPassword;
        private Label label2;
    }
}