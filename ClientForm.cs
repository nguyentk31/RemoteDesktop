using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteDesktop
{
    public partial class fClient : Form
    {
        fConnection formParent;
        private readonly IPAddress remoteIP;
        private readonly string password;
        private TcpClient client;
        private NetworkStream stream;
        private bool isActivated, isConnected;
        private Point mouse;
        private byte[] headerBytes, dataBytes, bytesSent;

        public fClient(fConnection fParent, IPAddress rmIP, string pw)
        {
            InitializeComponent();
            formParent = fParent;
            remoteIP = rmIP;
            password = pw;
            isConnected = false;
            headerBytes = new byte[8];
        }

        private void fClient_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Run)).Start();
        }

        private void fClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (isConnected)
                {
                    isConnected = false;
                    dataBytes = Encoding.ASCII.GetBytes("Quit");
                    bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.checkConnection);
                    stream.Write(bytesSent, 0, bytesSent.Length);
                    Thread.Sleep(500);
                    stream.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            formParent.Show();
        }

        private void fClient_Activated(object sender, EventArgs e)
        {
            if (!isConnected)
                Close();
            isActivated = true;
        }

        private void fClient_Deactivate(object sender, EventArgs e)
        {
            if (!isConnected)
                Close();
            isActivated = false;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isConnected)
                Close();
            try
            {
                dataBytes = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.down, (ushort)e.KeyCode);
                bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isConnected)
                Close();
            try
            {
                dataBytes = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.up, (ushort)e.KeyCode);
                bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (isActivated)
                Cursor.Hide();
            else
                Cursor.Show();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            try
            {
                mouse = this.PointToClient(Cursor.Position);
                ushort x = (ushort)(mouse.X * 10000 / (this.Size.Width - 20));
                ushort y = (ushort)(mouse.Y * 10000 / (this.Size.Height - 40));
                dataBytes = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.move, x, y);
                bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            try
            {
                ushort x = Convert.ToUInt16(e.Button == MouseButtons.Left);
                ushort y = Convert.ToUInt16(e.Button == MouseButtons.Right);
                dataBytes = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.down, x, y);
                bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            try
            {
                ushort x = Convert.ToUInt16(e.Button == MouseButtons.Left);
                ushort y = Convert.ToUInt16(e.Button == MouseButtons.Right);
                dataBytes = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.up, x, y);
                bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
        }

// Hàm được gọi khi nhấn nút Connect bên phía Client host để kết nối với Server host
// Sau khi kết nối hoàn tất, hàm Authenticate() được gọi.
        internal int Connect()
        {
            try
            {
                // Khởi tọa kết nối TCP
                client = new TcpClient();

                // Kết nối với ip và port của Server host
                client.Connect(remoteIP, RemoteDesktop.port);

                // Sau khi kết nối thành công sẽ tạo ra một luồng (Stream) 
                // để đọc password và chuyển thành dữ liệu dạng byte để kiểm tra
                if (client.Connected)
                {
                    // Mã hóa password thành byte và gửi đến Server host
                    stream = client.GetStream();
                    dataBytes = Encoding.ASCII.GetBytes(password);
                    bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.checkConnection);
                    stream.Write(bytesSent, 0, bytesSent.Length);

                    if (Authenticate())
                        return 1;
                    else
                        return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            return -1;
        }

// Hàm Authenticate dùng để đọc và kiểm tra password
        private bool Authenticate()
        {
            try
            {
                dataBytes = RemoteDesktop.ReadExactly(stream, 4);
                if ((connectionStatus)BitConverter.ToInt32(dataBytes) == connectionStatus.success)
                    return (isConnected = true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            return false;
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
                    headerBytes = RemoteDesktop.ReadExactly(stream, headerBytes.Length);
                    type = (dataFormat)BitConverter.ToInt32(headerBytes, 0);
                    if (type == dataFormat.handle)
                    {
                        dblength = BitConverter.ToInt32(headerBytes, 4);
                        dataBytes = RemoteDesktop.ReadExactly(stream, dblength);
                        using (MemoryStream ms = new MemoryStream(dataBytes))
                        {
                            pictureBox.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        if (isConnected)
                        {
                            isConnected = false;
                            dataBytes = Encoding.ASCII.GetBytes("Quit/");
                            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.checkConnection);
                            stream.Write(bytesSent, 0, bytesSent.Length);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                isConnected = false;
            }
        }
    }
}
