namespace ClipSpeak;

internal sealed class GlobalMouseMenu : IDisposable
{
    private const int WhMouseLowLevel = 14;
    private const int WmRButtonDown = 0x0204;
    private const int WmRButtonUp = 0x0205;
    private const int VkControl = 0x11;
    private const short KeyPressed = unchecked((short)0x8000);

    private readonly LowLevelMouseProc _proc;
    private readonly Action<Point> _showMenu;
    private readonly SynchronizationContext? _syncContext;
    private IntPtr _hookId;
    private bool _disposed;
    private bool _suppressNextRightButtonUp;

    public bool Enabled { get; set; }

    public GlobalMouseMenu(Action<Point> showMenu)
    {
        _showMenu = showMenu;
        _syncContext = SynchronizationContext.Current;
        _proc = HookCallback;
        _hookId = SetHook(_proc);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        return SetWindowsHookEx(WhMouseLowLevel, proc, GetModuleHandle(null), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();
            if (message == WmRButtonDown && Enabled && IsControlPressed())
            {
                var data = Marshal.PtrToStructure<MsllHookStruct>(lParam);
                _suppressNextRightButtonUp = true;
                PostShowMenu(new Point(data.Point.X, data.Point.Y));
                return 1;
            }

            if (message == WmRButtonUp && _suppressNextRightButtonUp)
            {
                _suppressNextRightButtonUp = false;
                return 1;
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void PostShowMenu(Point location)
    {
        if (_syncContext is null)
        {
            _showMenu(location);
            return;
        }

        _syncContext.Post(_ => _showMenu(location), null);
    }

    private static bool IsControlPressed()
    {
        return (GetAsyncKeyState(VkControl) & KeyPressed) == KeyPressed;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _disposed = true;
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? moduleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct MsllHookStruct
    {
        public NativePoint Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }
}
