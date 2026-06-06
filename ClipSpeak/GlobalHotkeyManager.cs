using System.Runtime.InteropServices;

namespace ClipSpeak;

internal sealed class GlobalHotkeyManager : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private int _nextId = 1;
    private bool _disposed;
    private readonly Dictionary<int, Action> _actions = [];

    public GlobalHotkeyManager()
    {
        CreateHandle(new CreateParams());
    }

    public bool Register(HotkeyDefinition hotkey, Action action, out string? error)
    {
        error = null;
        if (!hotkey.IsValid)
        {
            error = "Choose at least one modifier and a key.";
            return false;
        }

        var id = _nextId++;
        if (!RegisterHotKey(Handle, id, (uint)hotkey.Modifiers, (uint)hotkey.Key))
        {
            error = $"Could not register {hotkey}. Another app may already use it.";
            return false;
        }

        _actions[id] = action;
        return true;
    }

    public void Clear()
    {
        foreach (var id in _actions.Keys.ToArray())
        {
            UnregisterHotKey(Handle, id);
        }

        _actions.Clear();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && _actions.TryGetValue(m.WParam.ToInt32(), out var action))
        {
            try
            {
                action();
            }
            catch
            {
                // Keep the tray app alive if a hotkey action fails unexpectedly.
            }

            return;
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Clear();
        DestroyHandle();
        _disposed = true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
