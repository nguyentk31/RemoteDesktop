using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RD_Client
{
    public partial class Form2 : Form
    {
        private TcpListener _tcpListenerRecvImg;
        private TcpClient _tcpClientRecvImg;
        private NetworkStream _streamRecvImg;
        private NetworkStream _streamConnect;

        private IPAddress _localIP;

        public Form2(NetworkStream ksStream)
        {
            InitializeComponent();
            _localIP = ipv4();
            _tcpListenerRecvImg = new TcpListener(_localIP, 2004);
            _streamConnect = ksStream;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            _tcpListenerRecvImg.Start();
            _tcpClientRecvImg = _tcpListenerRecvImg.AcceptTcpClient();
            _tcpListenerRecvImg.Stop();
            Task t = Stream();
        }

        private IPAddress ipv4()
        {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] list = iphe.AddressList;
            foreach (IPAddress ip in list)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
            throw new Exception("Không tìm thấy ipv4 trong hệ thống.");
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] byteMsg = Encoding.ASCII.GetBytes("Quit");
            _streamConnect.Write(byteMsg, 0, byteMsg.Length);

            _streamRecvImg.Close();
            _tcpClientRecvImg.Close();
        }

        private async Task Stream()
        {
            _streamRecvImg = _tcpClientRecvImg.GetStream();
            byte[] byteImg;
            int length;
            while (true)
            {
                length = 10;
                byteImg = new byte[length];
                await _streamRecvImg.ReadAsync(byteImg, 0, length);

                length = Convert.ToInt32(Encoding.ASCII.GetString(byteImg));
                byteImg = new byte[length];
                await _streamRecvImg.ReadAsync(byteImg, 0, length);

                using (MemoryStream ms = new MemoryStream(byteImg))
                {
                    pictureBox1.Image = Image.FromStream(ms);
                }
            }
        }
    }
}
