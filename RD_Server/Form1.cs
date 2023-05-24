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
        // Kieu du lieu nhan duoc
        private enum dataFor {Init = 1, Auth, Handle, Stop};
        // Password de ket noi voi server
        private readonly int _passwordLength;
        private readonly string _password;

        // IP va port server lang nghe
        private readonly IPAddress _localIP;
        private readonly int _localPort;

        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;

        // Kiem tra server co dang ket noi voi client
        private bool _isConnected;

        // Bo dem de gui screen cho client
        private System.Timers.Timer _timer;

        public Form1()
        {
            InitializeComponent();
            _localIP = IPv4();
            _localPort = 2003;
            _passwordLength = 8;
            _password = GeneratePassword(_passwordLength);

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
                    _timer.Stop();
                    byte[] byteInfo = Encoding.ASCII.GetBytes("Quit");
                    byte[] byteSend = AddHeader(byteInfo, (int) dataFor.Stop);
                    _stream.Write(byteSend, 0, byteSend.Length);

                    Thread.Sleep(100);
                    _stream.Close();
                    _client.Close();
                }
                else
                    _listener.Stop();

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

        // Lay IPv4 cua server
        private IPAddress IPv4()
        {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] list = iphe.AddressList;
            foreach (IPAddress ip in list)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            throw new Exception("Không tìm thấy ipv4 trong hệ thống.");
        }

        // Khoi tao mot mat khau ngau nhien
        private string GeneratePassword(int l)
        {
            Random rnd = new Random();
            string str = string.Empty;
            for (int i = 0; i < l; i++)
            {
                str += (char)rnd.Next(48, 122);
            }
            return str;
        }

        private byte[] AddHeader(byte[] info, int type)
        {
            byte[] rt = new byte[info.Length + 1];
            byte[] header = Encoding.ASCII.GetBytes(type.ToString());
            Buffer.BlockCopy(header, 0, rt, 0, 1);
            Buffer.BlockCopy(info, 0, rt, 1, info.Length);
            return rt;
        }

        private byte[] CreateImgData(byte[] img, int length)
        {
            byte[] l = Encoding.ASCII.GetBytes(length.ToString());
            byte[] rt = new byte[length + 10];
            Buffer.BlockCopy(l, 0, rt, 0, l.Length);
            Buffer.BlockCopy(img, 0, rt, 10, length);
            return rt;
        }

        private async Task Run()
        {
            Action action = async () =>
            {
                try
                {
                    _listener = new TcpListener(_localIP, _localPort);
                    _listener.Start();

                    while (!_listener.Pending())
                    {
                        Thread.Sleep(500);
                    }

                    _client = _listener.AcceptTcpClient();
                    _listener.Stop();
                    _isConnected = true;
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
                _stream = _client.GetStream();

                byte[] byteInfo, byteSend, byteHead = new byte[1];
                string content;
                int type;
                while (true)
                {
                    await _stream.ReadAsync(byteHead, 0, 1);
                    type = Convert.ToInt32(Encoding.ASCII.GetString(byteHead));
                    if (type == (int) dataFor.Handle)
                    {
                    }
                    else if (type == (int) dataFor.Init)
                    {
                        byteInfo = new byte[_passwordLength];
                        await _stream.ReadAsync(byteInfo, 0, _passwordLength);

                        content = Encoding.ASCII.GetString(byteInfo);
                        if (content == _password)
                        {
                            byteInfo = Encoding.ASCII.GetBytes("1234");// 1234: Dung mat khau -> Dong y ket noi
                            byteSend = AddHeader(byteInfo, (int) dataFor.Auth);
                            await _stream.WriteAsync(byteSend, 0, byteSend.Length);
                            _timer.Start();
                        }
                        else
                        {
                            byteInfo = Encoding.ASCII.GetBytes("4321");// 4321: Dung mat khau -> Dong y ket noi
                            byteSend = AddHeader(byteInfo, (int) dataFor.Auth);
                            await _stream.WriteAsync(byteSend, 0, byteSend.Length);
                            break;
                        }
                    }
                    else if (type == (int) dataFor.Stop)
                    {
                        _timer.Stop();
                        break;
                    }
                    else
                    {
                        throw new Exception("Connection Error!");
                    }
                }

                _isConnected = false;

                Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
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

                    byte[] byteImgData = CreateImgData(byteImage, byteImage.Length);
                    byte[] byteSend = AddHeader(byteImgData, (int) dataFor.Handle);

                    _stream.Write(byteSend, 0, byteSend.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
            
        }
    }
}