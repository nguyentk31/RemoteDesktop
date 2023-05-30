namespace RemoteDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btServer_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (fServer server = new fServer())
            {
                server.ShowDialog();
            }
            this.Show();
        }

        private void btClient_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (fConnection connection = new fConnection())
            {
                connection.ShowDialog();
            }
            this.Show();
        }
    }
}