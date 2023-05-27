using RemoteDesktop;
using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RD_Server
{
    internal class Server
    {
        private readonly int passwordLength;
        public string password {get;}
        public IPAddress localIP {get;}
        private readonly int localPort;
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected, isOn;
        private System.Timers.Timer timer;
        private byte[] byteHeader, bytesLength, bytesData, bytesSend;

        public Server()
        {
            localIP = GetIPv4();
            localPort = 2003;
            passwordLength = 8;
            password = GeneratePassword(passwordLength);
            byteHeader = new byte[1];
            bytesLength = new byte[6];
            isConnected = false;
            isOn = true;
            timer = new System.Timers.Timer(100);
            timer.Elapsed += async (sender, e) => SendImage();
        }

        // Lay IPv4 cua server
        private IPAddress GetIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                if (ni.GetIPProperties().GatewayAddresses.Count > 0 && ni.OperationalStatus == OperationalStatus.Up)
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address;
            throw new Exception("Can't find IPv4!");
        }

        private string GeneratePassword(int pwLength)
        {
            Random rnd = new Random();
            string pw = string.Empty;
            for (int i = 0; i < pwLength; i++)
                pw += (char)rnd.Next(48, 122);
            return pw;
        }

        public void StartListening()
        {
            try
            {
                listener = new TcpListener(localIP, localPort);
                listener.Start();
                while (isOn)
                {
                    if (!listener.Pending())
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        client = listener.AcceptTcpClient();
                        listener.Stop();
                        isConnected = true;
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
                stream = client.GetStream();
                string infomation;
                int bdlength;
                dataFormat type;
                while (isConnected)
                {
                    stream.Read(byteHeader, 0, 1);
                    type = (dataFormat)Convert.ToInt32(Encoding.ASCII.GetString(byteHeader));
                    stream.Read(bytesLength, 0, 6);
                    bdlength = Convert.ToInt32(Encoding.ASCII.GetString(bytesLength));
                    bytesData = RDFunctions.ReadExactly(stream, bdlength);
                    if (type == dataFormat.handle)
                    {
                        Input[] inputs = new Input[] {RDFunctions.ConvertBytesToInput(bytesData)};
                        if (inputs[0].type == (int)InputType.Mouse)
                        {
                            if (inputs[0].u.mi.dwFlags == (uint)MouseEventF.Absolute)
                                User32.SetCursorPos(inputs[0].u.mi.dx * Screen.PrimaryScreen.Bounds.Width / 100, inputs[0].u.mi.dy * Screen.PrimaryScreen.Bounds.Height / 100);
                        }
                        else
                            User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                    }
                    else
                    {
                        if (bdlength == passwordLength)
                        {
                            infomation = Encoding.ASCII.GetString(bytesData);
                            if (infomation == password)
                            {
                                bytesSend = Encoding.ASCII.GetBytes("123456");
                                stream.Write(bytesSend, 0, bytesSend.Length);
                                Thread.Sleep(100);
                                timer.Start();
                            }
                            else
                            {
                                bytesSend = Encoding.ASCII.GetBytes("654321");
                                stream.Write(bytesSend, 0, bytesSend.Length);
                                break;
                            }
                        }
                        else
                        {
                            timer.Stop();
                            if (isConnected)
                            {
                                bytesData = Encoding.ASCII.GetBytes("Quit");
                                bytesSend = RDFunctions.CreateBytesSend(bytesData, type);
                                stream.Write(bytesSend, 0, bytesSend.Length);
                            }
                            break;
                        }
                    }
                }
                isConnected = false;
                new Thread(new ThreadStart(StartListening)).Start();
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
                Image screen = RDFunctions.Capture();
                using (MemoryStream ms = new MemoryStream())
                {
                    screen.Save(ms, ImageFormat.Jpeg);
                    byte[] bytesData = ms.ToArray();
                    byte[] bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.handle);
                    stream.Write(bytesSend, 0, bytesSend.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        public void CloseServer()
        {
            try
            {
                isOn = false;
                if (isConnected)
                {
                    timer.Stop();
                    byte[] bytesData = Encoding.ASCII.GetBytes("Quit");
                    byte[] bytesSend = RDFunctions.CreateBytesSend(bytesData, dataFormat.checkConnection);
                    stream.Write(bytesSend, 0, bytesSend.Length);
                    Thread.Sleep(100);
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
        }
    }
}