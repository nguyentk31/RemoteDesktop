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

        private byte[] headerByte, authBytes, dataBytes, bytesSent;

        public fClient(fConnection fParent, IPAddress rmIP, string pw)
        {
            InitializeComponent();
            formParent = fParent;

            remoteIP = rmIP;
            password = pw;

            isConnected = false;

            headerByte = new byte[RemoteDesktop.headerLength];
            authBytes = new byte[RemoteDesktop.authLength];
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
                    Thread.Sleep(5000);
                    stream.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            finally
            {
                formParent.Show();
            }
        }

        private void fClient_Activated(object sender, EventArgs e)
        {
            if (!isConnected)
                Close();
            Cursor.Hide();
            isActivated = true;
        }

        private void fClient_Deactivate(object sender, EventArgs e)
        {
            if (!isConnected)
                Close();
            Cursor.Show();
            isActivated = false;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isConnected)
                Close();
            Input input = new Input
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = ((ushort)e.KeyCode),
                        dwFlags = (uint)(KeyEventF.KeyDown),
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            dataBytes = RemoteDesktop.ConvertInputToBytes(input);
            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
            stream.Write(bytesSent, 0, bytesSent.Length);
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isConnected)
                Close();
            Input input = new Input
            {
                type = (int)InputType.Keyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = ((ushort)e.KeyCode),
                        dwFlags = (uint)(KeyEventF.KeyUp),
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            dataBytes = RemoteDesktop.ConvertInputToBytes(input);
            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
            stream.Write(bytesSent, 0, bytesSent.Length);

        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            mouse = this.PointToClient(Cursor.Position);
            Input input = new Input
            {
                type = (int)InputType.Mouse,
                u = new InputUnion
                {
                    mi = new MouseInput
                    {
                        dx = mouse.X * 10000 / (this.Size.Width - 25),
                        dy = mouse.Y * 10000 / (this.Size.Height - 50),
                        dwFlags = (uint)MouseEventF.Absolute,
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            dataBytes = RemoteDesktop.ConvertInputToBytes(input);
            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
            stream.Write(bytesSent, 0, bytesSent.Length);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            Input input = new Input
            {
                type = (int)InputType.Mouse,
                u = new InputUnion
                {
                    mi = new MouseInput
                    {
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            if (e.Button == MouseButtons.Right)
                input.u.mi.dwFlags = (uint)MouseEventF.RightDown;
            else if (e.Button == MouseButtons.Left)
                input.u.mi.dwFlags = (uint)MouseEventF.LeftDown;
            else
                input.u.mi.dwFlags = (uint)MouseEventF.MiddleDown;
            dataBytes = RemoteDesktop.ConvertInputToBytes(input);
            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
            stream.Write(bytesSent, 0, bytesSent.Length);
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isConnected)
                Close();
            if (!isActivated)
                return;
            Input input = new Input
            {
                type = (int)InputType.Mouse,
                u = new InputUnion
                {
                    mi = new MouseInput
                    {
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            if (e.Button == MouseButtons.Right)
                input.u.mi.dwFlags = (uint)MouseEventF.RightUp;
            else if (e.Button == MouseButtons.Left)
                input.u.mi.dwFlags = (uint)MouseEventF.LeftUp;
            else
                input.u.mi.dwFlags = (uint)MouseEventF.MiddleUp;
            dataBytes = RemoteDesktop.ConvertInputToBytes(input);
            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
            stream.Write(bytesSent, 0, bytesSent.Length);
        }

        internal int Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(remoteIP, RemoteDesktop.port);
                if (client.Connected)
                {
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

        private bool Authenticate()
        {
            try
            {
                authBytes = RemoteDesktop.ReadExactly(stream, RemoteDesktop.authLength);
                if ((connectionStatus)Convert.ToInt32(Encoding.ASCII.GetString(authBytes)) == connectionStatus.success)
                    return (isConnected = true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            return false;
        }

        private void Run()
        {
            try
            {
                int dblength;
                dataFormat type;
                while (true)
                {
                    headerByte = RemoteDesktop.ReadExactly(stream, RemoteDesktop.headerLength);
                    type = (dataFormat)Convert.ToInt32(Encoding.ASCII.GetString(headerByte));
                    if (type == dataFormat.handle)
                    {
                        authBytes = RemoteDesktop.ReadExactly(stream, RemoteDesktop.authLength);
                        dblength = Convert.ToInt32(Encoding.ASCII.GetString(authBytes));
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
                            dataBytes = Encoding.ASCII.GetBytes("Quit");
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
            }
        }
    }
}
