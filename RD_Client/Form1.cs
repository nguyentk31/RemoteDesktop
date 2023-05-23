using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RD_Client
{
    public partial class Form1 : Form
    {

        private TcpClient _tcpClientConnect;
        private NetworkStream _streamConnect;

        private bool _isConnected;


        public Form1()
        {
            InitializeComponent();
            _isConnected = false;
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            Connecting();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_isConnected)
                {
                    byte[] byteMsg = Encoding.ASCII.GetBytes("Quit");
                    _streamConnect.Write(byteMsg, 0, byteMsg.Length);

                }
                _streamConnect.Close();
                _tcpClientConnect.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task Connecting()
        {
            Action action = () =>
            {
                try
                {
                    _tcpClientConnect = new TcpClient();
                    _tcpClientConnect.Connect(IPAddress.Parse(tbIP.Text), 2003);
                    if (_tcpClientConnect.Connected)
                    {
                        _streamConnect = _tcpClientConnect.GetStream();
                        byte[] byteMsg = Encoding.ASCII.GetBytes(tbPassword.Text);
                        _streamConnect.Write(byteMsg, 0, byteMsg.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                }
            };

            Task t = new Task(action);
            t.Start();

            await t;

            try
            {
                int length = 0;
                byte[] byteMsg;
                string msg;
                while (true)
                {
                    length = _tcpClientConnect.Available;
                    byteMsg = new byte[length];
                    _streamConnect.Read(byteMsg, 0, length);

                    if (length > 0)
                    {
                        msg = Encoding.ASCII.GetString(byteMsg);
                        if (msg == "True")
                        {
                            Task showScreen = ShowScreeen();
                        }
                        else if (msg == "Quit")
                        {
                            MessageBox.Show("Server Quit");
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Wrong password");
                            break;
                        }
                    }
                }
                _streamConnect.Close();
                _tcpClientConnect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }
        private async Task ShowScreeen()
        {
            new Form2(_streamConnect).ShowDialog();
        }
    }
}