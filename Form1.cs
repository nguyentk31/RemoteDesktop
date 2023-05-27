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
            Hide();
            new fServer(this).ShowDialog();
        }

        private void btClient_Click(object sender, EventArgs e)
        {
            Hide();
            new fConnection(this).ShowDialog();
        }
    }
}