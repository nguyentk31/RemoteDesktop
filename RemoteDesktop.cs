using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    internal enum dataFormat { checkConnection = 1, handle };

    internal enum connectionStatus { success = 123456, failure = 654321 };

    internal static class RemoteDesktop
    {
        internal static readonly int port = 2003, passwordLength = 8, headerLength = 1, authLength = 6;

        internal static IPAddress GetIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                if (ni.GetIPProperties().GatewayAddresses.Count > 0 && ni.OperationalStatus == OperationalStatus.Up)
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address;
            throw new Exception("Can't find IPv4!");
        }

        internal static string GeneratePassword(int pwLength)
        {
            string tp = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rnd = new Random();
            string pw = string.Empty;
            for (int i = 0; i < pwLength; i++)
                pw += tp[rnd.Next(0, 61)];
            return pw;
        }
        
        internal static Image Capture()
        {
            Rectangle bound = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy);
                CURSORINFO cursorInfo;
                cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                if (User32.GetCursorInfo(out cursorInfo))
                {
                    if (cursorInfo.flags == User32.CURSOR_SHOWING)
                    {
                        var iconPointer = User32.CopyIcon(cursorInfo.hCursor);
                        ICONINFO iconInfo;
                        int iconX, iconY;
                        if (User32.GetIconInfo(iconPointer, out iconInfo))
                        {
                            iconX = cursorInfo.ptScreenPos.x - ((int)iconInfo.xHotspot);
                            iconY = cursorInfo.ptScreenPos.y - ((int)iconInfo.yHotspot);
                            User32.DrawIcon(graphics.GetHdc(), iconX, iconY, cursorInfo.hCursor);
                            graphics.ReleaseHdc();
                        }
                    }
                }
            }
            return bitmap;
        }

        internal static byte[] CreateBytesSent(byte[] dataBytes, dataFormat type)
        {
            int dblength = dataBytes.Length;
            byte[] bytesSent = new byte[dblength + headerLength + authLength];
            byte[] HeaderByte = Encoding.ASCII.GetBytes(((int)type).ToString());
            byte[] AuthBytes = Encoding.ASCII.GetBytes(dblength.ToString());
            Buffer.BlockCopy(HeaderByte, 0, bytesSent, 0, HeaderByte.Length);
            Buffer.BlockCopy(AuthBytes, 0, bytesSent, headerLength, AuthBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, bytesSent, headerLength + authLength, dblength);
            return bytesSent;
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

        internal static byte[] ConvertInputToBytes(Input input)
        {
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(input, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        internal static Input ConvertBytesToInput(byte[] arr)
        {
            Input input = new Input();
            int size = Marshal.SizeOf(input);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(arr, 0, ptr, size);
                input = (Input)Marshal.PtrToStructure(ptr, input.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return input;
        }
    }
}