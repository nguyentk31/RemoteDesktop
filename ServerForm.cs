using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    public partial class fServer : Form
    {
        Form1 formParent;
        private readonly IPAddress localIP;
        private readonly string password;
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected, isRunning;
        private System.Timers.Timer timer;
        private byte[] headerByte, authBytes, dataBytes, bytesSent;

        public fServer(Form1 fParent)
        {
            InitializeComponent();
            formParent = fParent;
            localIP = RemoteDesktop.GetIPv4();
            password = RemoteDesktop.GeneratePassword(RemoteDesktop.passwordLength);
            isConnected = false;
            isRunning = true;
            timer = new System.Timers.Timer(100);
            timer.Elapsed += (sender, e) => SendImage();
            headerByte = new byte[RemoteDesktop.headerLength];
            authBytes = new byte[RemoteDesktop.authLength];
        }

        private void fServer_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Listen)).Start();
            tbIP.Text = localIP.ToString();
            tbPW.Text = password;
        }

        private void fServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                isRunning = false;
                if (isConnected)
                {
                    timer.Stop();
                    dataBytes = Encoding.ASCII.GetBytes("Quit");
                    bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.checkConnection);
                    stream.Write(bytesSent, 0, bytesSent.Length);
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
                tbST.Text = "SERVER IS LISTENING.";
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
                    headerByte = RemoteDesktop.ReadExactly(stream, RemoteDesktop.headerLength);
                    type = (dataFormat)Convert.ToInt32(Encoding.ASCII.GetString(headerByte));
                    authBytes = RemoteDesktop.ReadExactly(stream, RemoteDesktop.authLength);
                    dblength = Convert.ToInt32(Encoding.ASCII.GetString(authBytes));
                    dataBytes = RemoteDesktop.ReadExactly(stream, dblength);
                    if (type == dataFormat.handle)
                    {
                        Input[] inputs = RemoteDesktop.HandleInputBytes(dataBytes);
                        if (inputs[0].u.mi.dwFlags == (uint)MouseEventF.Absolute)
                            continue;
                        User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }
                    else if (dblength == RemoteDesktop.passwordLength)
                    {
                        infomation = Encoding.ASCII.GetString(dataBytes);
                        if (infomation == password)
                        {
                            tbST.Text = "SERVER IS CONNECTED.";
                            bytesSent = Encoding.ASCII.GetBytes(((int)connectionStatus.success).ToString());
                            stream.Write(bytesSent, 0, bytesSent.Length);
                            timer.Start();
                        }
                        else
                        {
                            bytesSent = Encoding.ASCII.GetBytes(((int)connectionStatus.failure).ToString());
                            stream.Write(bytesSent, 0, bytesSent.Length);
                            break;
                        }
                    }
                    else
                    {
                        timer.Stop();
                        if (isConnected)
                        {
                            dataBytes = Encoding.ASCII.GetBytes("Quit");
                            bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, type);
                            stream.Write(bytesSent, 0, bytesSent.Length);
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
                    dataBytes = ms.ToArray();
                    bytesSent = RemoteDesktop.CreateBytesSent(dataBytes, dataFormat.handle);
                    stream.Write(bytesSent, 0, bytesSent.Length);
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
