using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fClient : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private static bool isActivated, isConnected, isCursorShow;
        private Point mouse;
        private byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;
        private event ConnectionChangedEvent connectionClosed;

        internal fClient()
        {
            InitializeComponent();
            isConnected = false;
            headerBytesRecv = new byte[6];
            connectionClosed += CloseForm;
        }

        private void CloseForm(string msg)
        {
            Close();
        }

        private void fClient_Load(object sender, EventArgs e)
        {
            isConnected = true;
            isCursorShow = true;
            new Thread(new ThreadStart(Run)).Start();
        }

        private void fClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isCursorShow)
            {
                isCursorShow = true;
                Cursor.Show();
            }
            if (isConnected)
            {
                isConnected = false;
                dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
            }
        }

        private static void fClient_Activated(object sender, EventArgs e)
        {
            isActivated = true;
        }

        private static void fClient_Deactivate(object sender, EventArgs e)
        {
            isActivated = false;
            if (!isCursorShow)
            {
                isCursorShow = true;
                Cursor.Show();
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            dataBytesSent = CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.down, (ushort)e.KeyCode);
            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            dataBytesSent = CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.up, (ushort)e.KeyCode);
            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
        }

        private static void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (isActivated && isCursorShow)
            {
                isCursorShow = false;
                Cursor.Hide();
            }
        }

        private static void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (isActivated && !isCursorShow)
            {
                isCursorShow = true;
                Cursor.Show();
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isActivated)
                return;
            mouse = this.PointToClient(Cursor.Position);
            ushort x = (ushort)(mouse.X * 10000 / (this.Size.Width - 20));
            ushort y = (ushort)(mouse.Y * 10000 / (this.Size.Height - 40));
            dataBytesSent = CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.move, x, y);
            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isActivated)
                return;
            ushort x = Convert.ToUInt16(e.Button == MouseButtons.Left);
            ushort y = Convert.ToUInt16(e.Button == MouseButtons.Right);
            dataBytesSent = CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.down, x, y);
            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isActivated)
                return;
            ushort x = Convert.ToUInt16(e.Button == MouseButtons.Left);
            ushort y = Convert.ToUInt16(e.Button == MouseButtons.Right);
            dataBytesSent = CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.up, x, y);
            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
        }

        // Hàm được gọi khi nhấn nút Connect bên phía Client host để kết nối với Server host
        // Sau khi kết nối hoàn tất, hàm Authenticate() được gọi.
        internal int Connect(IPAddress ip, string pw)
        {
            try
            {
                // Khởi tọa kết nối TCP
                client = new TcpClient();
                // Kết nối với ip và port của Server host
                client.Connect(ip, RemoteDesktop.port);
                if (!client.Connected)
                    throw new Exception();
                stream = client.GetStream();
                dataBytesSent = Encoding.ASCII.GetBytes(pw);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                // Đọc phản hồi từ server
                dataBytesRecv = RemoteDesktop.ReadExactly(stream, 8);
                if ((connectionStatus)BitConverter.ToInt16(dataBytesRecv, 6) == connectionStatus.success)
                    return 1;
                stream.Dispose();
                client.Dispose();
                return 0;
            }
            catch
            {
                stream.Dispose();
                client.Dispose();
                return -1;
            }
        }

        // Hàm Run dùng để nhận và hiển thị hình ảnh do Server host gửi qua
        // hoặc nhận tín hiệu kết thúc và gửi tín hiệu kết thúc đến Server
        private void Run()
        {
            try
            {
                int dblength;
                dataFormat type;
                while (true)
                {
                    headerBytesRecv = RemoteDesktop.ReadExactly(stream, headerBytesRecv.Length);
                    type = (dataFormat)BitConverter.ToInt16(headerBytesRecv, 0);
                    if (type == dataFormat.handle)
                    {
                        dblength = BitConverter.ToInt32(headerBytesRecv, 2);
                        dataBytesRecv = RemoteDesktop.ReadExactly(stream, dblength);
                        using (MemoryStream ms = new MemoryStream(dataBytesRecv))
                        {
                            pictureBox.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        if (isConnected)
                        {
                            isConnected = false;
                            connectionClosed?.Invoke("quit");
                            Thread.Sleep(1000);
                            dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                        }
                        else
                        {
                            stream.Dispose();
                            client.Dispose();
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        // Chuyển các tín hiệu input từ bit thành dữ liệu dưới dạng mảng Byte
        private byte[] CreateInputBytes(ushort iType, ushort iEvent, ushort iInfor1, ushort iInfor2 = 0)
        {
            // Chuyển thông tin của tín hiệu thành dữ liệu byte và lưu vào một mảng 8 bytes
            byte[] inputBytes = new byte[8];

            // Hai byte đầu chứa loại input (chuột, bàn phím)
            Buffer.BlockCopy(BitConverter.GetBytes(iType), 0, inputBytes, 0, 2);

            // Byte thứ 3 và 4 chứa thông tin sự kiện của tín hiệu đó (up, down, move, ...)
            Buffer.BlockCopy(BitConverter.GetBytes(iEvent), 0, inputBytes, 2, 2);

            // Byte thứ 5 và 6 chứa thông tin thứ nhất của tín hiệu (tọa độ x, y,...)
            Buffer.BlockCopy(BitConverter.GetBytes(iInfor1), 0, inputBytes, 4, 2);

            // Byte thứ 7 và 8 chứa thông tin thứ hai của tín hiệu (tọa độ x, y,...)
            Buffer.BlockCopy(BitConverter.GetBytes(iInfor2), 0, inputBytes, 6, 2);
            return inputBytes;
        }
    }
}
