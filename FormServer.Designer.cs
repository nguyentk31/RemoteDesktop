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
            tbST = new TextBox();
            SuspendLayout();
            // 
            // tbST
            // 
            tbST.Cursor = Cursors.IBeam;
            tbST.Dock = DockStyle.Fill;
            tbST.Location = new Point(0, 0);
            tbST.Name = "tbST";
            tbST.ReadOnly = true;
            tbST.Size = new Size(322, 29);
            tbST.TabIndex = 3;
            tbST.TabStop = false;
            tbST.TextAlign = HorizontalAlignment.Center;
            // 
            // fServer
            // 
            AutoScaleDimensions = new SizeF(10F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(322, 26);
            Controls.Add(tbST);
            Font = new Font("Times New Roman", 14F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "fServer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Server";
            FormClosed += fServer_FormClosed;
            Load += fServer_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox tbST;
    }
}