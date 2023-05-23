using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RD_Client
{
    public partial class Form1 : Form
    {

        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public Form1()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            Task t = Connecting();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                byte[] byteMsg = Encoding.ASCII.GetBytes("Quit");
                _stream.Write(byteMsg, 0, byteMsg.Length);

                _stream.Close();
                _tcpClient.Close();
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
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(IPAddress.Parse(tbIP.Text), 2003);
                    if (_tcpClient.Connected)
                    {
                        _stream = _tcpClient.GetStream();
                        byte[] byteMsg = Encoding.ASCII.GetBytes(tbPassword.Text);
                        _stream.Write(byteMsg, 0, byteMsg.Length);
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
                    length = _tcpClient.Available;
                    byteMsg = new byte[length];
                    _stream.Read(byteMsg, 0, length);

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
                _stream.Close();
                _tcpClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }
        private async Task ShowScreeen()
        {
            new Form2(_stream).ShowDialog();
        }
    }
}