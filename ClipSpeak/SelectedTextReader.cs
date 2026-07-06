namespace ClipSpeak;

internal sealed class SelectedTextReader
{
    private const int ClipboardCopyTimeoutMilliseconds = 500;

    public string? TryGetSelectedText(IntPtr targetWindow = default)
    {
        try
        {
            ForegroundWindow.TryActivate(targetWindow);

            var previousClipboard = Clipboard.GetDataObject();
            try
            {
                var text = TryCopySelectionToClipboard(KeyboardInput.SendCopyShortcutWithSendKeys);
                if (text is not null)
                {
                    return text;
                }

                return TryCopySelectionToClipboard(KeyboardInput.SendCopyShortcut);
            }
            finally
            {
                TryRestoreClipboard(previousClipboard);
            }
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException or COMException)
        {
            return null;
        }
    }

    private static string? TryCopySelectionToClipboard(Action copySelection)
    {
        var clipboardSequence = GetClipboardSequenceNumber();
        copySelection();

        var deadline = Environment.TickCount64 + ClipboardCopyTimeoutMilliseconds;
        while (Environment.TickCount64 < deadline)
        {
            Application.DoEvents();

            if (GetClipboardSequenceNumber() != clipboardSequence && Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                var text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            Thread.Sleep(25);
        }

        return null;
    }

    private static void TryRestoreClipboard(IDataObject? previousClipboard)
    {
        try
        {
            if (previousClipboard is null)
            {
                Clipboard.Clear();
                return;
            }

            Clipboard.SetDataObject(previousClipboard, true);
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException or COMException)
        {
        }
    }

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
}
