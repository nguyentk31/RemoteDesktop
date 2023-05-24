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

        private enum dataFor {Init = 1, Auth, Handle, Stop};
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private bool _isConnected;
        internal static bool _isOn = false;

        public Form2(TcpClient tcpClient, NetworkStream stream)
        {
            InitializeComponent();
            _tcpClient = tcpClient;
            _stream = stream;
            _isConnected = true;
            _isOn = true;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_isConnected)
                {
                    byte[] byteInfo = Encoding.ASCII.GetBytes("Quit");
                    byte[] byteSend = AddHeader(byteInfo, (int) dataFor.Stop);
                    _stream.Write(byteSend, 0, byteSend.Length);

                    Thread.Sleep(100);
                    _stream.Close();
                    _tcpClient.Close();
                    _isOn = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        
        private byte[] AddHeader(byte[] info, int type)
        {
            byte[] rt = new byte[info.Length + 1];
            byte[] header = Encoding.ASCII.GetBytes(type.ToString());
            Buffer.BlockCopy(header, 0, rt, 0, 1);
            Buffer.BlockCopy(info, 0, rt, 1, info.Length);
            return rt;
        }

        private async Task Run()
        {
            try
            {
                byte[] byteHead = new byte[1];
                byte[] byteInfo;
                byte[] byteInfoLength = new byte[10];
                int type;
                int infoLength;
                while (true)
                {
                    await _stream.ReadAsync(byteHead, 0, 1);
                    type = Convert.ToInt32(Encoding.ASCII.GetString(byteHead));

                    if (type == (int) dataFor.Handle)
                    {
                        await _stream.ReadAsync(byteInfoLength, 0, 10);

                        infoLength = Convert.ToInt32(Encoding.ASCII.GetString(byteInfoLength));
                        byteInfo = new byte[infoLength];
                        await _stream.ReadAsync(byteInfo, 0, infoLength);

                        using (MemoryStream ms = new MemoryStream(byteInfo))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        _isConnected = false;
                        if (type == (int) dataFor.Stop)
                            break;
                        else
                            throw new Exception("Connection Error!");
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
