namespace ClipSpeak;

internal sealed class SelectedTextReader
{
    private const int CopyTimeoutMilliseconds = 700;
    private const int ClipboardPollIntervalMilliseconds = 50;

    public SelectedTextResult TryGetSelectedText()
    {
        System.Windows.Forms.IDataObject? previousClipboard = null;
        var clipboardRestored = true;

        try
        {
            previousClipboard = Clipboard.GetDataObject();
            Clipboard.Clear();
            KeyboardInput.SendCopyShortcut();

            var text = WaitForClipboardText();
            clipboardRestored = TryRestoreClipboard(previousClipboard);

            return string.IsNullOrWhiteSpace(text)
                ? new SelectedTextResult(null, clipboardRestored)
                : new SelectedTextResult(text, clipboardRestored);
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException)
        {
            clipboardRestored = TryRestoreClipboard(previousClipboard);
            return new SelectedTextResult(null, clipboardRestored);
        }
    }

    private static string? WaitForClipboardText()
    {
        var deadline = Environment.TickCount64 + CopyTimeoutMilliseconds;
        while (Environment.TickCount64 < deadline)
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    return Clipboard.GetText(TextDataFormat.UnicodeText);
                }
            }
            catch (Exception ex) when (ex is ExternalException or ThreadStateException)
            {
                return null;
            }

            Thread.Sleep(ClipboardPollIntervalMilliseconds);
            Application.DoEvents();
        }

        return null;
    }

    private static bool TryRestoreClipboard(System.Windows.Forms.IDataObject? previousClipboard)
    {
        try
        {
            if (previousClipboard is null)
            {
                Clipboard.Clear();
            }
            else
            {
                Clipboard.SetDataObject(previousClipboard, copy: true);
            }

            return true;
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException)
        {
            return false;
        }
    }
}

internal sealed record SelectedTextResult(string? Text, bool ClipboardRestored);
