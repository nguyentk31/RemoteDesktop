using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RD_Server
{
    internal class Server
    {
        // Kieu du lieu nhan duoc
        private enum dataFor {Init = 1, Auth, Handle, Stop};
        // Password de ket noi voi server
        private readonly int passwordLength;
        public string password {get;}
        // IP va port server lang nghe
        public IPAddress localIP {get;}
        private readonly int localPort;
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        // Kiem tra server co dang ket noi voi client
        private bool isConnected;
        private bool isOn;
        // Bo dem de gui screen cho client
        private System.Timers.Timer timer;

        public Server()
        {
            localIP = IPv4();
            localPort = 2003;
            passwordLength = 8;
            password = GeneratePassword(passwordLength);
            isConnected = false;
            isOn = true;
            timer = new System.Timers.Timer(100);
            timer.Elapsed += async (sender, e) => SendImage();
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
            throw new Exception("Can't find IPv4!");
        }

        // Khoi tao mot mat khau ngau nhien
        private string GeneratePassword(int length)
        {
            Random rnd = new Random();
            string pw = string.Empty;
            for (int i = 0; i < length; i++)
            {
                pw += (char)rnd.Next(48, 122);
            }
            return pw;
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
            byte[] rt = new byte[length + 6];
            Buffer.BlockCopy(l, 0, rt, 0, l.Length);
            Buffer.BlockCopy(img, 0, rt, 6, length);
            return rt;
        }

        public void StartListening()
        {
            try
            {
                listener = new TcpListener(localIP, localPort);
                listener.Start();
                while (isOn)
                {
                    if (!listener.Pending())
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        client = listener.AcceptTcpClient();
                        listener.Stop();
                        isConnected = true;
                        new Thread(new ThreadStart(Monitoring)).Start();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        private void Monitoring()
        {
            try
            {
                stream = client.GetStream();
                byte[] byteInfo, byteSend, byteHead = new byte[1];
                string content;
                string type;
                while (true)
                {
                    stream.Read(byteHead, 0, 1);
                    type = Encoding.ASCII.GetString(byteHead);
                    if (type == ((int)dataFor.Handle).ToString())
                    {
                    }
                    else if (type == ((int)dataFor.Init).ToString())
                    {
                        byteInfo = new byte[passwordLength];
                        stream.Read(byteInfo, 0, passwordLength);
                        content = Encoding.ASCII.GetString(byteInfo);
                        if (content == password)
                        {
                            byteInfo = Encoding.ASCII.GetBytes("1234");// 1234: Dung mat khau -> Dong y ket noi
                            byteSend = AddHeader(byteInfo, (int) dataFor.Auth);
                            stream.Write(byteSend, 0, byteSend.Length);
                            timer.Start();
                        }
                        else
                        {
                            byteInfo = Encoding.ASCII.GetBytes("4321");// 4321: Dung mat khau -> Dong y ket noi
                            byteSend = AddHeader(byteInfo, (int) dataFor.Auth);
                            stream.Write(byteSend, 0, byteSend.Length);
                            break;
                        }
                    }
                    else if (type == ((int)dataFor.Stop).ToString())
                    {
                        timer.Stop();
                        if (isOn)
                        {
                            byteInfo = Encoding.ASCII.GetBytes("Quit");
                            byteSend = AddHeader(byteInfo, (int) dataFor.Stop);
                            stream.Write(byteSend, 0, byteSend.Length);
                        }
                        break;
                    }
                    else
                    {
                        byte[] byteDel = new byte[client.Available];
                        stream.Read(byteDel, 0, byteDel.Length);
                    }
                }
                isConnected = false;
                new Thread(new ThreadStart(StartListening)).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }
        private void SendImage()
        {
            try
            {
                Image screen = ScreenCapture.CapturingScreen();
                using (MemoryStream ms = new MemoryStream())
                {
                    screen.Save(ms, ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    byte[] byteImgData = CreateImgData(byteImage, byteImage.Length);
                    byte[] byteSend = AddHeader(byteImgData, (int) dataFor.Handle);
                    stream.Write(byteSend, 0, byteSend.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Host: {Dns.GetHostName()}.\nException: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        public void CloseServer()
        {
            try
            {
                isOn = false;
                if (isConnected)
                {
                    timer.Stop();
                    byte[] byteInfo = Encoding.ASCII.GetBytes("Quit");
                    byte[] byteSend = AddHeader(byteInfo, (int) dataFor.Stop);
                    stream.Write(byteSend, 0, byteSend.Length);
                    Thread.Sleep(100);
                    stream.Close();
                    client.Close();
                }
                else
                {
                    listener.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}