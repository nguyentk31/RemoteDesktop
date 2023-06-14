using System.Net;

namespace RemoteDesktop
{
    public partial class Form1 : Form
    {
        private IPAddress localIP;
        private string localPW;
        private fServer server;
        private fClient client;

        public Form1()
        {
            InitializeComponent();
            localIP = RemoteDesktop.GetIPv4();
            localPW = RemoteDesktop.GeneratePassword(5);
            server = new fServer(localIP, localPW);
            client = new fClient();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbLocalIP.Text = localIP.ToString();
            tbLocalPW.Text = localPW;
        }

        private void btListen_Click(object sender, EventArgs e)
        {
            Hide();
            server.ShowDialog();
            Show();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (!RemoteDesktop.ValidateIPv4(tbRemoteIP.Text))
            {
                MessageBox.Show("Invalid IP address!");
                return;
            }
            int state = client.Connect(IPAddress.Parse(tbRemoteIP.Text), tbRemotePW.Text);
            if (state == 1)
            {
                Hide();
                client.ShowDialog();
                Show();
            }
            else if (state == 0)
                MessageBox.Show("Wrong assword!");
            else
                MessageBox.Show("Server not found!");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Dispose();
            client.Dispose();
        }
    }
}