﻿using System.Net;

namespace RemoteDesktop
{
    internal partial class fConnection : Form
    {
        Form1 formParent;
        public fConnection(Form1 fParent)
        {
            InitializeComponent();
            formParent = fParent;
        }

        private void fConnection_FormClosed(object sender, FormClosedEventArgs e)
        {
            formParent.Show();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            fClient client = new fClient(this, IPAddress.Parse(tbIP.Text), tbPW.Text);
            int state = client.Connect();
            if (state == 1)
            {
                Hide();
                client.Show();
            }
            else if (state == 0)
                MessageBox.Show("Wrong assword!");
            else
                MessageBox.Show("Server not found!");
        }
    }
}
