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
            timer = new System.Timers.Timer(100);
            timer.Elapsed += (sender, e) => SendImage();
            headerBytesRecv = new byte[8];
        }

        private void fServer_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Listen)).Start();
            tbIP.Text = localIP.ToString();
            tbPW.Text = password;
        }

        private void fServer_Activated(object sender, EventArgs e)
        {
            tbST.Text = status;
        }

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

        private void Monitor()
        {
            try
            {
                string infomation;
                int dblength;
                dataFormat type;
                while (true)
                {
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
