using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fServer : Form
    {
        private static IPAddress localIP = RemoteDesktop.GetIPv4();
        private static string password = RemoteDesktop.GeneratePassword(5);
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected, isRunning;
        private System.Timers.Timer timer;
        private byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;
        private event ConnectionChangedEvent statusChanged;


        internal fServer()
        {
            InitializeComponent();
            isConnected = false;
            isRunning = true;
            headerBytesRecv = new byte[8];
            statusChanged += PublishStatus;
        }

// Bắt đầu hàm Listen() và load dữ liệu của Server, bao gồm: 
// Ipv4 và password
        private void fServer_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Listen)).Start();
            tbIP.Text = localIP.ToString();
            tbPW.Text = password;
        }

// Cập nhật trạng thái Server
        private void PublishStatus(string st)
        {
            tbST.Text = st;
        }

// Được gọi khi tắt Server host, dùng để dùng việc gửi dữ liệu, gửi tín hiệu kết thúc đến Client
// và đóng server.
        private void fServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isConnected)
            {
                timer.Dispose();
                isConnected = false;
                dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
            }
            else
                isRunning = false;
        }

// Thiết lập trạng thái khi bắt đầu lắng nghe kết nối
// Dùng để lắng nghe đến port và ip của Server,
// giới hạn chỉ được 1 kết nối đến Server
        private void Listen()
        {
            try
            {
                listener = new TcpListener(localIP, RemoteDesktop.port);
                listener.Start();
                statusChanged?.Invoke("SERVER IS LISTENING.");
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
                        listener.Stop();
                        new Thread(new ThreadStart(Monitor)).Start();
                        break;
                    }
                }
                if (!isRunning)
                    listener.Stop();
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
                int dblength;
                dataFormat type;
                while (true)
                {
                    // Dữ liệu input
                    headerBytesRecv = RemoteDesktop.ReadExactly(stream, headerBytesRecv.Length);
                    type = (dataFormat)BitConverter.ToInt32(headerBytesRecv, 0);
                    dblength = BitConverter.ToInt32(headerBytesRecv, 4);
                    dataBytesRecv = RemoteDesktop.ReadExactly(stream, dblength);
                    // Khi nhận gói tin input
                    if (type == dataFormat.handle)
                    {
                        Input[] inputs = RemoteDesktop.HandleInputBytes(dataBytesRecv);
                        if (inputs[0].u.mi.dwFlags == (uint)MouseEventF.Absolute)
                            continue;
                        User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }
                    // Khi nhận gói tin kết nối
                    else if (dblength == password.Length)
                    {
                        string infomation = Encoding.ASCII.GetString(dataBytesRecv);
                        if (infomation == password)
                        {
                            isConnected = true;
                            statusChanged?.Invoke("SERVER IS CONNECTED.");
                            dataBytesSent = BitConverter.GetBytes((int)connectionStatus.success);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            // Timer dùng dể thiết lập thời gian gửi hình ảnh màn hình từ server đến client
                            // Ở đây timer được set là 100 mili giây
                            // nên cứ mỗi 100ms, Client sẽ nhận 1 hình ảnh màn hình từ Server
                            timer = new System.Timers.Timer(100);
                            timer.Elapsed += (sender, e) => RemoteDesktop.SendImage(stream);
                            timer.Start();
                        }
                        else
                        {
                            dataBytesSent = BitConverter.GetBytes((int)connectionStatus.failure);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            break;
                        }
                    }
                    // Khi nhận gói tin quit
                    else if (isConnected)
                    {
                        timer.Dispose();
                        isConnected = false;
                        Thread.Sleep(5000);
                        dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                        RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                        break;
                    }
                    else
                    {
                        stream.Close();
                        client.Close();
                        return;
                    }
                }
                // Khởi động lại server
                new Thread(new ThreadStart(Listen)).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }
    }
}
