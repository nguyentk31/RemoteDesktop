namespace RD_Client
{
    partial class Client
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
            tb = new TextBox();
            pictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // tb
            // 
            tb.Location = new Point(12, 415);
            tb.Name = "tb";
            tb.ReadOnly = true;
            tb.Size = new Size(28, 23);
            tb.TabIndex = 0;
            tb.KeyDown += tb_KeyDown;
            tb.KeyUp += tb_KeyUp;
            // 
            // pictureBox
            // 
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(800, 450);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            pictureBox.MouseMove += pictureBox_MouseMove;
            pictureBox.MouseWheel += pictureBox_MouseWheel;
            pictureBox.MouseDown += pictureBox_MouseDown;
            pictureBox.MouseUp += pictureBox_MouseUp;
            // 
            // Client
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pictureBox);
            Controls.Add(tb);
            Name = "Client";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Client";
            Activated += Client_Activated;
            Deactivate += Client_Deactivate;
            FormClosed += Client_FormClosed;
            Load += Client_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tb;
        private PictureBox pictureBox;
    }
}