using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RemoteDesktop
{
    internal enum dataFormat { checkConnection, handle }
    internal enum connectionStatus { success = 12345, failure = 54321 }
    internal enum inputType { mouse, key }
    internal enum inputEvent { up, down, move, wheel }
    
    internal static class RemoteDesktop
    {
        internal static readonly int port = 2003;

// Lấy địa chỉ IPv4 của Server host
        internal static IPAddress GetIPv4()
        {
            // Hàm sẽ kiểm tra tất cả các network interface mà Server kết nối
            // và lấy ra địa chỉ IP của LAN hoặc VPN
            // với điệu kiện là có địa chỉ Gateway và có trạng thái đang bật
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                if (ni.GetIPProperties().GatewayAddresses.Count > 0 && ni.OperationalStatus == OperationalStatus.Up)
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address;
            throw new Exception("Can't find IPv4!");
        }

        internal static bool ValidateIPv4(string ip)
        {
            if (String.IsNullOrEmpty(ip))
                return false;
            string[] nums = ip.Split('.');
            return nums.Length == 4 && nums.All(x => byte.TryParse(x, out _));
        }

// Tạo mật khẩu ngẫu nhiên cho Server Host
        internal static string GeneratePassword(int pwLength)
        {
            string tp = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random rnd = new Random();
            string pw = string.Empty;
            for (int i = 0; i < pwLength; i++)
                pw += tp[rnd.Next(0, 35)];
            return pw;
        }

        internal static void SendDataBytes(byte[] dataBytes, dataFormat type, NetworkStream stream)
        {
            try
            {
                int dblength = dataBytes.Length;
                byte[] bytesSent = new byte[dblength + 6];
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)type), 0, bytesSent, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(dblength), 0, bytesSent, 2, 4);
                Buffer.BlockCopy(dataBytes, 0, bytesSent, 6, dblength);
                stream.Write(bytesSent, 0, bytesSent.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
        }

        internal static byte[] ReadExactly(NetworkStream stream, int length)
        {
            byte[] bytesReceived = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(bytesReceived, offset, length - offset);
                if (read == 0)
                    throw new System.IO.EndOfStreamException();
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == length);
            return bytesReceived;
        }
    }
}