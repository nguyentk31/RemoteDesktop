namespace RemoteDesktop
{
    partial class fConnection
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
            btConnect = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(29, 30);
            label1.Name = "label1";
            label1.Size = new Size(165, 19);
            label1.TabIndex = 0;
            label1.Text = "REMOTE IP ADDRESS:";
            // 
            // tbIP
            // 
            tbIP.Cursor = Cursors.IBeam;
            tbIP.Location = new Point(29, 52);
            tbIP.Name = "tbIP";
            tbIP.Size = new Size(207, 26);
            tbIP.TabIndex = 1;
            // 
            // tbPW
            // 
            tbPW.Cursor = Cursors.IBeam;
            tbPW.Location = new Point(29, 117);
            tbPW.Name = "tbPW";
            tbPW.Size = new Size(207, 26);
            tbPW.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 95);
            label2.Name = "label2";
            label2.Size = new Size(97, 19);
            label2.TabIndex = 2;
            label2.Text = "PASSWORD:";
            // 
            // btConnect
            // 
            btConnect.Location = new Point(304, 136);
            btConnect.Name = "btConnect";
            btConnect.Size = new Size(150, 150);
            btConnect.TabIndex = 4;
            btConnect.Text = "CONNECT";
            btConnect.UseVisualStyleBackColor = true;
            btConnect.Click += btConnect_Click;
            // 
            // fConnection
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 311);
            Controls.Add(btConnect);
            Controls.Add(tbPW);
            Controls.Add(label2);
            Controls.Add(tbIP);
            Controls.Add(label1);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "fConnection";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Connection";
            FormClosed += fConnection_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbIP;
        private TextBox tbPW;
        private Label label2;
        private Button btConnect;
    }
}