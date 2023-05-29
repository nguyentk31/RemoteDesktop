using System.Net;

namespace RemoteDesktop
{
    internal partial class fConnection : Form
    {
        public fConnection()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (!RemoteDesktop.ValidateIPv4(tbIP.Text))
            {
                MessageBox.Show("Invalid IP address!");
                return;
            }
            using (fClient client = new fClient(IPAddress.Parse(tbIP.Text), tbPW.Text))
            {
                int state = client.Connect();
                if (state == 1)
                {
                    this.Hide();
                    client.ShowDialog();
                    this.Show();
                }
                else if (state == 0)
                    MessageBox.Show("Wrong assword!");
                else
                    MessageBox.Show("Server not found!");
            }
        }
    }
}
