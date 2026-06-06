namespace ClipSpeak;

internal sealed class HotkeyBox : TextBox
{
    private HotkeyDefinition _hotkey = new(Keys.None, HotkeyModifiers.None);

    public HotkeyDefinition Hotkey
    {
        get => _hotkey;
        set
        {
            _hotkey = value;
            Text = value.ToString();
        }
    }

    public HotkeyBox()
    {
        ReadOnly = true;
        ShortcutsEnabled = false;
        TabStop = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        SetHotkeyFromKeys(keyData);
        return true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        SetHotkeyFromKeys(e.KeyData);
        e.SuppressKeyPress = true;
        e.Handled = true;
        base.OnKeyDown(e);
    }

    private void SetHotkeyFromKeys(Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        if (key is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin)
        {
            key = Keys.None;
        }

        var modifiers = HotkeyModifiers.None;
        if ((keyData & Keys.Control) == Keys.Control) modifiers |= HotkeyModifiers.Control;
        if ((keyData & Keys.Alt) == Keys.Alt) modifiers |= HotkeyModifiers.Alt;
        if ((keyData & Keys.Shift) == Keys.Shift) modifiers |= HotkeyModifiers.Shift;

        Hotkey = new HotkeyDefinition(key, modifiers);
    }
}
