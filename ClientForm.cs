using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fClient : Form
    {
        private static fConnection formParent;
        private static IPAddress remoteIP;
        private static string password;
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool isActivated, isConnected;
        private static Point mouse;
        private static byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;

        internal fClient(fConnection fParent, IPAddress rmIP, string pw)
        {
            InitializeComponent();
            formParent = fParent;
            remoteIP = rmIP;
            password = pw;
            isConnected = false;
            headerBytesRecv = new byte[8];
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
                    dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
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
            Cursor.Show();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isConnected)
                Close();
            try
            {
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.down, (ushort)e.KeyCode);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
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
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.up, (ushort)e.KeyCode);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
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
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.move, x, y);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);

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
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.down, x, y);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
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
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.mouse, (ushort)inputEvent.up, x, y);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
                Close();
            }
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
                    dataBytesSent = Encoding.ASCII.GetBytes(password);
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
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
                dataBytesRecv = RemoteDesktop.ReadExactly(stream, 12);
                if ((connectionStatus)BitConverter.ToInt32(dataBytesRecv, 8) == connectionStatus.success)
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
                    headerBytesRecv = RemoteDesktop.ReadExactly(stream, headerBytesRecv.Length);
                    type = (dataFormat)BitConverter.ToInt32(headerBytesRecv, 0);
                    if (type == dataFormat.handle)
                    {
                        dblength = BitConverter.ToInt32(headerBytesRecv, 4);
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
                            dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
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
