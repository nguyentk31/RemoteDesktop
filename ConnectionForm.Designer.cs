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
            label1.Location = new Point(37, 28);
            label1.Name = "label1";
            label1.Size = new Size(165, 19);
            label1.TabIndex = 0;
            label1.Text = "REMOTE IP ADDRESS:";
            // 
            // tbIP
            // 
            tbIP.Cursor = Cursors.IBeam;
            tbIP.Location = new Point(37, 50);
            tbIP.Name = "tbIP";
            tbIP.Size = new Size(207, 26);
            tbIP.TabIndex = 1;
            // 
            // tbPW
            // 
            tbPW.Cursor = Cursors.IBeam;
            tbPW.Location = new Point(37, 115);
            tbPW.Name = "tbPW";
            tbPW.Size = new Size(207, 26);
            tbPW.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(37, 93);
            label2.Name = "label2";
            label2.Size = new Size(97, 19);
            label2.TabIndex = 2;
            label2.Text = "PASSWORD:";
            // 
            // btConnect
            // 
            btConnect.Location = new Point(37, 184);
            btConnect.Name = "btConnect";
            btConnect.Size = new Size(407, 96);
            btConnect.TabIndex = 4;
            btConnect.TabStop = false;
            btConnect.Text = "CONNECT";
            btConnect.UseVisualStyleBackColor = true;
            btConnect.Click += btConnect_Click;
            // 
            // fConnection
            // 
            AcceptButton = btConnect;
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 307);
            Controls.Add(tbPW);
            Controls.Add(label2);
            Controls.Add(tbIP);
            Controls.Add(label1);
            Controls.Add(btConnect);
            Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "fConnection";
            Text = "Connection";
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