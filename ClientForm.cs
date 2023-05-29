using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteDesktop
{
    internal partial class fClient : Form
    {
        private IPAddress remoteIP;
        private string password;
        private TcpClient client;
        private NetworkStream stream;
        private bool isActivated, isConnected, isMouseShow;
        private Point mouse;
        private byte[] headerBytesRecv, dataBytesRecv, dataBytesSent;
        private event ConnectionChangedEvent connectionClosed;

        internal fClient(IPAddress rmIP, string pw)
        {
            InitializeComponent();
            remoteIP = rmIP;
            password = pw;
            isConnected = false;
            isMouseShow = true;
            headerBytesRecv = new byte[8];
            connectionClosed += CloseForm;
        }

        private void CloseForm(string msg)
        {
            Close();
        }

        private void fClient_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Run)).Start();
        }

        private void fClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (!isMouseShow)
                    Cursor.Show();
                if (isConnected)
                {
                    isConnected = false;
                    dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        private void fClient_Activated(object sender, EventArgs e)
        {
            isActivated = true;
        }

        private void fClient_Deactivate(object sender, EventArgs e)
        {
            isActivated = false;
            if (!isMouseShow)
            {
                isMouseShow = false;
                Cursor.Show();
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.down, (ushort)e.KeyCode);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                dataBytesSent = RemoteDesktop.CreateInputBytes((ushort)inputType.key, (ushort)inputEvent.up, (ushort)e.KeyCode);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (isActivated && isMouseShow)
            {
                isMouseShow = false;
                Cursor.Hide();
            }
        }

        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (isActivated && !isMouseShow)
            {
                isMouseShow = true;
                Cursor.Show();
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
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
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
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
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
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
            }
        }

        internal int Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(remoteIP, RemoteDesktop.port);
                if (!client.Connected)
                    throw new Exception();
                stream = client.GetStream();
                dataBytesSent = Encoding.ASCII.GetBytes(password);
                RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                if (Authenticate())
                    return 1;
                stream.Close();
                client.Close();
                return 0;
            }
            catch
            {
                stream.Close(5000);
                client.Close();
                return -1;
            }
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
                            connectionClosed?.Invoke("quit");
                            dataBytesSent = Encoding.ASCII.GetBytes("/Quit/");
                            RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.checkConnection, stream);
                        }
                        else
                        {
                            stream.Close();
                            client.Close();
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
