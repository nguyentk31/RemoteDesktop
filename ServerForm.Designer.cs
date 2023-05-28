namespace RemoteDesktop
{
    partial class fServer
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
            label1 = new Label();
            tbIP = new TextBox();
            tbPW = new TextBox();
            label2 = new Label();
            tbST = new TextBox();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 40);
            label1.Name = "label1";
            label1.Size = new Size(98, 19);
            label1.TabIndex = 0;
            label1.Text = "IP ADDRESS:";
            // 
            // tbIP
            // 
            tbIP.Cursor = Cursors.IBeam;
            tbIP.Location = new Point(42, 62);
            tbIP.Name = "tbIP";
            tbIP.ReadOnly = true;
            tbIP.Size = new Size(183, 26);
            tbIP.TabIndex = 1;
            tbIP.TabStop = false;
            // 
            // tbPW
            // 
            tbPW.Cursor = Cursors.IBeam;
            tbPW.Location = new Point(144, 159);
            tbPW.Name = "tbPW";
            tbPW.ReadOnly = true;
            tbPW.Size = new Size(183, 26);
            tbPW.TabIndex = 2;
            tbPW.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(144, 137);
            label2.Name = "label2";
            label2.Size = new Size(97, 19);
            label2.TabIndex = 2;
            label2.Text = "PASSWORD:";
            // 
            // tbST
            // 
            tbST.Cursor = Cursors.IBeam;
            tbST.Location = new Point(260, 239);
            tbST.Name = "tbST";
            tbST.ReadOnly = true;
            tbST.Size = new Size(183, 26);
            tbST.TabIndex = 3;
            tbST.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(260, 217);
            label3.Name = "label3";
            label3.Size = new Size(67, 19);
            label3.TabIndex = 4;
            label3.Text = "STATUS:";
            // 
            // fServer
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 307);
            Controls.Add(tbST);
            Controls.Add(label3);
            Controls.Add(tbPW);
            Controls.Add(label2);
            Controls.Add(tbIP);
            Controls.Add(label1);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "fServer";
            Text = "Server";
            FormClosed += fServer_FormClosed;
            Load += fServer_Load;
            Activated += fServer_Activated;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbIP;
        private TextBox tbPW;
        private Label label2;
        private TextBox tbST;
        private Label label3;
    }
}