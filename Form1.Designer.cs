namespace RemoteDesktop
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
            btServer = new Button();
            btClient = new Button();
            SuspendLayout();
            // 
            // btServer
            // 
            btServer.Font = new Font("Times New Roman", 20F, FontStyle.Regular, GraphicsUnit.Point);
            btServer.Location = new Point(13, 13);
            btServer.Margin = new Padding(4, 4, 4, 4);
            btServer.Name = "btServer";
            btServer.Size = new Size(200, 200);
            btServer.TabIndex = 0;
            btServer.TabStop = false;
            btServer.Text = "SERVER";
            btServer.UseVisualStyleBackColor = true;
            btServer.Click += btServer_Click;
            // 
            // btClient
            // 
            btClient.Font = new Font("Times New Roman", 20F, FontStyle.Regular, GraphicsUnit.Point);
            btClient.Location = new Point(271, 98);
            btClient.Margin = new Padding(4, 4, 4, 4);
            btClient.Name = "btClient";
            btClient.Size = new Size(200, 200);
            btClient.TabIndex = 1;
            btClient.TabStop = false;
            btClient.Text = "CLIENT";
            btClient.UseVisualStyleBackColor = true;
            btClient.Click += btClient_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 311);
            Controls.Add(btClient);
            Controls.Add(btServer);
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "Remote Desktop";
            ResumeLayout(false);
        }

        #endregion

        private Button btServer;
        private Button btClient;
    }
}