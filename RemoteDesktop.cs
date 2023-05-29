using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteDesktop
{
    internal enum dataFormat { checkConnection = 1, handle }
    internal enum connectionStatus { success = 123456, failure = 654321 }
    internal enum inputType { mouse, key }
    internal enum inputEvent { up, down, move }
    internal delegate void ConnectionChangedEvent(string message);
    
    internal static class RemoteDesktop
    {
        internal static readonly int port = 2003;

        internal static IPAddress GetIPv4()
        {
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

        internal static string GeneratePassword(int pwLength)
        {
            string tp = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rnd = new Random();
            string pw = string.Empty;
            for (int i = 0; i < pwLength; i++)
                pw += tp[rnd.Next(0, 35)];
            return pw;
        }
        
        internal static Image Capture()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            return null;
        }

        internal static void SendImage(NetworkStream stream)
        {
            try
            {
                Image screen = RemoteDesktop.Capture();
                using (MemoryStream ms = new MemoryStream())
                {
                    screen.Save(ms, ImageFormat.Jpeg);
                    byte[] dataBytesSent = ms.ToArray();
                    RemoteDesktop.SendDataBytes(dataBytesSent, dataFormat.handle, stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: of type {ex.GetType().Name}.\nMessage: {ex.Message}");
            }
        }

        internal static byte[] CreateInputBytes(ushort iType, ushort iEvent, ushort iInfor1, ushort iInfor2 = 0)
        {
            byte[] inputBytes = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(iType), 0, inputBytes, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(iEvent), 0, inputBytes, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(iInfor1), 0, inputBytes, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(iInfor2), 0, inputBytes, 6, 2);
            return inputBytes;
        }

        internal static Input[] HandleInputBytes(byte[] iBytes)
        {
            Input input = new Input();
            input.u.ki.dwExtraInfo = User32.GetMessageExtraInfo();
            try
            {
                ushort iType = BitConverter.ToUInt16(iBytes, 0),
                    iEvent = BitConverter.ToUInt16(iBytes, 2),
                    iInfor1 = BitConverter.ToUInt16(iBytes, 4),
                    iInfor2 = BitConverter.ToUInt16(iBytes, 6);  
                if (iType == (ushort)InputType.Keyboard)
                {
                    input.type = (int)InputType.Keyboard;
                    input.u.ki.wVk = iInfor1;
                    if (iEvent == (ushort)inputEvent.down)
                        input.u.ki.dwFlags = (uint)KeyEventF.KeyDown;
                    else
                        input.u.ki.dwFlags = (uint)KeyEventF.KeyUp;
                }
                else
                {
                    input.type = (int)InputType.Mouse;
                    if (iEvent == (ushort)inputEvent.move)
                    {
                        int x = (int)iInfor1 * Screen.PrimaryScreen.Bounds.Width / 10000;
                        int y = (int)iInfor2 * Screen.PrimaryScreen.Bounds.Height / 10000;
                        input.u.mi.dwFlags = (uint)MouseEventF.Absolute;
                        User32.SetCursorPos(x, y);
                    }
                    else if (iEvent == (ushort)inputEvent.down)
                    {
                        if (iInfor1 == 1)
                            input.u.mi.dwFlags = (uint)MouseEventF.LeftDown;
                        else if (iInfor2 == 1)
                            input.u.mi.dwFlags = (uint)MouseEventF.RightDown;
                        else
                            input.u.mi.dwFlags = (uint)MouseEventF.MiddleDown;
                    }
                    else
                    {
                        if (iInfor1 == 1)
                            input.u.mi.dwFlags = (uint)MouseEventF.LeftUp;
                        else if (iInfor2 == 1)
                            input.u.mi.dwFlags = (uint)MouseEventF.RightUp;
                        else
                            input.u.mi.dwFlags = (uint)MouseEventF.MiddleUp;
                    }
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception of type: {ex.GetType().Name}.\nMessage: {ex.Message}.");
            }
            return new Input[] {input};
        }

        internal static void SendDataBytes(byte[] dataBytes, dataFormat type, NetworkStream stream)
        {
            try
            {
                int dblength = dataBytes.Length;
                byte[] bytesSent = new byte[dblength + 8];
                Buffer.BlockCopy(BitConverter.GetBytes((int)type), 0, bytesSent, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(dblength), 0, bytesSent, 4, 4);
                Buffer.BlockCopy(dataBytes, 0, bytesSent, 8, dblength);
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