using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace RD_Client
{
    public partial class Form1 : Form
    {

        private enum dataFor {Init = 1, Auth, Handle, Stop};

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Task _task;
        private bool _taskRan;

        private Form2 _form;

        public Form1()
        {
            InitializeComponent();
            _taskRan = false;
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            _task = Connecting();
            _taskRan = true;
            tbIP.ReadOnly = tbPassword.ReadOnly = true;
            btConnect.Enabled = false;
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!Form2._isOn)
            {
                tbIP.ReadOnly = tbPassword.ReadOnly = false;
                btConnect.Enabled = true;
                _taskRan = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Form2._isOn)
                _form.Close();
        }
        
        private byte[] AddHeader(byte[] info, int type)
        {
            byte[] rt = new byte[info.Length + 1];
            byte[] header = Encoding.ASCII.GetBytes(type.ToString());
            Buffer.BlockCopy(header, 0, rt, 0, 1);
            Buffer.BlockCopy(info, 0, rt, 1, info.Length);
            return rt;
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
                        byte[] byteInfo = Encoding.ASCII.GetBytes(tbPassword.Text);
                        byte[] byteSend = AddHeader(byteInfo, (int) dataFor.Init);
                        _stream.Write(byteSend, 0, byteSend.Length);
                    }
                    else
                        throw new Exception("Server not found");
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
                byte[] byteAuth = new byte[4], byteHead = new byte[1];
                string content;
                int type;

                await _stream.ReadAsync(byteHead, 0, 1);
                type = Convert.ToInt32(Encoding.ASCII.GetString(byteHead));

                if (type == (int) dataFor.Auth)
                {
                    await _stream.ReadAsync(byteAuth, 0, 4);
                    content = Encoding.ASCII.GetString(byteAuth);
                    if (content == "1234")
                    {
                        _form = new Form2(_tcpClient, _stream);
                        new Task(() => _form.ShowDialog()).Start();
                    }
                    else if (content == "4321")
                        MessageBox.Show("Wrong Password!");
                    else
                        throw new Exception("Connection Error!");
                }
                else
                        throw new Exception("Connection Error!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }
    }
}