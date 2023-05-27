using System.Runtime.InteropServices;

namespace RemoteDesktop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;
        public Int32 xHotspot;
        public Int32 yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public Int32 x;
        public Int32 y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
        
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }

    public struct Input
    {
        public int type;
        public InputUnion u;
    }

    [Flags]
    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }

    [Flags]
    public enum MouseEventF
    {
        Absolute = 0x8000,
        HWheel = 0x01000,
        Move = 0x0001,
        MoveNoCoalesce = 0x2000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        VirtualDesk = 0x4000,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100
    }

    internal static class User32
    {
        internal const Int32 CURSOR_SHOWING = 0x00000001;

        [DllImport("user32.dll")]
        internal static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        internal static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        internal static extern bool DrawIcon(IntPtr hdc, int x, int y, IntPtr hIcon);

        [DllImport("user32.dll")]
        internal static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetMessageExtraInfo();

        [DllImport("User32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

    }
}