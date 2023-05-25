using System.Net;

namespace RD_Client
{
    public partial class Form1 : Form
    {

        private enum dataFor { Init = 1, Auth, Handle, Stop };
        private Client client;

        public Form1()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            tbIP.ReadOnly = tbPassword.ReadOnly = true;
            btConnect.Enabled = false;
            client = new Client(IPAddress.Parse(tbIP.Text), 2003, tbPassword.Text);
            int state = client.Connect();
            if (state == 1)
                client.Show();
            else
            {
                tbIP.ReadOnly = tbPassword.ReadOnly = false;
                btConnect.Enabled = true;
                if (state == 0)
                    MessageBox.Show("Wrong assword!");
                else
                    MessageBox.Show("Server not found!");
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!Client.isOn)
            {
                tbIP.ReadOnly = tbPassword.ReadOnly = false;
                btConnect.Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Client.isOn)
                client.Close();
        }
    }
}