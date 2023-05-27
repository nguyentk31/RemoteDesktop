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
            client = new Client(IPAddress.Parse(tbIP.Text), 2003, tbPassword.Text, this);
            int state = client.Connect();
            if (state == 1)
                client.Show();
            else if (state == 0)
                MessageBox.Show("Wrong assword!");
            else
                MessageBox.Show("Server not found!");
        }
    }
}