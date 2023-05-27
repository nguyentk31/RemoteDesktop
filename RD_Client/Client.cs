using RemoteDesktop;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RD_Client
{
    public partial class Client : Form
    {
        private readonly IPAddress remoteIP;
        private readonly int remotePort;
        private readonly string password;
        private TcpClient client;
        private NetworkStream stream;
        private bool isActivated, isConnected;
        private Point mouse;
        private byte[] byteHeader, bytesAuth, bytesLength, bytesData, bytesSend;
        private Form1 formParent;

        public Client(IPAddress _remoteIP, int _remotePort, string _password, Form1 fParent)
        {
            InitializeComponent();
            formParent = fParent;
            remoteIP = _remoteIP;
            remotePort = _remotePort;
            password = _password;
            isConnected = false;
            byteHeader = new byte[1];
            bytesAuth = bytesLength = new byte[6];
        }

        private void Client_Load(object sender, EventArgs e)
        {
            formParent.Hide();
            new Thread(new ThreadStart(Run)).Start();
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (isConnected)
                {
                    isConnected = false;
                    byte[] bytesData = Encoding.ASCII.GetBytes("Quit");
                    byte[] bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.checkConnection);
                    stream.Write(bytesSend, 0, bytesSend.Length);
                    Thread.Sleep(100);
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

        private void Client_Activated(object sender, EventArgs e)
        {
            if (!isConnected)
                Close();
            Cursor.Hide();
            isActivated = true;
        }

        private void Client_Deactivate(object sender, EventArgs e)
        {
            Cursor.Show();
            isActivated = false;
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
                        dx = mouse.X * 100 / (this.Size.Width - 25),
                        dy = mouse.Y * 100 / (this.Size.Height - 50),
                        dwFlags = (uint)MouseEventF.Absolute,
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
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
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
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
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
        }

        private void pictureBox_MouseWheel(object? sender, MouseEventArgs e)
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
                        mouseData = (uint)e.Delta,
                        dwFlags = (uint)MouseEventF.Wheel,
                        dwExtraInfo = User32.GetMessageExtraInfo()
                    }
                }
            };
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
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
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
        }

        private void tb_KeyUp(object sender, KeyEventArgs e)
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
            bytesData = RDFunctions.ConvertInputToBytes(input);
            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
            stream.Write(bytesSend, 0, bytesSend.Length);
        }

        public int Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(remoteIP, remotePort);
                if (client.Connected)
                {
                    stream = client.GetStream();
                    bytesData = Encoding.ASCII.GetBytes(password);
                    bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.checkConnection);
                    stream.Write(bytesSend, 0, bytesSend.Length);
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
                stream.Read(bytesAuth, 0, 6);
                string information = Encoding.ASCII.GetString(bytesAuth);
                if (information == "123456")
                {
                    isConnected = true;
                    return true;
                }
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
                int bdlength;
                dataFormat type;
                while (true)
                {
                    stream.Read(byteHeader, 0, 1);
                    type = (dataFormat)Convert.ToInt32(Encoding.ASCII.GetString(byteHeader));
                    if (type == dataFormat.handle)
                    {
                        stream.Read(bytesLength, 0, 6);
                        bdlength = Convert.ToInt32(Encoding.ASCII.GetString(bytesLength));
                        bytesData = RDFunctions.ReadExactly(stream, bdlength);
                        using (MemoryStream ms = new MemoryStream(bytesData))
                        {
                            pictureBox.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        if (isConnected)
                        {
                            isConnected = false;
                            bytesData = Encoding.ASCII.GetBytes("Quit");
                            bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.checkConnection);
                            stream.Write(bytesSend, 0, bytesSend.Length);
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
