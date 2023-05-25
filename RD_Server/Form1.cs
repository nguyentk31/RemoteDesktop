namespace RD_Server
{
    public partial class Form1 : Form
    {
        Server server;

        public Form1()
        {
            InitializeComponent();
            server = new Server();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(server.StartListening)).Start();
            tbIP.Text = server.localIP.ToString();
            tbPassword.Text = server.password;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.CloseServer();
        }
    }
}