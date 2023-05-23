using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace RD_Server
{
    public partial class Form1 : Form
    {

        public readonly string _password;

        public readonly IPAddress _localIP;
        public readonly int _localPort;
        public IPAddress _remotelIP;

        private TcpListener _listenerConnect;
        private TcpClient _clientConnect;
        private NetworkStream _streamConnect;

        private TcpClient _clientSendImg;
        private NetworkStream _streamSendImg;

        private bool _isConnected;
        private System.Timers.Timer _timer;

        public Form1()
        {
            InitializeComponent();
            _localIP = ipv4();
            _localPort = 2003;
            _password = generatePassword();

            _isConnected = false;
            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Run();
            tbIP.Text = _localIP.ToString();
            tbPassword.Text = _password;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_isConnected)
                {
                    byte[] byteMsg = Encoding.ASCII.GetBytes("Quit");
                    _streamConnect.Write(byteMsg, 0, byteMsg.Length);

                    _streamSendImg.Close();
                    _clientSendImg.Close();
                }
                _streamConnect.Close();
                _clientConnect.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            SendImage();
        }

        private IPAddress ipv4()
        {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] list = iphe.AddressList;
            foreach (IPAddress ip in list)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
            throw new Exception("Không tìm thấy ipv4 trong hệ thống.");
        }

        private string generatePassword()
        {
            Random rnd = new Random();
            string str = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                str += (char)rnd.Next(48, 122);
            }
            return str;
        }

        private async Task Run()
        {
            Action action = async () =>
            {
                try
                {
                    _listenerConnect = new TcpListener(_localIP, _localPort);
                    _listenerConnect.Start();

                    while (!_listenerConnect.Pending())
                    {
                        Thread.Sleep(500);
                    }

                    _clientConnect = _listenerConnect.AcceptTcpClient();
                    _listenerConnect.Stop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
                }
            };

            Task t = new Task(action);
            t.Start();

            await t;

            try
            {
                _streamConnect = _clientConnect.GetStream();   
                int length = 0;
                byte[] byteMsg;
                string msg;

                while (true)
                {
                    length = _clientConnect.Available;
                    byteMsg = new byte[length];
                    await _streamConnect.ReadAsync(byteMsg, 0, length);

                    if (length > 0)
                    {
                        msg = Encoding.ASCII.GetString(byteMsg);
                    if (msg == _password)
                    {
                        byteMsg = Encoding.ASCII.GetBytes("True");
                        await _streamConnect.WriteAsync(byteMsg, 0, byteMsg.Length);
                        AfterConnection();
                    }
                    else if (msg == "Quit")
                    {
                        _timer.Stop();
                        
                        byteMsg = Encoding.ASCII.GetBytes("Quit");
                        await _streamConnect.WriteAsync(byteMsg, 0, byteMsg.Length);

                        break;
                    }
                    else
                    {
                        byteMsg = Encoding.ASCII.GetBytes("False");
                        await _streamConnect.WriteAsync(byteMsg, 0, byteMsg.Length);
                        break;
                    }
                    }
                }

                _streamConnect.Close();
                _clientConnect.Close();

                Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        private async Task AfterConnection()
        {
            try
            {
                _remotelIP = ((IPEndPoint)_clientConnect.Client.RemoteEndPoint).Address;
                _clientSendImg = new TcpClient();

                await _clientSendImg.ConnectAsync(_remotelIP, _localPort + 1);
                _streamSendImg = _clientSendImg.GetStream();

                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        private async Task SendImage()
        {

            try
            {
                Func<Bitmap> function = () =>
                {
                    try
                    {
                        Rectangle bound = Screen.PrimaryScreen.Bounds;
                        Bitmap screenshot = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppArgb);
                        Graphics graphics = Graphics.FromImage(screenshot);
                        graphics.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy);
                        return screenshot;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
                    }
                    return null;
                };

                Task<Bitmap> t = new Task<Bitmap>(function);
                t.Start();

                using (MemoryStream ms = new MemoryStream())
                {
                    t.Result.Save(ms, ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    byte[] byteLength = Encoding.ASCII.GetBytes(byteImage.Length.ToString());
                    byte[] byteSend = new byte[byteImage.Length + 10];

                    Buffer.BlockCopy(byteLength, 0, byteSend, 0, byteLength.Length);
                    Buffer.BlockCopy(byteImage, 0, byteSend, 10, byteImage.Length);
                    _streamSendImg.Write(byteSend, 0, byteSend.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
            
        }
    }
}