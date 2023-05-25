using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RD_Client
{
    public partial class Client : Form
    {
        private enum dataFormat { checkConnection = 1, Handle };
        private readonly IPAddress remoteIP;
        private readonly int remotePort;
        private readonly string password;
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        internal static bool isOn;

        public Client(IPAddress _remoteIP, int _remotePort, string _password)
        {
            InitializeComponent();
            remoteIP = _remoteIP;
            remotePort = _remotePort;
            password = _password;
            isOn = isConnected = false;
        }

        private void Client_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(Run)).Start();
            isOn = true;
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                isOn = false;
                if (isConnected)
                {
                    byte[] bytesData = Encoding.ASCII.GetBytes("Quit");
                    byte[] bytesSend = CreateBytesSend(bytesData, dataFormat.checkConnection);
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
        }

        private byte[] CreateBytesSend(byte[] bytesData, dataFormat type)
        {
            int bdlength = bytesData.Length;
            byte[] bytesSend = new byte[bdlength + 7];
            byte[] byteHeader = Encoding.ASCII.GetBytes(((int)type).ToString());
            byte[] bytesLength = Encoding.ASCII.GetBytes(bdlength.ToString());
            Buffer.BlockCopy(byteHeader, 0, bytesSend, 0, 1);
            Buffer.BlockCopy(bytesLength, 0, bytesSend, 1, bytesLength.Length);
            Buffer.BlockCopy(bytesData, 0, bytesSend, 7, bdlength);
            return bytesSend;
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
                    byte[] bytesData = Encoding.ASCII.GetBytes(password);
                    byte[] bytesSend = CreateBytesSend(bytesData, dataFormat.checkConnection);
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
                byte[] bytesAuth = new byte[6];
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
                byte[] byteHeader = new byte[1], bytesLength = new byte[6], bytesData, bytesSend;
                int bdlength;
                dataFormat type;
                while (true)
                {
                    stream.Read(byteHeader, 0, 1);
                    type = (dataFormat)Convert.ToInt32(Encoding.ASCII.GetString(byteHeader));
                    if (type == dataFormat.Handle)
                    {
                        stream.Read(bytesLength, 0, 6);
                        bdlength = Convert.ToInt32(Encoding.ASCII.GetString(bytesLength));
                        bytesData = ExactStream.ReadExactly(stream, bdlength);
                        using (MemoryStream ms = new MemoryStream(bytesData))
                        {
                            pictureBox.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        isConnected = false;
                        if (isOn)
                        {
                            bytesData = Encoding.ASCII.GetBytes("Quit");
                            bytesSend = CreateBytesSend(bytesData, dataFormat.checkConnection);
                            stream.Write(bytesSend, 0, bytesSend.Length);
                        }
                        break;
                    }
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }
    }
}
