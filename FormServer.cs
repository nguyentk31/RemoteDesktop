using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fServer : Form
    {
        private string password;
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected, isRunning;
        private System.Timers.Timer timer;
        private byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;
        private event ConnectionChangedEvent statusChanged;


        internal fServer(IPAddress ip, string pw)
        {
            InitializeComponent();
            listener = new TcpListener(ip, RemoteDesktop.port);
            password = pw;
            headerBytesRecv = new byte[6];
            statusChanged += PublishStatus;
        }

        // Bắt đầu hàm Listen() và load dữ liệu của Server, bao gồm: 
        // Ipv4 và password
        private void fServer_Load(object sender, EventArgs e)
        {
            isConnected = false;
            isRunning = true;
            new Thread(new ThreadStart(Listen)).Start();
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
                isConnected = false;
                timer.Dispose();
                dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
            }
            isRunning = false;
        }

        // Thiết lập trạng thái khi bắt đầu lắng nghe kết nối
        // Dùng để lắng nghe đến port và ip của Server,
        // giới hạn chỉ được 1 kết nối đến Server
        private void Listen()
        {
            try
            {
                listener.Start();
                //statusChanged?.Invoke("SERVER IS LISTENING...");
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
                        new Thread(new ThreadStart(Monitor)).Start();
                        break;
                    }
                }
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
                    type = (dataFormat)BitConverter.ToInt16(headerBytesRecv, 0);
                    dblength = BitConverter.ToInt32(headerBytesRecv, 2);
                    dataBytesRecv = RemoteDesktop.ReadExactly(stream, dblength);
                    // Khi nhận gói tin input
                    if (type == dataFormat.handle)
                    {
                        Input[] inputs = HandleInputBytes(dataBytesRecv);
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
                            //statusChanged?.Invoke("SERVER IS CONNECTED.");
                            dataBytesSent = BitConverter.GetBytes((ushort)connectionStatus.success);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            // Timer dùng dể thiết lập thời gian gửi hình ảnh màn hình từ server đến client
                            // Ở đây timer được set là 100 mili giây
                            // nên cứ mỗi 100ms, Client sẽ nhận 1 hình ảnh màn hình từ Server
                            timer = new System.Timers.Timer(100);
                            timer.Elapsed += (sender, e) => SendImage();
                            timer.Start();
                        }
                        else
                        {
                            dataBytesSent = BitConverter.GetBytes((ushort)connectionStatus.failure);
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                            break;
                        }
                    }
                    // Khi nhận gói tin quit
                    else if (isConnected)
                    {
                        timer.Dispose();
                        isConnected = false;
                        Thread.Sleep(1000);
                        dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                        RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                        break;
                    }
                    else
                    {
                        stream.Dispose();
                        client.Dispose();
                        break;
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

        private void SendImage()
        {
            try
            {
                // Chụp một tấm hình của màn hình Server dưới dạng Bitmap
                // Bitmap là hình ảnh có dạng mảnh hóa, nghĩa là ảnh tạo từ các điểm ảnh
                // Mỗi điểm ảnh chứa 1 thông số màu nhất định được xếp với nhau theo lưới sẽ tạo thành ảnh bitmap
                Rectangle bound = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppArgb))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy);

                    // Lưu lại trạng thái con trỏ và vẽ lại lên trên tấm hình nếu con trỏ xuất hiện
                    CURSORINFO cursorInfo;
                    cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    if (User32.GetCursorInfo(out cursorInfo))
                    {
                        if (cursorInfo.flags == User32.CURSOR_SHOWING)
                        {
                            var iconPointer = User32.CopyIcon(cursorInfo.hCursor);
                            ICONINFO iconInfo;
                            int iconX, iconY;
                            if (User32.GetIconInfo(iconPointer, out iconInfo))
                            {
                                iconX = cursorInfo.ptScreenPos.x - ((int)iconInfo.xHotspot);
                                iconY = cursorInfo.ptScreenPos.y - ((int)iconInfo.yHotspot);
                                User32.DrawIcon(graphics.GetHdc(), iconX, iconY, cursorInfo.hCursor);
                                graphics.ReleaseHdc();
                            }
                        }
                    }
                    // Chuyển Bitmap về dạng byte để gửi đi
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        dataBytesSent = ms.ToArray();
                        RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
                    }
                }
            }
            catch (Exception ex)
            {
                timer.Dispose();
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        // Xử lí thông tin input
        private Input[] HandleInputBytes(byte[] iBytes)
        {
            ushort iType = BitConverter.ToUInt16(iBytes, 0),
                iEvent = BitConverter.ToUInt16(iBytes, 2),
                iInfor1 = BitConverter.ToUInt16(iBytes, 4),
                iInfor2 = BitConverter.ToUInt16(iBytes, 6);
            Input input = new Input();
            input.u.ki.dwExtraInfo = User32.GetMessageExtraInfo();
            if (iType == (ushort)InputType.Keyboard)
            {
                input.type = (int)InputType.Keyboard;
                input.u.ki.wVk = iInfor1;
                if (iEvent == (ushort)inputEvent.down)
                    input.u.ki.dwFlags = (uint)KeyEventF.KeyDown;
                else
                    input.u.ki.dwFlags = (uint)KeyEventF.KeyUp;
            }
            else
            {
                input.type = (int)InputType.Mouse;
                if (iEvent == (ushort)inputEvent.move)
                {
                    int x = (int)iInfor1 * Screen.PrimaryScreen.Bounds.Width / 10000;
                    int y = (int)iInfor2 * Screen.PrimaryScreen.Bounds.Height / 10000;
                    input.u.mi.dwFlags = (uint)MouseEventF.Absolute;
                    User32.SetCursorPos(x, y);
                }
                else if (iEvent == (ushort)inputEvent.down)
                {
                    if (iInfor1 == 1)
                        input.u.mi.dwFlags = (uint)MouseEventF.LeftDown;
                    else if (iInfor2 == 1)
                        input.u.mi.dwFlags = (uint)MouseEventF.RightDown;
                    else
                        input.u.mi.dwFlags = (uint)MouseEventF.MiddleDown;
                }
                else
                {
                    if (iInfor1 == 1)
                        input.u.mi.dwFlags = (uint)MouseEventF.LeftUp;
                    else if (iInfor2 == 1)
                        input.u.mi.dwFlags = (uint)MouseEventF.RightUp;
                    else
                        input.u.mi.dwFlags = (uint)MouseEventF.MiddleUp;
                }
            }
            return new Input[] { input };
        }
    }
}
