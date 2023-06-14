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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fServer));
            tbST = new TextBox();
            label1 = new Label();
            richTextBox = new RichTextBox();
            label2 = new Label();
            tbInfo = new TextBox();
            btNote = new Button();
            SuspendLayout();
            // 
            // tbST
            // 
            tbST.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbST.Location = new Point(78, 12);
            tbST.Name = "tbST";
            tbST.ReadOnly = true;
            tbST.Size = new Size(744, 29);
            tbST.TabIndex = 3;
            tbST.TabStop = false;
            tbST.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 60);
            label1.Name = "label1";
            label1.Size = new Size(78, 21);
            label1.TabIndex = 4;
            label1.Text = "Notepad:";
            // 
            // richTextBox
            // 
            richTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox.Location = new Point(12, 84);
            richTextBox.Name = "richTextBox";
            richTextBox.ReadOnly = true;
            richTextBox.Size = new Size(810, 333);
            richTextBox.TabIndex = 5;
            richTextBox.TabStop = false;
            richTextBox.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 15);
            label2.Name = "label2";
            label2.Size = new Size(60, 21);
            label2.TabIndex = 6;
            label2.Text = "Status:";
            // 
            // tbInfo
            // 
            tbInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbInfo.Location = new Point(12, 423);
            tbInfo.Name = "tbInfo";
            tbInfo.Size = new Size(729, 29);
            tbInfo.TabIndex = 7;
            // 
            // btNote
            // 
            btNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btNote.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point);
            btNote.Location = new Point(747, 423);
            btNote.Name = "btNote";
            btNote.Size = new Size(75, 29);
            btNote.TabIndex = 8;
            btNote.Text = "Note";
            btNote.UseVisualStyleBackColor = true;
            btNote.Click += btNote_Click;
            // 
            // fServer
            // 
            AcceptButton = btNote;
            AutoScaleDimensions = new SizeF(10F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(834, 461);
            Controls.Add(btNote);
            Controls.Add(tbInfo);
            Controls.Add(label2);
            Controls.Add(richTextBox);
            Controls.Add(label1);
            Controls.Add(tbST);
            Font = new Font("Times New Roman", 14F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            Name = "fServer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Server";
            FormClosing += fServer_FormClosing;
            Load += fServer_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox tbST;
        private Label label1;
        private RichTextBox richTextBox;
        private Label label2;
        private TextBox tbInfo;
        private Button btNote;
    }
}