using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RD_Server
{
    internal class ScreenCapture
    {
        public static Image CapturingScreen()
        {
            Rectangle bound = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy);
                User32.CURSORINFO cursorInfo;
                cursorInfo.cbSize = Marshal.SizeOf(typeof(User32.CURSORINFO));
                if (User32.GetCursorInfo(out cursorInfo))
                {
                    if (cursorInfo.flags == User32.CURSOR_SHOWING)
                    {
                        var iconPointer = User32.CopyIcon(cursorInfo.hCursor);
                        User32.ICONINFO iconInfo;
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
    }
}