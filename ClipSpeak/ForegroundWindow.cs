namespace ClipSpeak;

internal static class ForegroundWindow
{
    public static IntPtr Current => GetForegroundWindow();

    public static void TryActivate(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero || windowHandle == Current)
        {
            return;
        }

        _ = SetForegroundWindow(windowHandle);
        Thread.Sleep(120);
        Application.DoEvents();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
