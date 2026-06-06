namespace ClipSpeak;

[Flags]
internal enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}

internal sealed record HotkeyDefinition(Keys Key, HotkeyModifiers Modifiers)
{
    public bool IsValid => Key != Keys.None && Modifiers != HotkeyModifiers.None;

    public override string ToString()
    {
        if (!IsValid)
        {
            return "Unassigned";
        }

        var parts = new List<string>();
        if (Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(HotkeyModifiers.Windows)) parts.Add("Win");
        parts.Add(KeyToDisplayString(Key));
        return string.Join(" + ", parts);
    }

    private static string KeyToDisplayString(Keys key) => key switch
    {
        Keys.Oemtilde => "`",
        Keys.OemMinus => "-",
        Keys.Oemplus => "=",
        Keys.OemOpenBrackets => "[",
        Keys.OemCloseBrackets => "]",
        Keys.OemPipe => "\\",
        Keys.OemSemicolon => ";",
        Keys.OemQuotes => "'",
        Keys.Oemcomma => ",",
        Keys.OemPeriod => ".",
        Keys.OemQuestion => "/",
        _ => key.ToString()
    };
}
