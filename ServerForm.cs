using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fServer : Form
    {
        private static Form1 formParent;
        private static IPAddress localIP;
        private static string password;
        private static TcpListener listener;
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool isConnected, isRunning;
        private static string status;
        private static System.Timers.Timer timer;
        private static byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;

        internal fServer(Form1 fParent)
        {
            InitializeComponent();
            formParent = fParent;
            localIP = RemoteDesktop.GetIPv4();
            password = RemoteDesktop.GeneratePassword(10);
            isConnected = false;
            isRunning = true;
            status = "SERVER IS RUNNING.";

            // Timer dùng dể thiết lập thời gian gửi hình ảnh màn hình từ server đến client
            // Ở đây timer được set là 100 mili giây
            // nên cứ mỗi 100ms, Client sẽ nhận 1 hình ảnh màn hình từ Server
            timer = new System.Timers.Timer(100);
            timer.Elapsed += (sender, e) => SendImage();
            headerBytesRecv = new byte[8];
        }

// Bắt đầu hàm Listen() và load dữ liệu của Server, bao gồm: 
// Ipv4 và password
        private void fServer_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Listen)).Start();
            tbIP.Text = localIP.ToString();
            tbPW.Text = password;
        }

// Thiết lập trạng thái khi bắt đầu lắng nghe kết nối
        private void fServer_Activated(object sender, EventArgs e)
        {
            tbST.Text = status;
        }

// Được gọi khi tắt Server host, dùng để dùng việc gửi dữ liệu, gửi tín hiệu kết thúc đến Client
// và đóng server.
        private void fServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                isRunning = false;
                if (isConnected)
                {
                    timer.Stop();
                    dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                    Thread.Sleep(500);
                    stream.Close();
                    client.Close();
                }
                else
                    listener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
            formParent.Show();
        }


// Dùng để lắng nghe đến port và ip của Server,
// giới hạn chỉ được 1 kết nối đến Server
        private void Listen()
        {
            try
            {
                listener = new TcpListener(localIP, RemoteDesktop.port);
                listener.Start();
                status = "SERVER IS LISTENING.";
                while (isRunning)
                {
                    if (!listener.Pending())
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        client = listener.AcceptTcpClient();
                        stream = client.GetStream();
                        isConnected = true;
                        listener.Stop();
                        new Thread(new ThreadStart(Monitor)).Start();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

// Dùng để xử lý dữ liệu được gửi đến
        private void Monitor()
        {
            try
            {
                string infomation;
                int dblength;
                dataFormat type;
                while (true)
                {
                    // Dữ liệu input
                    headerBytesRecv = RemoteDesktop.ReadExactly(stream, headerBytesRecv.Length);
                    type = (dataFormat)BitConverter.ToInt32(headerBytesRecv, 0);
                    dblength = BitConverter.ToInt32(headerBytesRecv, 4);
                    dataBytesRecv = RemoteDesktop.ReadExactly(stream, dblength);

                    if (type == dataFormat.handle)
                    {
                        Input[] inputs = RemoteDesktop.HandleInputBytes(dataBytesRecv);
                        if (inputs[0].u.mi.dwFlags == (uint)MouseEventF.Absolute)
                            continue;
                        User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }

                    // Dữ liệu để xác thực
                    else if (dblength == password.Length)
                    {
                        infomation = Encoding.ASCII.GetString(dataBytesRecv);
                        if (infomation == password)
                        {
                            status = "SERVER IS CONNECTED.";
                            dataBytesSent = BitConverter.GetBytes((int)connectionStatus.success);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            timer.Start();
                        }
                        else
                        {
                            dataBytesSent = BitConverter.GetBytes((int)connectionStatus.failure);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            break;
                        }
                    }

                    // Dữ liệu để kết thúc
                    else
                    {
                        timer.Stop();
                        if (isConnected)
                        {
                            dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
            isConnected = false;
            new Thread(new ThreadStart(Listen)).Start();
        }

// Dùng để gửi hình ảnh đã được chụp đến Client
        private void SendImage()
        {
            try
            {
                Image screen = RemoteDesktop.Capture();
                using (MemoryStream ms = new MemoryStream())
                {
                    screen.Save(ms, ImageFormat.Jpeg);
                    dataBytesSent = ms.ToArray();
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }
    }
}
